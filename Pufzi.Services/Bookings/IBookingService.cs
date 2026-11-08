
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Bookings;
using Pufzi.Contracts.Responses.Bookings;

namespace Pufzi.Services.Bookings;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(
        Guid userId,
        CreateBookingRequest request,
        CancellationToken cancellationToken = default);

    Task ConfirmBookingAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task RejectBookingAsync(
        Guid userId,
        Guid bookingId,
        string? reason,
        CancellationToken cancellationToken = default);

    Task CompleteBookingAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<BookingListItemResponse>> GetMyBookingsAsync(
        Guid userId,
        GetBookingsRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<BookingListItemResponse>> GetSalonBookingsAsync(
        Guid userId,
        Guid salonId,
        GetBookingsRequest request,
        CancellationToken cancellationToken = default);

    Task<BookingResponse> GetBookingByIdAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task CancelBookingAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<BookingChangeRequestResponse> CreateBookingChangeRequestAsync(
        Guid userId,
        Guid bookingId,
        CreateBookingChangeRequest request,
        CancellationToken cancellationToken = default);

    Task ApproveBookingChangeRequestAsync(
        Guid userId,
        Guid bookingId,
        Guid requestId,
        CancellationToken cancellationToken = default);

    Task RejectBookingChangeRequestAsync(
        Guid userId,
        Guid bookingId,
        Guid requestId,
        string reason,
        CancellationToken cancellationToken = default);

    Task CancelBookingChangeRequestAsync(
        Guid userId,
        Guid bookingId,
        Guid requestId,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<BookingChangeRequestListItemResponse>>
        GetMyBookingChangeRequestsAsync(
            Guid userId,
            GetBookingChangeRequestsRequest request,
            CancellationToken cancellationToken = default);

    Task<PagedResponse<BookingChangeRequestListItemResponse>>
        GetSalonBookingChangeRequestsAsync(
            Guid userId,
            Guid salonId,
            GetBookingChangeRequestsRequest request,
            CancellationToken cancellationToken = default);

    Task<BookingChangeRequestResponse> GetBookingChangeRequestByIdAsync(
        Guid userId,
        Guid bookingId,
        Guid requestId,
        CancellationToken cancellationToken = default);

    Task<List<BookingHistoryResponse>> GetBookingHistoryAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task SalonCancelBookingAsync(
        Guid userId,
        Guid bookingId,
        string? reason,
        CancellationToken cancellationToken = default);
}
