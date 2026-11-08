
using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Bookings;
using Pufzi.Contracts.Responses.Bookings;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Services.Bookings;

public partial class BookingService
{
    public async Task<PagedResponse<BookingChangeRequestListItemResponse>>
        GetMyBookingChangeRequestsAsync(
            Guid userId,
            GetBookingChangeRequestsRequest request,
            CancellationToken cancellationToken = default)
    {
        var query = _dbContext.BookingChangeRequests
            .AsNoTracking()
            .Where(x => x.Booking.UserId == userId);

        return await GetPagedChangeRequestsAsync(
            query,
            request,
            cancellationToken);
    }

    public async Task<PagedResponse<BookingChangeRequestListItemResponse>>
        GetSalonBookingChangeRequestsAsync(
            Guid userId,
            Guid salonId,
            GetBookingChangeRequestsRequest request,
            CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.BusinessMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.BusinessId == salonId &&
                    x.UserId == userId &&
                    x.IsActive &&
                    x.RemovedAt == null &&
                    x.User.IsActive,
                cancellationToken);

        if (membership is null)
        {
            throw new UnauthorizedAccessException(
                "Nu ai acces la solicitările acestui salon.");
        }

        var query = _dbContext.BookingChangeRequests
            .AsNoTracking()
            .Where(x => x.Booking.BusinessId == salonId);

        if (membership.Role == BusinessRole.Groomer)
        {
            query = query.Where(x =>
                x.Booking.BusinessMembershipId == membership.Id);
        }
        else if (membership.Role != BusinessRole.Owner &&
                 membership.Role != BusinessRole.Receptionist)
        {
            throw new UnauthorizedAccessException(
                "Nu ai permisiunea să consulți aceste solicitări.");
        }

        return await GetPagedChangeRequestsAsync(
            query,
            request,
            cancellationToken);
    }

    public async Task<BookingChangeRequestResponse>
        GetBookingChangeRequestByIdAsync(
            Guid userId,
            Guid bookingId,
            Guid requestId,
            CancellationToken cancellationToken = default)
    {
        var changeRequest = await _dbContext.BookingChangeRequests
            .AsNoTracking()
            .Include(x => x.Appointments)
            .Include(x => x.Booking)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == requestId &&
                    x.BookingId == bookingId,
                cancellationToken);

        if (changeRequest is null)
        {
            throw new KeyNotFoundException(
                "Solicitarea de modificare nu a fost găsită.");
        }

        if (changeRequest.Booking.UserId != userId)
        {
            var membership = await _dbContext.BusinessMemberships
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.BusinessId == changeRequest.Booking.BusinessId &&
                        x.UserId == userId &&
                        x.IsActive &&
                        x.RemovedAt == null &&
                        x.User.IsActive,
                    cancellationToken);

            if (membership is null ||
                (
                    membership.Role != BusinessRole.Owner &&
                    membership.Role != BusinessRole.Receptionist &&
                    !(
                        membership.Role == BusinessRole.Groomer &&
                        membership.Id ==
                            changeRequest.Booking.BusinessMembershipId
                    )
                ))
            {
                throw new UnauthorizedAccessException(
                    "Nu ai acces la această solicitare.");
            }
        }

        return new BookingChangeRequestResponse
        {
            Id = changeRequest.Id,
            BookingId = changeRequest.BookingId,
            RequestedByUserId = changeRequest.RequestedByUserId,
            Status = changeRequest.Status.ToString(),
            Reason = changeRequest.Reason,
            ExpiresAt = changeRequest.ExpiresAt,
            CreatedAt = changeRequest.CreatedAt,

            Appointments = changeRequest.Appointments
                .OrderBy(x => x.ProposedStartAt)
                .Select(x => new BookingChangeAppointmentResponse
                {
                    AppointmentId = x.AppointmentId,
                    ProposedStartAtUtc = x.ProposedStartAt,
                    ProposedEndAtUtc = x.ProposedEndAt
                })
                .ToList()
        };
    }

    private async Task<PagedResponse<BookingChangeRequestListItemResponse>>
        GetPagedChangeRequestsAsync(
            IQueryable<BookingChangeRequest> query,
            GetBookingChangeRequestsRequest request,
            CancellationToken cancellationToken)
    {
        if (request.Status.HasValue)
        {
            if (!Enum.IsDefined(
                typeof(BookingChangeRequestStatus),
                request.Status.Value))
            {
                throw new ArgumentException(
                    "Statusul solicitării este invalid.");
            }

            var status =
                (BookingChangeRequestStatus)request.Status.Value;

            query = query.Where(x => x.Status == status);
        }

        if (request.DateFrom.HasValue &&
            request.DateTo.HasValue &&
            request.DateFrom.Value > request.DateTo.Value)
        {
            throw new ArgumentException(
                "Data de început nu poate fi după data de sfârșit.");
        }

        if (request.DateFrom.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(
                request.DateFrom.Value.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);

            query = query.Where(x => x.CreatedAt >= fromUtc);
        }

        if (request.DateTo.HasValue)
        {
            var toUtcExclusive = DateTime.SpecifyKind(
                request.DateTo.Value
                    .AddDays(1)
                    .ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);

            query = query.Where(x =>
                x.CreatedAt < toUtcExclusive);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orderedQuery =
            request.SortDirection == SortDirection.Asc
                ? query.OrderBy(x => x.CreatedAt)
                    .ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.Id);

        var items = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new BookingChangeRequestListItemResponse
            {
                Id = x.Id,
                BookingId = x.BookingId,
                SalonId = x.Booking.BusinessId,
                SalonName = x.Booking.Business.Name,
                RequestedByUserId = x.RequestedByUserId,
                Status = (int)x.Status,
                Reason = x.Reason,
                AppointmentsCount = x.Appointments.Count,
                ExpiresAt = x.ExpiresAt,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<BookingChangeRequestListItemResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
