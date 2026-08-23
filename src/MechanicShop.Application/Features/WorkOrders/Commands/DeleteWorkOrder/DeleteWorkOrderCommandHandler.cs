using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Events;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;

public class DeleteWorkOrderCommandHandler(
    IAppDbContext context,
    HybridCache cache
) : IRequestHandler<DeleteWorkOrderCommand, Result<Deleted>>
{
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Deleted>> Handle(DeleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
                                .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId,cancellationToken);

        if (workOrder is null)
            return ApplicationErrors.WorkOrderNotFound;
            
        if(workOrder.State is not WorkOrderState.Scheduled)
            return WorkOrderErrors.Readonly;

        _context.WorkOrders.Remove(workOrder);

        await _context.SaveChangesAsync(cancellationToken); 

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());                         

        await _cache.RemoveByTagAsync("work-order", cancellationToken);

        return Result.deleted;
    }
}