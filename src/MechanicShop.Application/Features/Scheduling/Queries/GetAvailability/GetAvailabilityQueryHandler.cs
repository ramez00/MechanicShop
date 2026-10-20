using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.workOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Scheduling.Queries.GetAvailability;

public sealed class GetAvailabilityQueryHandler(
    IAppDbContext context,
    IShopSettings settings,
    TimeProvider timeProvider
) : IRequestHandler<GetAvailabilityQuery, Result<AvailabilityResponse>>
{
    private static readonly TimeSpan Step = TimeSpan.FromMinutes(15);

    public async Task<Result<AvailabilityResponse>> Handle(GetAvailabilityQuery query, CancellationToken ct)
    {
        // 1. Load repair tasks and compute total duration
        var repairTasks = await context.RepairTasks
            .Where(rt => query.RepairTaskIds.Contains(rt.Id))
            .ToListAsync(ct);

        if (repairTasks.Count != query.RepairTaskIds.Count)
            return Error.NotFound("RepairTask_NotFound", "One or more repair tasks were not found.");

        var durationMinutes = repairTasks.Sum(rt => (int)rt.EstimatedDurationInMins);
        var duration = TimeSpan.FromMinutes(durationMinutes);

        // 2. Compute UTC window for the requested date in the caller's timezone
        var openingLocal = query.Date.ToDateTime(settings.OpeningTime);
        var closingLocal = query.Date.ToDateTime(settings.ClosingTime);
        var openingUtc = TimeZoneInfo.ConvertTimeToUtc(openingLocal, query.TimeZone);
        var closingUtc = TimeZoneInfo.ConvertTimeToUtc(closingLocal, query.TimeZone);
        var windowLength = closingUtc - openingUtc;

        // 3. Validate duration fits inside operating window
        if (duration > windowLength)
            return new AvailabilityResponse(
                [],
                $"The total service duration of {durationMinutes} min exceeds the {(int)windowLength.TotalMinutes}-min operating window.");

        // 4. Count available mechanics
        var totalMechanics = await context.Employees
            .CountAsync(e => e.Role == Role.Mechanic, ct);

        if (totalMechanics == 0)
            return new AvailabilityResponse([], "No mechanics are currently available.");

        // 5. Load day's non-Cancelled work orders (single query)
        var workOrders = await context.WorkOrders
            .Where(wo =>
                wo.State != WorkOrderState.Cancelled &&
                wo.StartAtUtc < closingUtc &&
                wo.EndAtUtc > openingUtc)
            .Select(wo => new { wo.StartAtUtc, wo.EndAtUtc, wo.Spot })
            .ToListAsync(ct);

        // 6. Enumerate 15-min candidate starts
        var nowUtc = timeProvider.GetUtcNow();
        var leadTime = TimeSpan.FromMinutes(settings.BookingLeadTimeMinutes);
        var slots = new List<BookingSlotDto>();

        for (var slotStart = openingUtc; slotStart + duration <= closingUtc; slotStart += Step)
        {
            if (slotStart < nowUtc + leadTime)
                continue;

            var slotEnd = slotStart + duration;
            var available = true;

            // Check every 15-min sub-slot within [slotStart, slotEnd)
            for (var sub = slotStart; sub < slotEnd; sub += Step)
            {
                var subEnd = sub + Step;

                var overlapping = workOrders
                    .Where(wo => wo.StartAtUtc < subEnd && wo.EndAtUtc > sub)
                    .ToList();

                var busySpots = overlapping.Select(wo => wo.Spot).Distinct().Count();
                var busyMechanics = overlapping.Count;

                if (settings.MaxSpots - busySpots < 1 || totalMechanics - busyMechanics < 1)
                {
                    available = false;
                    break;
                }
            }

            if (available)
                slots.Add(new BookingSlotDto(slotStart, slotEnd));
        }

        var message = slots.Count == 0
            ? "No availability for the selected date and services."
            : null;

        return new AvailabilityResponse(slots, message);
    }
}
