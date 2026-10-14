using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Events;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace MechanicShop.Application.Features.WorkOrders.AssignLabor;

public class AssignLaborCommandHandler(
    IAppDbContext context,
    IWorkOrderPolicy workOrderPolicy,
    HybridCache cache
) : IRequestHandler<AssignLaborCommand, Result<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly IWorkOrderPolicy _workOrderPolicy = workOrderPolicy;
    private readonly HybridCache _cache = cache;
    public async Task<Result<Updated>> Handle(AssignLaborCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.Id == request.WorkOrderId);

        if(workOrder is null)
          return ApplicationErrors.WorkOrderNotFound;

        var isLaborExist = await _context.Employees.AnyAsync(e => e.Id == request.LaborId);

        if(!isLaborExist)
           return ApplicationErrors.LaborNotFound;

        if(await _workOrderPolicy.IsLaborOccupied(request.LaborId,request.WorkOrderId,workOrder.StartAtUtc,workOrder.EndAtUtc))
           return ApplicationErrors.LaborOccupied;

        var updateLabor = workOrder.UpdateLabor(request.LaborId);

        if(updateLabor.IsError)
          return updateLabor.Errors!;

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("work-order", cancellationToken);

        return Result.updated;
    }
}
