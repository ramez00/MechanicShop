using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Appointments.Commands.ApproveAppointmentRequest;

public sealed class ApproveAppointmentRequestCommandHandler(
    IAppDbContext context,
    IWorkOrderPolicy workOrderPolicy,
    TimeProvider timeProvider,
    HybridCache cache,
    ILogger<ApproveAppointmentRequestCommandHandler> logger
) : IRequestHandler<ApproveAppointmentRequestCommand, Result<Guid>>
{
    private readonly IAppDbContext _context = context;
    private readonly IWorkOrderPolicy _workOrderPolicy = workOrderPolicy;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<ApproveAppointmentRequestCommandHandler> _logger = logger;

    public async Task<Result<Guid>> Handle(ApproveAppointmentRequestCommand command, CancellationToken ct)
    {
        var appointmentRequest = await _context.AppointmentRequests
            .Include(ar => ar.RepairTasks)
            .FirstOrDefaultAsync(ar => ar.Id == command.Id, ct);

        if (appointmentRequest is null)
            return ApplicationErrors.AppointmentNotFound;

        var labor = await _context.Employees.FindAsync([command.LaborId], ct);

        if (labor is null)
            return ApplicationErrors.LaborNotFound;

        var startAt = appointmentRequest.RequestedStartAtUtc;
        var endAt = appointmentRequest.RequestedEndAtUtc;

        var spotCheck = await _workOrderPolicy.CheckSpotAvailabilityAsync(
            command.Spot, startAt, endAt, excludeWorkOrderId: null, ct);

        if (spotCheck.IsError)
        {
            _logger.LogWarning("Spot {Spot} not available for appointment {Id}", command.Spot, command.Id);
            return ApplicationErrors.AppointmentSlotTakenOnApproval;
        }

        var isLaborOccupied = await _context.WorkOrders
            .AnyAsync(w =>
                w.LaborId == command.LaborId &&
                w.StartAtUtc < endAt &&
                w.EndAtUtc > startAt,
                ct);

        if (isLaborOccupied)
        {
            _logger.LogWarning("Labor {LaborId} occupied during appointment {Id}", command.LaborId, command.Id);
            return ApplicationErrors.LaborOccupied;
        }

        var workOrderId = Guid.NewGuid();

        var createResult = WorkOrder.Create(
            workOrderId,
            appointmentRequest.CarId,
            startAt,
            endAt,
            command.LaborId,
            command.Spot,
            WorkOrderState.Scheduled,
            appointmentRequest.RepairTasks.ToList());

        if (createResult.IsError)
            return createResult.Errors!;

        var workOrder = createResult.Value;
        _context.WorkOrders.Add(workOrder);

        var approveResult = appointmentRequest.Approve(
            workOrderId, command.ReviewedBy, _timeProvider.GetUtcNow());

        if (approveResult.IsError)
            return approveResult.Errors!;

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("work-order", ct);
        await _cache.RemoveByTagAsync("appointment-request", ct);

        _logger.LogInformation("Appointment {Id} approved, WorkOrder {WorkOrderId} created",
            command.Id, workOrderId);

        return workOrderId;
    }
}
