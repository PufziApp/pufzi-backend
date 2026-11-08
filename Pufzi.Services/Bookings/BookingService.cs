
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Bookings;
using Pufzi.Contracts.Responses.Bookings;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;
using System.Data;

namespace Pufzi.Services.Bookings;

public partial class BookingService : IBookingService
{
    private const int MaxAppointments = 10;
    private const int MaxDurationMinutes = 720;

    private readonly PufziDbContext _dbContext;
    private readonly IAppointmentAvailabilityService _availability;

    public BookingService(
        PufziDbContext dbContext,
        IAppointmentAvailabilityService availability)
    {
        _dbContext = dbContext;
        _availability = availability;
    }


    public async Task<PagedResponse<BookingListItemResponse>> GetMyBookingsAsync(
        Guid userId,
        GetBookingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Bookings
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        return await GetPagedBookingsAsync(
            query,
            request,
            cancellationToken);
    }

    public async Task<PagedResponse<BookingListItemResponse>> GetSalonBookingsAsync(
        Guid userId,
        Guid salonId,
        GetBookingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.BusinessMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.BusinessId == salonId &&
                x.UserId == userId &&
                x.IsActive &&
                x.RemovedAt == null &&
                x.User.IsActive,
                cancellationToken);

        if (membership is null)
        {
            throw new UnauthorizedAccessException(
                "Nu ai acces la rezervările acestui salon.");
        }

        var query = _dbContext.Bookings
            .AsNoTracking()
            .Where(x => x.BusinessId == salonId);

        if (membership.Role == BusinessRole.Groomer)
        {
            query = query.Where(x =>
                x.BusinessMembershipId == membership.Id);
        }

        return await GetPagedBookingsAsync(
            query,
            request,
            cancellationToken);
    }


    public async Task<BookingResponse> GetBookingByIdAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var booking = await _dbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Appointments)
                .ThenInclude(x => x.Services)
            .FirstOrDefaultAsync(
                x => x.Id == bookingId,
                cancellationToken);

        if (booking is null)
        {
            throw new KeyNotFoundException(
                "Rezervarea nu a fost găsită.");
        }

        if (booking.UserId != userId)
        {
            var membership = await _dbContext.BusinessMemberships
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.BusinessId == booking.BusinessId &&
                        x.UserId == userId &&
                        x.IsActive &&
                        x.RemovedAt == null &&
                        x.User.IsActive,
                    cancellationToken);

            if (membership is null ||
                (membership.Role == BusinessRole.Groomer &&
                 membership.Id != booking.BusinessMembershipId))
            {
                throw new UnauthorizedAccessException(
                    "Nu ai acces la această rezervare.");
            }
        }

        return new BookingResponse
        {
            Id = booking.Id,
            SalonId = booking.BusinessId,
            UserId = booking.UserId,
            BusinessMembershipId = booking.BusinessMembershipId,
            Status = booking.Status.ToString(),
            Notes = booking.Notes,
            ExpiresAt = booking.ExpiresAt,
            CreatedAt = booking.CreatedAt,

            Appointments = booking.Appointments
                .OrderBy(x => x.SortOrder)
                .Select(x => new BookingAppointmentResponse
                {
                    Id = x.Id,
                    PetId = x.PetId,
                    StartAtUtc = x.StartAt,
                    EndAtUtc = x.EndAt,
                    DurationMinutes = x.DurationMinutes,
                    TotalPrice = x.TotalPrice,
                    HasPriceOnRequest = x.HasPriceOnRequest,

                    Services = x.Services
                        .OrderBy(s => s.SortOrder)
                        .Select(s => new BookingServiceResponse
                        {
                            ServicePackageId = s.ServicePackageId,
                            PackageName = s.PackageName,
                            DurationMinutes = s.DurationMinutes,
                            TotalPrice = s.TotalPrice,
                            HasPriceOnRequest = s.HasPriceOnRequest
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    private async Task<PagedResponse<BookingListItemResponse>> GetPagedBookingsAsync(
        IQueryable<Booking> query,
        GetBookingsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Status.HasValue)
        {
            if (!Enum.IsDefined(
                typeof(BookingStatus),
                request.Status.Value))
            {
                throw new ArgumentException(
                    "Statusul rezervării este invalid.");
            }

            var status = (BookingStatus)request.Status.Value;

            query = query.Where(x => x.Status == status);
        }

        if (request.BusinessMembershipId.HasValue)
        {
            query = query.Where(x =>
                x.BusinessMembershipId ==
                request.BusinessMembershipId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(
                request.DateFrom.Value.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);

            query = query.Where(x =>
                x.Appointments.Any(a => a.StartAt >= fromUtc));
        }

        if (request.DateTo.HasValue)
        {
            var toUtcExclusive = DateTime.SpecifyKind(
                request.DateTo.Value
                    .AddDays(1)
                    .ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);

            query = query.Where(x =>
                x.Appointments.Any(a =>
                    a.StartAt < toUtcExclusive));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orderedQuery = request.SortDirection == SortDirection.Desc
            ? query.OrderByDescending(x =>
                    x.Appointments.Min(a => a.StartAt))
                .ThenByDescending(x => x.Id)
            : query.OrderBy(x =>
                    x.Appointments.Min(a => a.StartAt))
                .ThenBy(x => x.Id);

        var items = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new BookingListItemResponse
            {
                Id = x.Id,
                SalonId = x.BusinessId,
                SalonName = x.Business.Name,
                UserId = x.UserId,
                BusinessMembershipId = x.BusinessMembershipId,
                Status = (int)x.Status,
                PetsCount = x.Appointments.Count,
                StartAtUtc = x.Appointments.Min(a => a.StartAt),
                EndAtUtc = x.Appointments.Max(a => a.EndAt),
                TotalPrice = x.Appointments.Sum(a => a.TotalPrice),
                HasPriceOnRequest = x.Appointments.Any(a =>
                    a.HasPriceOnRequest),
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<BookingListItemResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<BookingResponse> CreateBookingAsync(
        Guid userId,
        CreateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Utilizatorul este invalid.");

        if (request.SalonId == Guid.Empty ||
            request.BusinessMembershipId == Guid.Empty)
            throw new ArgumentException("Salonul sau angajatul este invalid.");

        if (request.Notes?.Length > 2000)
            throw new ArgumentException("Observațiile sunt prea lungi.");

        if (request.Appointments is null ||
            request.Appointments.Count is < 1 or > MaxAppointments)
            throw new ArgumentException(
                "Rezervarea trebuie să conțină între 1 și 10 programări.");

        var salonExists = await _dbContext.Businesses
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == request.SalonId &&
                x.IsPublished,
                cancellationToken);

        if (!salonExists)
            throw new KeyNotFoundException("Salonul nu a fost găsit.");

        var employeeExists = await _dbContext.BusinessMemberships
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == request.BusinessMembershipId &&
                x.BusinessId == request.SalonId &&
                x.IsActive &&
                x.RemovedAt == null &&
                x.User.IsActive,
                cancellationToken);

        if (!employeeExists)
            throw new KeyNotFoundException("Angajatul nu a fost găsit.");

        var petIds = request.Appointments
            .Select(x => x.PetId)
            .ToList();

        if (petIds.Any(x => x == Guid.Empty) ||
            petIds.Distinct().Count() != petIds.Count)
            throw new ArgumentException(
                "Fiecare animal poate apărea o singură dată în rezervare.");

        var pets = await _dbContext.Pets
            .AsNoTracking()
            .Where(x =>
                petIds.Contains(x.Id) &&
                x.UserId == userId &&
                !x.IsArchived)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (pets.Count != petIds.Count)
            throw new ArgumentException(
                "Unul dintre animale nu aparține utilizatorului sau este arhivat.");

        var packageIds = request.Appointments
            .Where(x => x.Services is not null)
            .SelectMany(x => x.Services)
            .Select(x => x.ServicePackageId)
            .Distinct()
            .ToList();

        if (packageIds.Count == 0 ||
            packageIds.Contains(Guid.Empty))
            throw new ArgumentException(
                "Trebuie selectat cel puțin un pachet valid.");

        var packages = await _dbContext.ServicePackages
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.AnimalSpecies)
            .Include(x => x.Employees)
            .Include(x => x.Items)
                .ThenInclude(x => x.Options)
                    .ThenInclude(x => x.Variants)
            .Where(x =>
                packageIds.Contains(x.Id) &&
                x.BusinessId == request.SalonId &&
                x.IsActive)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (packages.Count != packageIds.Count)
            throw new ArgumentException(
                "Unul dintre pachete este invalid sau inactiv.");

        var nowUtc = DateTime.UtcNow;
        var bookingId = Guid.NewGuid();

        var booking = new Booking
        {
            Id = bookingId,
            BusinessId = request.SalonId,
            UserId = userId,
            BusinessMembershipId = request.BusinessMembershipId,
            Status = BookingStatus.Pending,
            Notes = request.Notes?.Trim(),
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };

        var response = new BookingResponse
        {
            Id = bookingId,
            SalonId = request.SalonId,
            UserId = userId,
            BusinessMembershipId = request.BusinessMembershipId,
            Status = BookingStatus.Pending.ToString(),
            Notes = booking.Notes,
            CreatedAt = nowUtc
        };

        for (var appointmentIndex = 0;
             appointmentIndex < request.Appointments.Count;
             appointmentIndex++)
        {
            var input = request.Appointments[appointmentIndex];
            var pet = pets[input.PetId];

            if (input.StartAtUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException(
                    "Ora programării trebuie transmisă în UTC.");

            if (input.StartAtUtc <= nowUtc)
                throw new ArgumentException(
                    "Nu se pot crea programări în trecut.");

            if (input.Services is null || input.Services.Count == 0)
                throw new ArgumentException(
                    "Fiecare animal trebuie să aibă cel puțin un pachet.");

            if (input.Services
                .Select(x => x.ServicePackageId)
                .Distinct()
                .Count() != input.Services.Count)
                throw new ArgumentException(
                    "Un pachet nu poate fi selectat de două ori pentru același animal.");

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                BookingId = bookingId,
                PetId = pet.Id,
                StartAt = input.StartAtUtc,
                SortOrder = appointmentIndex,
                CreatedAt = nowUtc,
                UpdatedAt = nowUtc
            };

            decimal totalPrice = 0;
            var hasPriceOnRequest = false;
            var totalDuration = 0;

            for (var serviceIndex = 0;
                 serviceIndex < input.Services.Count;
                 serviceIndex++)
            {
                var selected = input.Services[serviceIndex];

                if (!packages.TryGetValue(
                    selected.ServicePackageId,
                    out var package))
                    throw new ArgumentException("Pachetul selectat este invalid.");

                if (!package.AnimalSpecies.Any(x =>
                    x.AnimalSpeciesId == pet.AnimalSpeciesId))
                    throw new ArgumentException(
                        $"Pachetul {package.Name} nu este disponibil pentru animalul {pet.Name}.");

                if (!package.Employees.Any(x =>
                    x.BusinessMembershipId == request.BusinessMembershipId))
                    throw new ArgumentException(
                        $"Angajatul nu poate efectua pachetul {package.Name}.");

                if (package.Items.Count == 0)
                    throw new ArgumentException(
                        $"Pachetul {package.Name} nu conține servicii.");

                var selectedVariantIds =
                    selected.ServicePackageItemVariantIds;

                if (selectedVariantIds is null ||
                    selectedVariantIds.Count != package.Items.Count ||
                    selectedVariantIds.Distinct().Count() !=
                    selectedVariantIds.Count)
                    throw new ArgumentException(
                        $"Selectează câte o variantă pentru fiecare serviciu din pachetul {package.Name}.");

                var appointmentService = new AppointmentService
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointment.Id,
                    ServicePackageId = package.Id,
                    PackageName = package.Name,
                    SortOrder = serviceIndex
                };

                decimal packagePrice = 0;
                var packageHasPriceOnRequest = false;
                var packageDuration = 0;

                foreach (var item in package.Items.OrderBy(x => x.SortOrder))
                {
                    var validVariants = item.Options
                        .Where(x =>
                            x.AnimalSpeciesId == pet.AnimalSpeciesId)
                        .SelectMany(x => x.Variants)
                        .Where(x =>
                            selectedVariantIds.Contains(x.Id))
                        .ToList();

                    if (validVariants.Count != 1)
                        throw new ArgumentException(
                            $"Varianta selectată pentru {item.Name} este invalidă.");

                    var variant = validVariants[0];

                    if (pet.WeightKg.HasValue)
                    {
                        if ((variant.MinWeightKg.HasValue &&
                             pet.WeightKg.Value < variant.MinWeightKg.Value) ||
                            (variant.MaxWeightKg.HasValue &&
                             pet.WeightKg.Value > variant.MaxWeightKg.Value))
                            throw new ArgumentException(
                                $"Varianta {variant.Name} nu corespunde greutății animalului.");
                    }
                    else if (variant.MinWeightKg.HasValue ||
                             variant.MaxWeightKg.HasValue)
                    {
                        throw new ArgumentException(
                            $"Completează greutatea animalului {pet.Name}.");
                    }

                    if (!variant.DurationMinutes.HasValue ||
                        variant.DurationMinutes.Value <= 0)
                        throw new ArgumentException(
                            $"Varianta {variant.Name} nu are o durată validă.");

                    var priceOnRequest = variant.IsPriceOnRequest ||
                        !variant.Price.HasValue;

                    packageDuration = checked(
                        packageDuration + variant.DurationMinutes.Value);

                    if (priceOnRequest)
                        packageHasPriceOnRequest = true;
                    else
                        packagePrice += variant.Price!.Value;

                    appointmentService.Variants.Add(
                        new AppointmentServiceVariant
                        {
                            Id = Guid.NewGuid(),
                            AppointmentServiceId = appointmentService.Id,
                            ServicePackageItemVariantId = variant.Id,
                            ItemName = item.Name,
                            VariantName = variant.Name,
                            Price = variant.Price,
                            IsStartingPrice = variant.IsStartingPrice,
                            IsPriceOnRequest = priceOnRequest,
                            DurationMinutes = variant.DurationMinutes.Value,
                            SortOrder = item.SortOrder
                        });
                }

                appointmentService.DurationMinutes = packageDuration;
                appointmentService.HasPriceOnRequest =
                    packageHasPriceOnRequest;
                appointmentService.TotalPrice =
                    packageHasPriceOnRequest ? null : packagePrice;

                totalDuration = checked(totalDuration + packageDuration);
                totalPrice += packagePrice;
                hasPriceOnRequest |= packageHasPriceOnRequest;

                appointment.Services.Add(appointmentService);
            }

            if (totalDuration is < 1 or > MaxDurationMinutes)
                throw new ArgumentException(
                    "Durata unei programări trebuie să fie între 1 și 720 de minute.");

            appointment.DurationMinutes = totalDuration;
            appointment.EndAt = input.StartAtUtc.AddMinutes(totalDuration);
            appointment.TotalPrice =
                hasPriceOnRequest ? null : totalPrice;
            appointment.HasPriceOnRequest = hasPriceOnRequest;

            appointment.Slot = new AppointmentSlot
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointment.Id,
                BusinessMembershipId = request.BusinessMembershipId,
                StartAt = appointment.StartAt,
                EndAt = appointment.EndAt,
                CreatedAt = nowUtc
            };

            booking.Appointments.Add(appointment);

            response.Appointments.Add(new BookingAppointmentResponse
            {
                Id = appointment.Id,
                PetId = appointment.PetId,
                StartAtUtc = appointment.StartAt,
                EndAtUtc = appointment.EndAt,
                DurationMinutes = appointment.DurationMinutes,
                TotalPrice = appointment.TotalPrice,
                HasPriceOnRequest = appointment.HasPriceOnRequest,
                Services = appointment.Services
                    .Select(x => new BookingServiceResponse
                    {
                        ServicePackageId = x.ServicePackageId,
                        PackageName = x.PackageName,
                        DurationMinutes = x.DurationMinutes,
                        TotalPrice = x.TotalPrice,
                        HasPriceOnRequest = x.HasPriceOnRequest
                    })
                    .ToList()
            });
        }

        var orderedAppointments = booking.Appointments
            .OrderBy(x => x.StartAt)
            .ToList();

        for (var i = 1; i < orderedAppointments.Count; i++)
        {
            if (orderedAppointments[i].StartAt <
                orderedAppointments[i - 1].EndAt)
                throw new ArgumentException(
                    "Programările animalelor nu se pot suprapune.");
        }

        var earliestStart = orderedAppointments[0].StartAt;

        booking.ExpiresAt = new[]
        {
            nowUtc.AddHours(24),
            earliestStart
        }.Min();

        response.ExpiresAt = booking.ExpiresAt;

        booking.History.Add(new BookingHistory
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            PerformedByUserId = userId,
            Action = BookingHistoryAction.Created,
            PreviousStatus = null,
            NewStatus = BookingStatus.Pending,
            Description = "Rezervarea a fost creată.",
            CreatedAt = nowUtc
        });

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            foreach (var appointment in booking.Appointments)
            {
                var salonTimeZoneId = await _dbContext.Businesses
                    .AsNoTracking()
                    .Where(x => x.Id == request.SalonId)
                    .Select(x => x.TimeZoneId)
                    .SingleAsync(cancellationToken);

                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                    salonTimeZoneId);

                var localDate = DateOnly.FromDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(
                        appointment.StartAt,
                        timeZone));

                var availability = await _availability.GetAvailabilityAsync(
                    request.SalonId,
                    request.BusinessMembershipId,
                    localDate,
                    appointment.DurationMinutes,
                    cancellationToken);

                if (!availability.AvailableStartTimesUtc.Contains(
                    appointment.StartAt))
                    throw new InvalidOperationException(
                        "Ora selectată nu mai este disponibilă.");
            }

            _dbContext.Bookings.Add(booking);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (PostgresException ex)
            when (ex.SqlState == "23P01")
        {
            await transaction.RollbackAsync(cancellationToken);

            throw new InvalidOperationException(
                "Intervalul selectat a fost rezervat între timp.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return response;
    }

    public async Task ConfirmBookingAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        await ReviewBookingAsync(
            userId,
            bookingId,
            BookingStatus.Confirmed,
            null,
            cancellationToken);
    }

    public async Task RejectBookingAsync(
        Guid userId,
        Guid bookingId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "Motivul respingerii este obligatoriu.");
        }

        reason = reason.Trim();

        if (reason.Length > 1000)
        {
            throw new ArgumentException(
                "Motivul respingerii nu poate depăși 1000 de caractere.");
        }

        await ReviewBookingAsync(
            userId,
            bookingId,
            BookingStatus.Rejected,
            reason,
            cancellationToken);
    }

    public async Task CancelBookingAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || bookingId == Guid.Empty)
        {
            throw new ArgumentException(
                "Utilizatorul sau rezervarea este invalidă.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

        var booking = await _dbContext.Bookings
            .FromSqlInterpolated(
                $"""
            SELECT *
            FROM "Bookings"
            WHERE "Id" = {bookingId}
            FOR UPDATE
            """)
            .SingleOrDefaultAsync(cancellationToken);

        if (booking is null)
        {
            throw new KeyNotFoundException(
                "Rezervarea nu a fost găsită.");
        }

        if (booking.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "Nu ai permisiunea să anulezi această rezervare.");
        }

        if (booking.Status != BookingStatus.Pending &&
            booking.Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Această rezervare nu mai poate fi anulată.");
        }

        var nowUtc = DateTime.UtcNow;

        var hasStarted = await _dbContext.Appointments
            .AnyAsync(
                x =>
                    x.BookingId == bookingId &&
                    x.StartAt <= nowUtc,
                cancellationToken);

        if (hasStarted)
        {
            throw new InvalidOperationException(
                "Nu poți anula o rezervare care a început deja.");
        }

        var previousStatus = booking.Status;

        booking.Status = BookingStatus.Cancelled;
        booking.ExpiresAt = null;
        booking.UpdatedAt = nowUtc;

        var appointmentIds = await _dbContext.Appointments
            .AsNoTracking()
            .Where(x => x.BookingId == bookingId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        await _dbContext.AppointmentSlots
            .Where(x => appointmentIds.Contains(x.AppointmentId))
            .ExecuteDeleteAsync(cancellationToken);

        _dbContext.BookingHistories.Add(new BookingHistory
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            PerformedByUserId = userId,
            Action = BookingHistoryAction.Cancelled,
            PreviousStatus = previousStatus,
            NewStatus = BookingStatus.Cancelled,
            Description = "Rezervarea a fost anulată de client.",
            CreatedAt = nowUtc
        });

        await _dbContext.BookingChangeRequests
            .Where(x =>
                x.BookingId == bookingId &&
                x.Status == BookingChangeRequestStatus.Pending)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.Status,
                        BookingChangeRequestStatus.Cancelled)
                    .SetProperty(
                        x => x.UpdatedAt,
                        nowUtc),
                cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task ReviewBookingAsync(
        Guid userId,
        Guid bookingId,
        BookingStatus newStatus,
        string? rejectionReason,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty || bookingId == Guid.Empty)
        {
            throw new ArgumentException(
                "Utilizatorul sau rezervarea este invalidă.");
        }

        if (newStatus != BookingStatus.Confirmed &&
            newStatus != BookingStatus.Rejected)
        {
            throw new ArgumentException(
                "Statusul solicitat este invalid.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

        var booking = await _dbContext.Bookings
            .FromSqlInterpolated(
                $"""
            SELECT *
            FROM "Bookings"
            WHERE "Id" = {bookingId}
            FOR UPDATE
            """)
            .SingleOrDefaultAsync(cancellationToken);

        if (booking is null)
        {
            throw new KeyNotFoundException(
                "Rezervarea nu a fost găsită.");
        }

        var authorized = await _dbContext.BusinessMemberships
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.BusinessId == booking.BusinessId &&
                    x.UserId == userId &&
                    x.IsActive &&
                    x.RemovedAt == null &&
                    x.User.IsActive &&
                    (
                        x.Role == BusinessRole.Owner ||
                        x.Role == BusinessRole.Receptionist ||
                        (
                            x.Role == BusinessRole.Groomer &&
                            x.Id == booking.BusinessMembershipId
                        )
                    ),
                cancellationToken);

        if (!authorized)
        {
            throw new UnauthorizedAccessException(
                "Nu ai permisiunea de a gestiona această rezervare.");
        }

        var nowUtc = DateTime.UtcNow;

        if (booking.Status != BookingStatus.Pending ||
            !booking.ExpiresAt.HasValue ||
            booking.ExpiresAt.Value <= nowUtc)
        {
            throw new InvalidOperationException(
                "Rezervarea nu mai este în așteptare sau a expirat.");
        }

        var hasStarted = await _dbContext.Appointments
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.BookingId == bookingId &&
                    x.StartAt <= nowUtc,
                cancellationToken);

        if (hasStarted)
        {
            throw new InvalidOperationException(
                "Nu poți confirma sau respinge o rezervare care a început deja.");
        }

        booking.Status = newStatus;
        booking.ReviewedByUserId = userId;
        booking.ReviewedAt = nowUtc;
        booking.RejectionReason = rejectionReason;
        booking.ExpiresAt = null;
        booking.UpdatedAt = nowUtc;

        if (newStatus == BookingStatus.Rejected)
        {
            var appointmentIds = await _dbContext.Appointments
                .AsNoTracking()
                .Where(x => x.BookingId == bookingId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            await _dbContext.AppointmentSlots
                .Where(x => appointmentIds.Contains(x.AppointmentId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        _dbContext.BookingHistories.Add(new BookingHistory
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            PerformedByUserId = userId,
            Action = newStatus == BookingStatus.Confirmed
                ? BookingHistoryAction.Confirmed
                : BookingHistoryAction.Rejected,
            PreviousStatus = BookingStatus.Pending,
            NewStatus = newStatus,
            Description = newStatus == BookingStatus.Confirmed
                ? "Rezervarea a fost confirmată de salon."
                : $"Rezervarea a fost respinsă. Motiv: {rejectionReason}",
            CreatedAt = nowUtc
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

}
