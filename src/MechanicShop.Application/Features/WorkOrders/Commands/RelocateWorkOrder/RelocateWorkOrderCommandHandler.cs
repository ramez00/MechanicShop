using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Events;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;

public class RelocateWorkOrderCommandHandler(
    IAppDbContext context,
    IWorkOrderPolicy workOrderValidator,
    HybridCache cache
) : IRequestHandler<RelocateWorkOrderCommand, Result<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly IWorkOrderPolicy _workOrderValidator = workOrderValidator;
    private readonly HybridCache _cache = cache;
    public async Task<Result<Updated>> Handle(RelocateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
                                      .Include(a => a.RepairTasks)
                                      .Include(a => a.Labor)
                                      .Include(a => a.Car)
                                      .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId);

        if(workOrder is null)
           return ApplicationErrors.WorkOrderNotFound;

        var duration = workOrder.EndAtUtc.Subtract(workOrder.StartAtUtc).Duration();

        var endAt = request.NewStartAt.Add(duration);

        if(_workOrderValidator.IsOutsideOperatingHours(request.NewStartAt, duration))
            return Error.Conflict("WorkOrder_Outside_OperatingHours", "WorkOrder timing exceeds business hours.");

        var spotCheckResult = await _workOrderValidator.CheckSpotAvailabilityAsync(request.NewSpot,request.NewStartAt,endAt,workOrder.Id,cancellationToken);

        if(spotCheckResult.IsError)
           return spotCheckResult.Errors!;

        if (await _workOrderValidator.IsLaborOccupied(workOrder.LaborId, request.WorkOrderId, request.NewStartAt, endAt))
            return ApplicationErrors.LaborOccupied;

        if(await _workOrderValidator.IsVehicleAlreadyScheduled(workOrder.CarId,request.NewStartAt,endAt,request.WorkOrderId))
            return ApplicationErrors.VehicleSchedulingConflict;

        var updateTiming = workOrder.UpdateTiming(request.NewStartAt, endAt);

        if(updateTiming.IsError)
            return updateTiming.Errors!;

        var updateSpot = workOrder.UpdateSpot(request.NewSpot);

        if(updateSpot.IsError)
            return updateSpot.Errors!;

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("work-order", cancellationToken);

        return Result.updated;
    }
}