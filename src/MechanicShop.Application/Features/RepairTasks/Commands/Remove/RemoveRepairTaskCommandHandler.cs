using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.Features.RepairTasks.Commands.Remove;

public class RemoveRepairTaskCommandHandler(IAppDbContext context,HybridCache cache) : IRequestHandler<RemoveRepairTaskCommand, Result<Deleted>>
{
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    public async Task<Result<Deleted>> Handle(RemoveRepairTaskCommand request, CancellationToken cancellationToken)
    {
        var repairTask = await _context.RepairTasks.FindAsync(request.RepairTaskId);

        if (repairTask is null)
            return ApplicationErrors.RepairTaskNotFound;

        var isInUse = await _context.WorkOrders
                                    .AsNoTracking()
                                    .Where(w => w.State == WorkOrderState.InProgress)
                                    .SelectMany(w => w.RepairTasks)
                                    .AnyAsync(r => r.Id == request.RepairTaskId,cancellationToken);

        if(isInUse)
            return RepairTaskErrors.InUse;

        _context.RepairTasks.Remove(repairTask);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.deleted;                                    
    }
}