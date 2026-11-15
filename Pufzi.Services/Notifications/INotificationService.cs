
using Pufzi.Contracts.Notifications.Requests;
using Pufzi.Contracts.Notifications.Responses;

namespace Pufzi.Services.Notifications;

public interface INotificationService
{
    Task<NotificationListResponse> GetMyNotificationsAsync(
        Guid userId,
        int page,
        int pageSize,
        string language,
        CancellationToken cancellationToken = default);

    Task<UnreadNotificationCountResponse> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task DeleteNotificationAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task<NotificationPreferencesResponse> GetPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<NotificationPreferencesResponse> UpdatePreferencesAsync(
        Guid userId,
        UpdateNotificationPreferencesRequest request,
        CancellationToken cancellationToken = default);

    Task<UserDeviceResponse> RegisterDeviceAsync(
        Guid userId,
        RegisterDeviceRequest request,
        CancellationToken cancellationToken = default);

    Task UnregisterDeviceAsync(
        Guid userId,
        Guid deviceId,
        CancellationToken cancellationToken = default);
}
