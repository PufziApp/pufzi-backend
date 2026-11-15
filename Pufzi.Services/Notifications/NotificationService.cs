
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pufzi.Contracts.Notifications.Requests;
using Pufzi.Contracts.Notifications.Responses;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly PufziDbContext _dbContext;

    public NotificationService(PufziDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationListResponse> GetMyNotificationsAsync(
        Guid userId,
        int page,
        int pageSize,
        string language,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateLanguage(language);

        if (page < 1)
            throw new ArgumentException(
                "Pagina trebuie să fie mai mare sau egală cu 1.");

        if (pageSize is < 1 or > 100)
            throw new ArgumentException(
                "Dimensiunea paginii trebuie să fie între 1 și 100.");

        var query = _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var unreadCount = await query.CountAsync(
            x => !x.IsRead,
            cancellationToken);

        var notifications = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip(checked((page - 1) * pageSize))
            .Take(pageSize)
            .Select(x => new NotificationResponse
            {
                Id = x.Id,
                Type = x.Type.ToString(),
                BusinessId = x.BusinessId,
                BookingId = x.BookingId,
                IsRead = x.IsRead,
                ReadAt = x.ReadAt,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new NotificationListResponse
        {
            Items = notifications,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            UnreadCount = unreadCount
        };
    }

    public async Task<UnreadNotificationCountResponse> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var count = await _dbContext.Notifications
            .AsNoTracking()
            .CountAsync(
                x => x.UserId == userId && !x.IsRead,
                cancellationToken);

        return new UnreadNotificationCountResponse
        {
            Count = count
        };
    }

    public async Task MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateId(notificationId, "Notificarea");

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(
                x => x.Id == notificationId && x.UserId == userId,
                cancellationToken);

        if (notification is null)
            throw new KeyNotFoundException(
                "Notificarea nu a fost găsită.");

        if (notification.IsRead)
            return;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var nowUtc = DateTime.UtcNow;

        await _dbContext.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.IsRead, true)
                    .SetProperty(x => x.ReadAt, nowUtc),
                cancellationToken);
    }

    public async Task DeleteNotificationAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateId(notificationId, "Notificarea");

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(
                x => x.Id == notificationId && x.UserId == userId,
                cancellationToken);

        if (notification is null)
            throw new KeyNotFoundException(
                "Notificarea nu a fost găsită.");

        _dbContext.Notifications.Remove(notification);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationPreferencesResponse> GetPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var preferences = await _dbContext.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        return MapPreferences(preferences);
    }

    public async Task<NotificationPreferencesResponse> UpdatePreferencesAsync(
        Guid userId,
        UpdateNotificationPreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ArgumentNullException.ThrowIfNull(request);
        ValidateLanguage(request.PreferredLanguage);

        var preferences = await _dbContext.NotificationPreferences
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (preferences is null)
        {
            preferences = new NotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId
            };

            _dbContext.NotificationPreferences.Add(preferences);
        }

        preferences.PreferredLanguage = request.PreferredLanguage;
        preferences.PushEnabled = request.PushEnabled;
        preferences.BookingNotificationsEnabled =
            request.BookingNotificationsEnabled;
        preferences.ReminderNotificationsEnabled =
            request.ReminderNotificationsEnabled;
        preferences.ReviewNotificationsEnabled =
            request.ReviewNotificationsEnabled;
        preferences.TeamNotificationsEnabled =
            request.TeamNotificationsEnabled;
        preferences.LeaveNotificationsEnabled =
            request.LeaveNotificationsEnabled;
        preferences.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_NotificationPreferences_UserId"
            })
        {
            throw new InvalidOperationException(
                "Preferințele au fost actualizate simultan. Reîncearcă.");
        }

        return MapPreferences(preferences);
    }

    public async Task<UserDeviceResponse> RegisterDeviceAsync(
        Guid userId,
        RegisterDeviceRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ArgumentNullException.ThrowIfNull(request);

        var token = request.FcmToken?.Trim();

        if (string.IsNullOrWhiteSpace(token) || token.Length > 4096)
            throw new ArgumentException(
                "Tokenul Firebase este invalid.");

        if (request.Platform is not ("ios" or "android"))
            throw new ArgumentException(
                "Platforma trebuie să fie ios sau android.");

        ValidateLanguage(request.Language);

        var device = await _dbContext.UserDevices
            .FirstOrDefaultAsync(
                x => x.FcmToken == token,
                cancellationToken);

        var nowUtc = DateTime.UtcNow;

        if (device is null)
        {
            device = new UserDevice
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FcmToken = token,
                Platform = request.Platform,
                Language = request.Language,
                IsActive = true,
                CreatedAt = nowUtc,
                UpdatedAt = nowUtc,
                LastSeenAt = nowUtc
            };

            _dbContext.UserDevices.Add(device);
        }
        else
        {
            // Un token deja asociat altui cont nu poate fi
            // preluat automat printr-un simplu request.
            if (device.UserId != userId)
                throw new InvalidOperationException(
                    "Dispozitivul este asociat altui cont.");

            device.Platform = request.Platform;
            device.Language = request.Language;
            device.IsActive = true;
            device.UpdatedAt = nowUtc;
            device.LastSeenAt = nowUtc;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_UserDevices_FcmToken"
            })
        {
            throw new InvalidOperationException(
                "Tokenul dispozitivului a fost înregistrat simultan. Reîncearcă.");
        }

        return new UserDeviceResponse
        {
            Id = device.Id,
            Platform = device.Platform,
            Language = device.Language,
            IsActive = device.IsActive,
            CreatedAt = device.CreatedAt,
            LastSeenAt = device.LastSeenAt
        };
    }

    public async Task UnregisterDeviceAsync(
        Guid userId,
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateId(deviceId, "Dispozitivul");

        var device = await _dbContext.UserDevices
            .FirstOrDefaultAsync(
                x => x.Id == deviceId && x.UserId == userId,
                cancellationToken);

        if (device is null)
            throw new KeyNotFoundException(
                "Dispozitivul nu a fost găsit.");

        device.IsActive = false;
        device.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static NotificationPreferencesResponse MapPreferences(
        NotificationPreference? preferences)
    {
        if (preferences is null)
        {
            return new NotificationPreferencesResponse
            {
                PreferredLanguage = "ro",
                PushEnabled = true,
                BookingNotificationsEnabled = true,
                ReminderNotificationsEnabled = true,
                ReviewNotificationsEnabled = true,
                TeamNotificationsEnabled = true,
                LeaveNotificationsEnabled = true
            };
        }

        return new NotificationPreferencesResponse
        {
            PreferredLanguage = preferences.PreferredLanguage,
            PushEnabled = preferences.PushEnabled,
            BookingNotificationsEnabled =
                preferences.BookingNotificationsEnabled,
            ReminderNotificationsEnabled =
                preferences.ReminderNotificationsEnabled,
            ReviewNotificationsEnabled =
                preferences.ReviewNotificationsEnabled,
            TeamNotificationsEnabled =
                preferences.TeamNotificationsEnabled,
            LeaveNotificationsEnabled =
                preferences.LeaveNotificationsEnabled
        };
    }

    private static void ValidateUserId(Guid userId)
    {
        ValidateId(userId, "Utilizatorul");
    }

    private static void ValidateId(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                $"{name} are un identificator invalid.");
    }

    private static void ValidateLanguage(string? language)
    {
        if (language is not ("ro" or "en" or "hu"))
            throw new ArgumentException(
                "Limba trebuie să fie ro, en sau hu.");
    }
}
