using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Events;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public class UpdateWorkOrderRepairTasksCommandHandler(
    IAppDbContext context,
    IWorkOrderPolicy workOrderValidator,
    HybridCache cache
) : IRequestHandler<UpdateWorkOrderRepairTasksCommand, Result<Updated>>
{
    private readonly IAppDbContext _context = context;

    private readonly IWorkOrderPolicy _workOrderValidator = workOrderValidator;

    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(UpdateWorkOrderRepairTasksCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await  _context.WorkOrders
                                       .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId,cancellationToken);

        if(workOrder is null)
            return ApplicationErrors.WorkOrderNotFound;

        if(request.RepairTasksId.Length == 0)
            return RepairTaskErrors.AtLeastOneRepairTaskIsRequired;

        var RepairTasks = await _context.RepairTasks
                                        .Where(r => request.RepairTasksId.Contains(r.Id))
                                        .ToListAsync(cancellationToken);
       
        foreach(var task in RepairTasks)
        {
            var addedTask = workOrder.AddRepairTask(task);

            if(addedTask.IsError)
                return addedTask.Errors!;
        }

        var totalDuration = TimeSpan.FromMinutes(RepairTasks.Sum(r => (int)r.EstimatedDurationInMins));

        var endAt = workOrder.StartAtUtc + totalDuration ;

        var isWorkOrderInValidBusinessHour = _workOrderValidator.IsOutsideOperatingHours(workOrder.StartAtUtc,totalDuration);

        if(isWorkOrderInValidBusinessHour)
            return Error.Conflict("WorkOrder_Outside_OperatingHours", "WorkOrder timing exceeds business hours.");

        var spotCheckResult = await _workOrderValidator.CheckSpotAvailabilityAsync(workOrder.Spot,workOrder.StartAtUtc,endAt,workOrder.Id,cancellationToken);

        if(spotCheckResult.IsError)
            return spotCheckResult.Errors!;

        var isLaborOccupied = await _workOrderValidator.IsLaborOccupied(workOrder.LaborId,workOrder.Id,workOrder.StartAtUtc,endAt);

        if(isLaborOccupied)
            return ApplicationErrors.LaborOccupied;

        workOrder.UpdateTiming(workOrder.StartAtUtc,endAt);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangesAsync(cancellationToken);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _cache.RemoveByTagAsync("work-order",cancellationToken);

        return Result.updated;
    }
}