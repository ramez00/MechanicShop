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

        var isSpotOccuiped = await _workOrderValidator.CheckSpotAvailabilityAsync(request.NewSpot,workOrder.StartAtUtc,workOrder.EndAtUtc,workOrder.Id);

        if(isSpotOccuiped.IsError)
           return WorkOrderErrors.SpotInvalid;

        var duration = workOrder.EndAtUtc.Subtract(workOrder.StartAtUtc).Duration();

        var endAt = request.NewStartAt.Add(duration);

        if (await _workOrderValidator.IsLaborOccupied(workOrder.LaborId, request.WorkOrderId, request.NewStartAt, endAt))
            return ApplicationErrors.LaborOccupied;

        if(await _workOrderValidator.IsVehicleAlreadyScheduled(workOrder.CarId,workOrder.StartAtUtc,endAt,request.WorkOrderId))
            return ApplicationErrors.VehicleSchedulingConflict;

        var updateSpot = workOrder.UpdateSpot(request.NewSpot);

        if(updateSpot.IsError)
            return updateSpot.Errors!;

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangesAsync(cancellationToken);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());        

        await _cache.RemoveByTagAsync("work-order", cancellationToken);

        return Result.updated;
    }
}