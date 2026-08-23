using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Enums;
using MechanicShop.Domain.workOrders.Events;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrder;

public class UpdateWorkOrderStateCommandHandler(
    IAppDbContext context,
    HybridCache cache 
) : IRequestHandler<UpdateWorkOrderStateCommand, Result<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    public async Task<Result<Updated>> Handle(UpdateWorkOrderStateCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.Id == request.WorkOrderId);

        if(workOrder is null)
            return ApplicationErrors.WorkOrderNotFound;

        var updateState = workOrder.UpdateState(request.WorkOrderState);

        if(updateState.IsError)
            return updateState.Errors!;

        if(request.WorkOrderState == WorkOrderState.Completed)
            workOrder.AddDomainEvent(new WorkOrderCompleted{ WorkOrderId =  request.WorkOrderId });
        
        await _context.SaveChangesAsync(cancellationToken);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());
        
        await _cache.RemoveByTagAsync("work-order",cancellationToken);

        return Result.updated;
    }
}