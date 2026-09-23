using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pufzi.Contracts.Enums;
using Pufzi.Contracts.Requests.Team;
using Pufzi.Contracts.Responses.Team;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;
using Pufzi.Infrastructure.Configuration;
using Pufzi.Infrastructure.Email;
using Pufzi.Infrastructure.Security;
using Pufzi.Services.Businesses;

namespace Pufzi.Services.Team;

public class TeamInvitationService : ITeamInvitationService
{
    private static readonly TimeSpan InvitationLifetime = TimeSpan.FromDays(7);

    private readonly PufziDbContext _dbContext;
    private readonly IBusinessAccessService _businessAccess;
    private readonly ISecureTokenService _secureTokenService;
    private readonly IEmailService _emailService;
    private readonly FrontendOptions _frontendOptions;

    public TeamInvitationService(
        PufziDbContext dbContext,
        IBusinessAccessService businessAccess,
        ISecureTokenService secureTokenService,
        IEmailService emailService,
        IOptions<FrontendOptions> frontendOptions)
    {
        _dbContext = dbContext;
        _businessAccess = businessAccess;
        _secureTokenService = secureTokenService;
        _emailService = emailService;
        _frontendOptions = frontendOptions.Value;
    }

    public async Task<IReadOnlyCollection<BusinessInvitationResponse>> GetAllAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var invitations = await _dbContext.BusinessInvitations
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return invitations
            .Select(MapResponse)
            .ToList();
    }

    public async Task<BusinessInvitationResponse> CreateAsync(
        Guid userId,
        Guid businessId,
        CreateBusinessInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var role = MapRole(request.Role);
        var email = NormalizeEmail(request.Email);
        var now = DateTime.UtcNow;

        var business = await _dbContext.Businesses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == businessId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Salonul nu a fost găsit.");

        var existingUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Email.ToLower() == email,
                cancellationToken);

        if (existingUser is not null)
        {
            var existingMembership = await _dbContext.BusinessMemberships
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.BusinessId == businessId &&
                        x.UserId == existingUser.Id,
                    cancellationToken);

            if (existingMembership)
            {
                throw new InvalidOperationException(
                    "Această persoană este deja membră a echipei.");
            }
        }

        var activeInvitationExists =
            await _dbContext.BusinessInvitations
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.BusinessId == businessId &&
                        x.Email.ToLower() == email &&
                        x.AcceptedAt == null &&
                        x.RevokedAt == null &&
                        x.ExpiresAt > now,
                    cancellationToken);

        if (activeInvitationExists)
        {
            throw new InvalidOperationException(
                "Există deja o invitație activă pentru această adresă de email.");
        }

        var token = _secureTokenService.GenerateToken();
        var tokenHash = _secureTokenService.HashToken(token);

        var invitation = new BusinessInvitation
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            InvitedByUserId = userId,
            Email = email,
            Role = role,
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAt = now.Add(InvitationLifetime)
        };

        _dbContext.BusinessInvitations.Add(invitation);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var invitationUrl =
            $"{_frontendOptions.WebUrl.TrimEnd('/')}" +
            $"/invite?token={Uri.EscapeDataString(token)}";

        try
        {
            await _emailService.SendAsync(
                email,
                $"🐾 {business.Name} te-a invitat în echipa sa pe Pufzi",
                BuildInvitationEmail(
                    business.Name,
                    role,
                    invitationUrl,
                    invitation.ExpiresAt),
                cancellationToken);
        }
        catch
        {
            _dbContext.BusinessInvitations.Remove(invitation);

            await _dbContext.SaveChangesAsync(
                CancellationToken.None);

            throw;
        }

        return MapResponse(invitation);
    }

    public async Task<BusinessInvitationDetailsResponse> ValidateAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Tokenul invitației este invalid.");
        }

        var tokenHash = _secureTokenService.HashToken(token);
        var now = DateTime.UtcNow;

        var invitation = await _dbContext.BusinessInvitations
            .AsNoTracking()
            .Include(x => x.Business)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Invitația nu a fost găsită.");

        EnsureInvitationCanBeUsed(invitation, now);

        return new BusinessInvitationDetailsResponse
        {
            BusinessName = invitation.Business.Name,
            Email = invitation.Email,
            Role = MapRole(invitation.Role),
            ExpiresAt = invitation.ExpiresAt
        };
    }

    public async Task AcceptAsync(
        Guid userId,
        AcceptBusinessInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash =
            _secureTokenService.HashToken(request.Token);

        var invitation = await _dbContext.BusinessInvitations
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Invitația nu a fost găsită.");

        var now = DateTime.UtcNow;

        EnsureInvitationCanBeUsed(invitation, now);

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Utilizatorul nu a fost găsit.");

        if (!user.Email.Equals(
                invitation.Email,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                "Invitația a fost emisă pentru o altă adresă de email.");
        }

        if (!user.EmailConfirmed)
        {
            throw new InvalidOperationException(
                "Adresa de email trebuie confirmată înainte de acceptarea invitației.");
        }

        var existingMembership =
            await _dbContext.BusinessMemberships
                .FirstOrDefaultAsync(
                    x =>
                        x.BusinessId == invitation.BusinessId &&
                        x.UserId == userId,
                    cancellationToken);

        if (existingMembership is not null)
        {
            throw new InvalidOperationException(
                "Ești deja membru al acestei echipe.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var membership = new BusinessMembership
        {
            Id = Guid.NewGuid(),
            BusinessId = invitation.BusinessId,
            UserId = userId,
            Role = invitation.Role,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.BusinessMemberships.Add(membership);

        invitation.AcceptedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RevokeAsync(
        Guid userId,
        Guid businessId,
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var invitation = await _dbContext.BusinessInvitations
            .FirstOrDefaultAsync(
                x =>
                    x.Id == invitationId &&
                    x.BusinessId == businessId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Invitația nu a fost găsită.");

        if (invitation.AcceptedAt.HasValue)
        {
            throw new InvalidOperationException(
                "O invitație deja acceptată nu poate fi anulată.");
        }

        if (invitation.RevokedAt.HasValue)
        {
            throw new InvalidOperationException(
                "Invitația este deja anulată.");
        }

        invitation.RevokedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureInvitationCanBeUsed(
        BusinessInvitation invitation,
        DateTime now)
    {
        if (invitation.AcceptedAt.HasValue)
        {
            throw new InvalidOperationException(
                "Invitația a fost deja acceptată.");
        }

        if (invitation.RevokedAt.HasValue)
        {
            throw new InvalidOperationException(
                "Invitația a fost anulată.");
        }

        if (invitation.ExpiresAt <= now)
        {
            throw new InvalidOperationException(
                "Invitația a expirat.");
        }
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static BusinessRole MapRole(
        BusinessRoleRequest role)
    {
        return role switch
        {
            BusinessRoleRequest.Groomer =>
                BusinessRole.Groomer,

            BusinessRoleRequest.Receptionist =>
                BusinessRole.Receptionist,

            _ => throw new InvalidOperationException(
                "Rolul selectat nu poate fi atribuit prin invitație.")
        };
    }

    private static BusinessRoleRequest MapRole(
        BusinessRole role)
    {
        return role switch
        {
            BusinessRole.Groomer =>
                BusinessRoleRequest.Groomer,

            BusinessRole.Receptionist =>
                BusinessRoleRequest.Receptionist,

            _ => throw new InvalidOperationException(
                "Rolul invitației nu este valid.")
        };
    }

    private static BusinessInvitationResponse MapResponse(
        BusinessInvitation invitation)
    {
        return new BusinessInvitationResponse
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = MapRole(invitation.Role),
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            AcceptedAt = invitation.AcceptedAt,
            RevokedAt = invitation.RevokedAt
        };
    }

    private static string BuildInvitationEmail(
    string businessName,
    BusinessRole role,
    string invitationUrl,
    DateTime expiresAt)
    {
        var roleName = role switch
        {
            BusinessRole.Groomer => "Groomer",
            BusinessRole.Receptionist => "Recepționer",
            _ => "Membru"
        };

        return $$"""
        <!DOCTYPE html>
        <html lang="ro">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Invitație Pufzi</title>
        </head>

        <body style="
            margin: 0;
            padding: 0;
            background-color: #f7f5f2;
            font-family: Arial, Helvetica, sans-serif;
            color: #242424;
        ">
            <table
                role="presentation"
                width="100%"
                cellspacing="0"
                cellpadding="0"
                border="0"
                style="background-color: #f7f5f2; padding: 40px 16px;"
            >
                <tr>
                    <td align="center">

                        <table
                            role="presentation"
                            width="100%"
                            cellspacing="0"
                            cellpadding="0"
                            border="0"
                            style="
                                max-width: 600px;
                                background-color: #ffffff;
                                border-radius: 24px;
                                overflow: hidden;
                                border: 1px solid #ece8e3;
                            "
                        >
                            <!-- Header -->
                            <tr>
                                <td
                                    align="center"
                                    style="
                                        padding: 36px 32px 24px 32px;
                                        background-color: #fff8f2;
                                    "
                                >
                                    <div
                                        style="
                                            font-size: 30px;
                                            font-weight: 800;
                                            color: #f47b35;
                                            letter-spacing: -1px;
                                        "
                                    >
                                        Pufzi
                                    </div>

                                    <div
                                        style="
                                            margin-top: 8px;
                                            font-size: 14px;
                                            color: #77716c;
                                        "
                                    >
                                        Tot ce are nevoie salonul tău, într-un singur loc.
                                    </div>
                                </td>
                            </tr>

                            <!-- Content -->
                            <tr>
                                <td style="padding: 40px 40px 32px 40px;">

                                    <h1
                                        style="
                                            margin: 0 0 16px 0;
                                            font-size: 26px;
                                            line-height: 1.25;
                                            color: #242424;
                                            text-align: center;
                                        "
                                    >
                                        Ai fost invitat în echipă! 🐾
                                    </h1>

                                    <p
                                        style="
                                            margin: 0 auto 28px auto;
                                            max-width: 460px;
                                            font-size: 16px;
                                            line-height: 1.6;
                                            color: #68635f;
                                            text-align: center;
                                        "
                                    >
                                        <strong style="color: #242424;">
                                            {{businessName}}
                                        </strong>
                                        te-a invitat să te alături echipei sale pe Pufzi.
                                    </p>

                                    <!-- Role card -->
                                    <table
                                        role="presentation"
                                        width="100%"
                                        cellspacing="0"
                                        cellpadding="0"
                                        border="0"
                                        style="
                                            margin-bottom: 30px;
                                            background-color: #faf8f5;
                                            border-radius: 16px;
                                            border: 1px solid #eee9e4;
                                        "
                                    >
                                        <tr>
                                            <td
                                                align="center"
                                                style="padding: 20px;"
                                            >
                                                <div
                                                    style="
                                                        margin-bottom: 8px;
                                                        font-size: 13px;
                                                        color: #8a847e;
                                                        text-transform: uppercase;
                                                        letter-spacing: 1px;
                                                        font-weight: 600;
                                                    "
                                                >
                                                    Rolul tău
                                                </div>

                                                <div
                                                    style="
                                                        display: inline-block;
                                                        padding: 8px 16px;
                                                        background-color: #fff0e5;
                                                        border-radius: 999px;
                                                        color: #e86825;
                                                        font-size: 15px;
                                                        font-weight: 700;
                                                    "
                                                >
                                                    {{roleName}}
                                                </div>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- CTA -->
                                    <table
                                        role="presentation"
                                        width="100%"
                                        cellspacing="0"
                                        cellpadding="0"
                                        border="0"
                                    >
                                        <tr>
                                            <td align="center">
                                                <a
                                                    href="{{invitationUrl}}"
                                                    style="
                                                        display: inline-block;
                                                        padding: 15px 30px;
                                                        background-color: #f47b35;
                                                        color: #ffffff;
                                                        text-decoration: none;
                                                        border-radius: 12px;
                                                        font-size: 16px;
                                                        font-weight: 700;
                                                    "
                                                >
                                                    Acceptă invitația
                                                </a>
                                            </td>
                                        </tr>
                                    </table>

                                    <p
                                        style="
                                            margin: 28px 0 0 0;
                                            font-size: 13px;
                                            line-height: 1.6;
                                            color: #918b85;
                                            text-align: center;
                                        "
                                    >
                                        Invitația este valabilă până la
                                        <strong style="color: #68635f;">
                                            {{expiresAt:dd.MM.yyyy, HH:mm}} UTC
                                        </strong>.
                                    </p>
                                </td>
                            </tr>

                            <!-- Divider -->
                            <tr>
                                <td style="padding: 0 40px;">
                                    <div
                                        style="
                                            height: 1px;
                                            background-color: #eeeae6;
                                        "
                                    ></div>
                                </td>
                            </tr>

                            <!-- Fallback link -->
                            <tr>
                                <td style="padding: 28px 40px;">
                                    <p
                                        style="
                                            margin: 0 0 10px 0;
                                            font-size: 13px;
                                            line-height: 1.6;
                                            color: #918b85;
                                        "
                                    >
                                        Dacă butonul nu funcționează, copiază
                                        linkul de mai jos în browser:
                                    </p>

                                    <p
                                        style="
                                            margin: 0;
                                            font-size: 12px;
                                            line-height: 1.6;
                                            word-break: break-all;
                                        "
                                    >
                                        <a
                                            href="{{invitationUrl}}"
                                            style="
                                                color: #f47b35;
                                                text-decoration: none;
                                            "
                                        >
                                            {{invitationUrl}}
                                        </a>
                                    </p>
                                </td>
                            </tr>

                            <!-- Footer -->
                            <tr>
                                <td
                                    align="center"
                                    style="
                                        padding: 24px 40px 32px 40px;
                                        background-color: #faf8f5;
                                    "
                                >
                                    <p
                                        style="
                                            margin: 0 0 6px 0;
                                            font-size: 13px;
                                            color: #77716c;
                                        "
                                    >
                                        Nu te așteptai la această invitație?
                                    </p>

                                    <p
                                        style="
                                            margin: 0;
                                            font-size: 12px;
                                            line-height: 1.5;
                                            color: #a19b95;
                                        "
                                    >
                                        Poți ignora acest email în siguranță.
                                        Nu vei fi adăugat în nicio echipă
                                        fără să accepți invitația.
                                    </p>
                                </td>
                            </tr>
                        </table>

                        <p
                            style="
                                margin: 20px 0 0 0;
                                font-size: 12px;
                                color: #a19b95;
                                text-align: center;
                            "
                        >
                            © {{DateTime.UtcNow.Year}} Pufzi
                        </p>

                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
    }
}