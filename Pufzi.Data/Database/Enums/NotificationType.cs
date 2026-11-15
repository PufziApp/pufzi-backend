
namespace Pufzi.Data.Database.Enums;

public enum NotificationType
{
    BookingCreated = 1,
    BookingConfirmed = 2,
    BookingRejected = 3,
    BookingCancelledByCustomer = 4,
    BookingCancelledBySalon = 5,
    BookingChangeRequested = 6,
    BookingChangeApproved = 7,
    BookingChangeRejected = 8,
    BookingReminder24Hours = 9,
    BookingReminder2Hours = 10,
    ReviewCreated = 11,
    ReviewReplied = 12,
    TeamInvitationReceived = 13,
    LeaveRequested = 14,
    LeaveApproved = 15,
    LeaveRejected = 16,
    LeaveRevoked = 17
}
