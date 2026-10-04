using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Enums;
using Pufzi.Contracts.Requests.Leave;
using Pufzi.Contracts.Responses.Leave;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;
using Pufzi.Services.Businesses;
using System.Data;

namespace Pufzi.Services.Leave;

public class LeaveService : ILeaveService
{
    private readonly PufziDbContext _dbContext;
    private readonly IBusinessAccessService _businessAccessService;

    public LeaveService(
        PufziDbContext dbContext,
        IBusinessAccessService businessAccessService)
    {
        _dbContext = dbContext;
        _businessAccessService = businessAccessService;
    }

    public async Task<EmployeeLeaveBalanceResponse> GetMyBalanceAsync(
        Guid userId,
        Guid businessId,
        int? year = null,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await _businessAccessService.RequireMembershipAsync(
                userId,
                businessId,
                cancellationToken);

        var targetYear = year ?? DateTime.UtcNow.Year;

        ValidateYear(targetYear);

        var balance = await GetOrCreateLeaveBalanceAsync(
            membership.Id,
            targetYear,
            cancellationToken);

        return MapBalance(balance);
    }

    public async Task<PagedResponse<LeaveRequestResponse>> GetMyRequestsAsync(
        Guid userId,
        Guid businessId,
        LeaveRequestsFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await _businessAccessService.RequireMembershipAsync(
                userId,
                businessId,
                cancellationToken);

        ValidateFilter(request);

        var query = _dbContext.LeaveRequests
            .AsNoTracking()
            .Include(x => x.BusinessMembership)
                .ThenInclude(x => x.User)
            .Where(x =>
                x.BusinessMembershipId == membership.Id);

        query = ApplyFilters(
            query,
            request,
            allowSearch: false);

        var totalCount =
            await query.CountAsync(cancellationToken);

        query = ApplySorting(query, request);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<LeaveRequestResponse>
        {
            Items = items.Select(MapLeaveRequest).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResponse<LeaveRequestResponse>> GetTeamRequestsAsync(
        Guid userId,
        Guid businessId,
        LeaveRequestsFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccessService.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        ValidateFilter(request);

        var query = _dbContext.LeaveRequests
            .AsNoTracking()
            .Include(x => x.BusinessMembership)
                .ThenInclude(x => x.User)
            .Where(x =>
                x.BusinessMembership.BusinessId == businessId &&
                x.BusinessMembership.RemovedAt == null);

        query = ApplyFilters(
            query,
            request,
            allowSearch: true);

        var totalCount =
            await query.CountAsync(cancellationToken);

        query = ApplySorting(query, request);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<LeaveRequestResponse>
        {
            Items = items.Select(MapLeaveRequest).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<EmployeeLeaveBalanceResponse> GetTeamMemberBalanceAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        int? year = null,
        CancellationToken cancellationToken = default)
    {
        var requester =
            await _businessAccessService.RequireMembershipAsync(
                userId,
                businessId,
                cancellationToken);

        if (requester.Role != BusinessRole.Owner &&
            requester.UserId != teamMemberUserId)
        {
            throw new UnauthorizedAccessException(
                "Poți vizualiza doar propriul sold de concediu.");
        }

        var membership = await GetTeamMemberAsync(
            businessId,
            teamMemberUserId,
            cancellationToken);

        var targetYear = year ?? DateTime.UtcNow.Year;

        ValidateYear(targetYear);

        var balance = await GetOrCreateLeaveBalanceAsync(
            membership.Id,
            targetYear,
            cancellationToken);

        return MapBalance(balance);
    }

    public async Task<EmployeeLeaveBalanceResponse> UpdateTeamMemberBalanceAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        UpdateEmployeeLeaveBalanceRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccessService.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        ValidateYear(request.Year);

        var membership = await GetTeamMemberAsync(
            businessId,
            teamMemberUserId,
            cancellationToken);

        var balance =
            await _dbContext.EmployeeLeaveBalances
                .FirstOrDefaultAsync(
                    x =>
                        x.BusinessMembershipId == membership.Id &&
                        x.Year == request.Year,
                    cancellationToken);

        if (balance is null)
        {
            balance = new EmployeeLeaveBalance
            {
                Id = Guid.NewGuid(),
                BusinessMembershipId = membership.Id,
                Year = request.Year,
                TotalDays = request.TotalDays,
                UsedDays = 0
            };

            _dbContext.EmployeeLeaveBalances.Add(balance);
        }
        else
        {
            if (request.TotalDays < balance.UsedDays)
            {
                throw new InvalidOperationException(
                    $"Numărul total de zile nu poate fi mai mic decât cele {balance.UsedDays} zile deja utilizate.");
            }

            balance.TotalDays = request.TotalDays;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapBalance(balance);
    }

    private async Task<EmployeeLeaveBalance> GetOrCreateLeaveBalanceAsync(
        Guid businessMembershipId,
        int year,
        CancellationToken cancellationToken)
    {
        var balance =
            await _dbContext.EmployeeLeaveBalances
                .FirstOrDefaultAsync(
                    x =>
                        x.BusinessMembershipId == businessMembershipId &&
                        x.Year == year,
                    cancellationToken);

        if (balance is not null)
        {
            return balance;
        }

        var previousBalance =
            await _dbContext.EmployeeLeaveBalances
                .Where(x =>
                    x.BusinessMembershipId == businessMembershipId &&
                    x.Year < year)
                .OrderByDescending(x => x.Year)
                .FirstOrDefaultAsync(cancellationToken);

        if (previousBalance is null)
        {
            throw new InvalidOperationException(
                $"Soldul de concediu pentru anul {year} nu a fost configurat.");
        }

        balance = new EmployeeLeaveBalance
        {
            Id = Guid.NewGuid(),
            BusinessMembershipId = businessMembershipId,
            Year = year,
            TotalDays = previousBalance.TotalDays,
            UsedDays = 0
        };

        _dbContext.EmployeeLeaveBalances.Add(balance);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return balance;
    }

    private async Task<BusinessMembership> GetTeamMemberAsync(
        Guid businessId,
        Guid teamMemberUserId,
        CancellationToken cancellationToken)
    {
        var membership =
            await _dbContext.BusinessMemberships
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x =>
                        x.BusinessId == businessId &&
                        x.UserId == teamMemberUserId &&
                        x.IsActive &&
                        x.RemovedAt == null,
                    cancellationToken);

        if (membership is null)
        {
            throw new KeyNotFoundException(
                "Membrul echipei nu a fost găsit.");
        }

        return membership;
    }

    private static IQueryable<LeaveRequest> ApplyFilters(
        IQueryable<LeaveRequest> query,
        LeaveRequestsFilterRequest request,
        bool allowSearch)
    {
        if (request.Status.HasValue)
        {
            var status =
                (LeaveRequestStatus)(int)request.Status.Value;

            query = query.Where(x =>
                x.Status == status);
        }

        if (request.LeaveType.HasValue)
        {
            var type =
                (LeaveType)(int)request.LeaveType.Value;

            query = query.Where(x =>
                x.Type == type);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(x =>
                x.EndDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x =>
                x.StartDate <= request.ToDate.Value);
        }

        if (allowSearch &&
            !string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(
                    x.BusinessMembership.User.FirstName,
                    $"%{search}%") ||
                EF.Functions.ILike(
                    x.BusinessMembership.User.LastName,
                    $"%{search}%") ||
                EF.Functions.ILike(
                    x.BusinessMembership.User.FirstName + " " +
                    x.BusinessMembership.User.LastName,
                    $"%{search}%"));
        }

        return query;
    }

    private static IQueryable<LeaveRequest> ApplySorting(
        IQueryable<LeaveRequest> query,
        LeaveRequestsFilterRequest request)
    {
        var descending =
            request.SortDirection == SortDirection.Desc;

        var sortBy =
            request.SortBy?.Trim().ToLowerInvariant();

        return sortBy switch
        {
            "startdate" => descending
                ? query.OrderByDescending(x => x.StartDate)
                : query.OrderBy(x => x.StartDate),

            "enddate" => descending
                ? query.OrderByDescending(x => x.EndDate)
                : query.OrderBy(x => x.EndDate),

            "status" => descending
                ? query.OrderByDescending(x => x.Status)
                : query.OrderBy(x => x.Status),

            "type" => descending
                ? query.OrderByDescending(x => x.Type)
                : query.OrderBy(x => x.Type),

            "createdat" or null or "" => descending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt),

            _ => throw new InvalidOperationException(
                "Câmpul de sortare nu este valid.")
        };
    }

    private static void ValidateFilter(
        LeaveRequestsFilterRequest request)
    {
        if (request.Page < 1)
        {
            throw new InvalidOperationException(
                "Numărul paginii trebuie să fie cel puțin 1.");
        }

        if (request.PageSize < 1 ||
            request.PageSize > 100)
        {
            throw new InvalidOperationException(
                "Numărul de rezultate pe pagină trebuie să fie între 1 și 100.");
        }

        if (request.FromDate.HasValue &&
            request.ToDate.HasValue &&
            request.FromDate.Value > request.ToDate.Value)
        {
            throw new InvalidOperationException(
                "Data de început a filtrului nu poate fi după data de sfârșit.");
        }

        if (request.Status.HasValue &&
            !Enum.IsDefined(request.Status.Value))
        {
            throw new InvalidOperationException(
                "Statusul cererii nu este valid.");
        }

        if (request.LeaveType.HasValue &&
            !Enum.IsDefined(request.LeaveType.Value))
        {
            throw new InvalidOperationException(
                "Tipul concediului nu este valid.");
        }
    }

    private static void ValidateYear(int year)
    {
        if (year < 2000 || year > 2100)
        {
            throw new InvalidOperationException(
                "Anul trebuie să fie între 2000 și 2100.");
        }
    }

    private static EmployeeLeaveBalanceResponse MapBalance(
        EmployeeLeaveBalance balance)
    {
        return new EmployeeLeaveBalanceResponse
        {
            BusinessMembershipId =
                balance.BusinessMembershipId,

            Year = balance.Year,
            TotalDays = balance.TotalDays,
            UsedDays = balance.UsedDays,

            RemainingDays = Math.Max(
                0,
                balance.TotalDays - balance.UsedDays)
        };
    }

    private static LeaveRequestResponse MapLeaveRequest(
        LeaveRequest leaveRequest)
    {
        return new LeaveRequestResponse
        {
            Id = leaveRequest.Id,

            BusinessMembershipId =
                leaveRequest.BusinessMembershipId,

            UserId =
                leaveRequest.BusinessMembership.UserId,

            FirstName =
                leaveRequest.BusinessMembership.User.FirstName,

            LastName =
                leaveRequest.BusinessMembership.User.LastName,

            Type =
                (LeaveTypeRequest)(int)leaveRequest.Type,

            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            RequestedDays = leaveRequest.RequestedDays,
            Reason = leaveRequest.Reason,

            Status =
                (LeaveRequestStatusRequest)(int)leaveRequest.Status,

            ReviewNote = leaveRequest.ReviewNote,
            ReviewedByUserId = leaveRequest.ReviewedByUserId,
            ReviewedAt = leaveRequest.ReviewedAt,
            CreatedAt = leaveRequest.CreatedAt,
            UpdatedAt = leaveRequest.UpdatedAt
        };
    }

    public async Task<LeaveRequestResponse> CreateRequestAsync(
        Guid userId,
        Guid businessId,
        CreateLeaveRequest request,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await _businessAccessService.RequireMembershipAsync(
                userId,
                businessId,
                cancellationToken);

        ValidateCreateRequest(request);

        var hasOverlap =
            await _dbContext.LeaveRequests
                .AnyAsync(
                    x =>
                        x.BusinessMembershipId == membership.Id &&
                        (x.Status == LeaveRequestStatus.Pending ||
                         x.Status == LeaveRequestStatus.Approved) &&
                        x.StartDate <= request.EndDate &&
                        x.EndDate >= request.StartDate,
                    cancellationToken);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "Există deja o cerere de concediu activă care se suprapune cu perioada selectată.");
        }

        var requestedDaysByYear =
            await CalculateEffectiveDaysByYearAsync(
                membership.Id,
                businessId,
                request.StartDate,
                request.EndDate,
                cancellationToken);

        var totalRequestedDays =
            requestedDaysByYear.Values.Sum();

        if (totalRequestedDays <= 0)
        {
            throw new InvalidOperationException(
                "Perioada selectată nu conține nicio zi de lucru eligibilă.");
        }

        var leaveType =
            (LeaveType)(int)request.Type;

        if (leaveType == LeaveType.AnnualLeave)
        {
            await ValidateAnnualLeaveBalanceAsync(
                membership.Id,
                requestedDaysByYear,
                cancellationToken);
        }

        var now = DateTime.UtcNow;

        var leaveRequest = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            BusinessMembershipId = membership.Id,
            Type = leaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RequestedDays = totalRequestedDays,
            Reason = NormalizeOptionalText(request.Reason),
            Status = LeaveRequestStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.LeaveRequests.Add(leaveRequest);

        await _dbContext.SaveChangesAsync(cancellationToken);

        leaveRequest.BusinessMembership = membership;

        if (membership.User is null)
        {
            await _dbContext.Entry(membership)
                .Reference(x => x.User)
                .LoadAsync(cancellationToken);
        }

        return MapLeaveRequest(leaveRequest);
    }

    public async Task<LeaveRequestResponse> CancelRequestAsync(
        Guid userId,
        Guid businessId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await _businessAccessService.RequireMembershipAsync(
                userId,
                businessId,
                cancellationToken);

        var leaveRequest =
            await _dbContext.LeaveRequests
                .Include(x => x.BusinessMembership)
                    .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == requestId &&
                        x.BusinessMembershipId == membership.Id,
                    cancellationToken);

        if (leaveRequest is null)
        {
            throw new KeyNotFoundException(
                "Cererea de concediu nu a fost găsită.");
        }

        if (leaveRequest.Status != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                "Doar cererile aflate în așteptare pot fi anulate.");
        }

        leaveRequest.Status = LeaveRequestStatus.Cancelled;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapLeaveRequest(leaveRequest);
    }

    public async Task<LeaveRequestResponse> ApproveRequestAsync(
        Guid userId,
        Guid businessId,
        Guid requestId,
        ReviewLeaveRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccessService.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        var leaveRequest =
            await GetLeaveRequestForReviewAsync(
                businessId,
                requestId,
                cancellationToken);

        if (leaveRequest.Status != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                "Doar cererile aflate în așteptare pot fi aprobate.");
        }

        var requestedDaysByYear =
            await CalculateEffectiveDaysByYearAsync(
                leaveRequest.BusinessMembershipId,
                businessId,
                leaveRequest.StartDate,
                leaveRequest.EndDate,
                cancellationToken);

        var totalRequestedDays =
            requestedDaysByYear.Values.Sum();

        if (totalRequestedDays <= 0)
        {
            throw new InvalidOperationException(
                "Cererea nu mai conține nicio zi de lucru eligibilă.");
        }

        if (leaveRequest.Type == LeaveType.AnnualLeave)
        {
            var balances =
                await ValidateAnnualLeaveBalanceAsync(
                    leaveRequest.BusinessMembershipId,
                    requestedDaysByYear,
                    cancellationToken);

            foreach (var entry in requestedDaysByYear)
            {
                var balance = balances[entry.Key];

                balance.UsedDays += entry.Value;
            }
        }

        var now = DateTime.UtcNow;

        leaveRequest.RequestedDays = totalRequestedDays;
        leaveRequest.Status = LeaveRequestStatus.Approved;
        leaveRequest.ReviewedByUserId = userId;
        leaveRequest.ReviewedAt = now;
        leaveRequest.ReviewNote =
            NormalizeOptionalText(request.ReviewNote);
        leaveRequest.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return MapLeaveRequest(leaveRequest);
    }

    public async Task<LeaveRequestResponse> RejectRequestAsync(
        Guid userId,
        Guid businessId,
        Guid requestId,
        ReviewLeaveRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccessService.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var leaveRequest =
            await GetLeaveRequestForReviewAsync(
                businessId,
                requestId,
                cancellationToken);

        if (leaveRequest.Status != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                "Doar cererile aflate în așteptare pot fi respinse.");
        }

        var now = DateTime.UtcNow;

        leaveRequest.Status = LeaveRequestStatus.Rejected;
        leaveRequest.ReviewedByUserId = userId;
        leaveRequest.ReviewedAt = now;
        leaveRequest.ReviewNote =
            NormalizeOptionalText(request.ReviewNote);
        leaveRequest.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapLeaveRequest(leaveRequest);
    }

    private static void ValidateCreateRequest(
    CreateLeaveRequest request)
    {
        if (!Enum.IsDefined(request.Type))
        {
            throw new InvalidOperationException(
                "Tipul concediului nu este valid.");
        }

        if (request.StartDate > request.EndDate)
        {
            throw new InvalidOperationException(
                "Data de început nu poate fi după data de sfârșit.");
        }

        var today =
            DateOnly.FromDateTime(DateTime.UtcNow);

        if (request.StartDate < today)
        {
            throw new InvalidOperationException(
                "Nu poți crea o cerere de concediu pentru o perioadă din trecut.");
        }
    }

    private async Task<LeaveRequest> GetLeaveRequestForReviewAsync(
        Guid businessId,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        var leaveRequest =
            await _dbContext.LeaveRequests
                .Include(x => x.BusinessMembership)
                    .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == requestId &&
                        x.BusinessMembership.BusinessId == businessId &&
                        x.BusinessMembership.RemovedAt == null,
                    cancellationToken);

        if (leaveRequest is null)
        {
            throw new KeyNotFoundException(
                "Cererea de concediu nu a fost găsită.");
        }

        return leaveRequest;
    }

    private async Task<Dictionary<int, int>>
        CalculateEffectiveDaysByYearAsync(
            Guid businessMembershipId,
            Guid businessId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken)
    {
        var workingDays =
            await _dbContext.EmployeeWorkingHours
                .AsNoTracking()
                .Where(x =>
                    x.BusinessMembershipId == businessMembershipId &&
                    x.IsWorking)
                .Select(x => x.DayOfWeek)
                .Distinct()
                .ToListAsync(cancellationToken);

        if (workingDays.Count == 0)
        {
            throw new InvalidOperationException(
                "Membrul echipei nu are un program de lucru configurat.");
        }

        var exceptions =
            await _dbContext.BusinessScheduleExceptions
                .AsNoTracking()
                .Where(x =>
                    x.BusinessId == businessId &&
                    x.Date >= startDate &&
                    x.Date <= endDate)
                .ToDictionaryAsync(
                    x => x.Date,
                    cancellationToken);

        var result = new Dictionary<int, int>();

        for (var date = startDate;
             date <= endDate;
             date = date.AddDays(1))
        {
            var dayOfWeek = date.DayOfWeek;

            if (!workingDays.Contains(dayOfWeek))
            {
                continue;
            }

            if (exceptions.TryGetValue(
                    date,
                    out var scheduleException) &&
                scheduleException.IsClosed)
            {
                continue;
            }

            if (!result.TryAdd(date.Year, 1))
            {
                result[date.Year]++;
            }
        }

        return result;
    }

    private async Task<Dictionary<int, EmployeeLeaveBalance>>
        ValidateAnnualLeaveBalanceAsync(
            Guid businessMembershipId,
            IReadOnlyDictionary<int, int> requestedDaysByYear,
            CancellationToken cancellationToken)
    {
        var balances =
            new Dictionary<int, EmployeeLeaveBalance>();

        foreach (var entry in requestedDaysByYear)
        {
            var balance =
                await GetOrCreateLeaveBalanceAsync(
                    businessMembershipId,
                    entry.Key,
                    cancellationToken);

            var remainingDays =
                balance.TotalDays - balance.UsedDays;

            if (remainingDays < entry.Value)
            {
                throw new InvalidOperationException(
                    $"Nu există suficiente zile de concediu disponibile pentru anul {entry.Key}. " +
                    $"Disponibile: {Math.Max(0, remainingDays)}, necesare: {entry.Value}.");
            }

            balances[entry.Key] = balance;
        }

        return balances;
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}