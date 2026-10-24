using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.AppointmentRequests;
using MechanicShop.Domain.AppointmentRequests.Enums;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace MechanicShop.Application.Features.Appointments.Commands.SubmitAppointmentRequest;

public sealed class SubmitAppointmentRequestCommandHandler(
    IAppDbContext context,
    IShopSettings settings,
    TimeProvider timeProvider,
    HybridCache cache
) : IRequestHandler<SubmitAppointmentRequestCommand, Result<Guid>>
{
    private static readonly TimeSpan Step = TimeSpan.FromMinutes(15);

    public async Task<Result<Guid>> Handle(SubmitAppointmentRequestCommand command, CancellationToken ct)
    {
        // 1. Load car and verify ownership
        var car = await context.Vehicles
            .FirstOrDefaultAsync(c => c.Id == command.CarId, ct);

        if (car is null)
            return ApplicationErrors.VehicleNotFound;

        if (car.CustomerId != command.CustomerId)
            return ApplicationErrors.AppointmentCarNotOwned;

        // 2. Load repair tasks and verify all exist
        var repairTasks = await context.RepairTasks
            .Where(rt => command.RepairTaskIds.Contains(rt.Id))
            .ToListAsync(ct);

        if (repairTasks.Count != command.RepairTaskIds.Count)
            return ApplicationErrors.RepairTaskNotFound;

        // 3. Compute duration and end time
        var durationMinutes = repairTasks.Sum(rt => (int)rt.EstimatedDurationInMins);
        var duration = TimeSpan.FromMinutes(durationMinutes);
        var requestedStartUtc = command.RequestedStartAt.UtcDateTime;
        var requestedEndUtc = requestedStartUtc + duration;

        // 4. Validate lead time
        var nowUtc = timeProvider.GetUtcNow();
        var leadTime = TimeSpan.FromMinutes(settings.BookingLeadTimeMinutes);

        if (requestedStartUtc < nowUtc + leadTime)
            return Error.Validation("Appointment_LeadTime",
                $"Appointments must be booked at least {settings.BookingLeadTimeMinutes} minutes in advance.");

        // 5. Validate booking horizon
        var today = DateOnly.FromDateTime(nowUtc.UtcDateTime);
        var requestedDate = DateOnly.FromDateTime(requestedStartUtc);

        if (requestedDate > today.AddDays(settings.BookingHorizonDays))
            return Error.Validation("Appointment_HorizonExceeded",
                $"Appointments cannot be booked more than {settings.BookingHorizonDays} days in advance.");

        // 6. Re-check availability for [requestedStartUtc, requestedEndUtc)
        var totalMechanics = await context.Employees
            .CountAsync(e => e.Role == Role.Mechanic, ct);

        // Load overlapping non-cancelled work orders
        var workOrders = await context.WorkOrders
            .Where(wo =>
                wo.State != WorkOrderState.Cancelled &&
                wo.StartAtUtc < requestedEndUtc &&
                wo.EndAtUtc > requestedStartUtc)
            .Select(wo => new { wo.StartAtUtc, wo.EndAtUtc, wo.Spot })
            .ToListAsync(ct);

        // Load overlapping pending appointment requests
        var pendingRequests = await context.AppointmentRequests
            .Where(ar =>
                ar.Status == AppointmentRequestStatus.Pending &&
                ar.RequestedStartAtUtc < requestedEndUtc &&
                ar.RequestedEndAtUtc > requestedStartUtc)
            .Select(ar => new { ar.RequestedStartAtUtc, ar.RequestedEndAtUtc })
            .ToListAsync(ct);

        // Check every 15-min sub-slot within [requestedStartUtc, requestedEndUtc)
        for (var sub = requestedStartUtc; sub < requestedEndUtc; sub += Step)
        {
            var subEnd = sub + Step;

            var overlappingWos = workOrders
                .Where(wo => wo.StartAtUtc < subEnd && wo.EndAtUtc > sub)
                .ToList();

            var busySpots = overlappingWos.Select(wo => wo.Spot).Distinct().Count();
            var busyMechanicsFromWos = overlappingWos.Count;

            var busyMechanicsFromRequests = pendingRequests
                .Count(ar => ar.RequestedStartAtUtc < subEnd && ar.RequestedEndAtUtc > sub);

            var totalBusyMechanics = busyMechanicsFromWos + busyMechanicsFromRequests;

            if (settings.MaxSpots - busySpots < 1 || totalMechanics - totalBusyMechanics < 1)
                return ApplicationErrors.AppointmentSlotNoLongerAvailable;
        }

        // 7. Create the appointment request
        var requestResult = AppointmentRequest.Create(
            Guid.NewGuid(),
            command.CustomerId,
            command.CarId,
            new DateTimeOffset(requestedStartUtc, TimeSpan.Zero),
            new DateTimeOffset(requestedEndUtc, TimeSpan.Zero),
            repairTasks,
            command.Notes);

        if (requestResult.IsError)
            return requestResult.Errors!;

        // 8. Persist
        context.AppointmentRequests.Add(requestResult.Value);
        await context.SaveChangesAsync(ct);

        // 9. Invalidate cache
        await cache.RemoveByTagAsync("appointment-request", ct);

        // 10. Return the new ID
        return requestResult.Value.Id;
    }
}
