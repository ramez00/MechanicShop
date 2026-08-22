using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace MechanicShop.Application.Features.RepairTasks.Commands.Update;

public class UpdateRepairTaskCommandHandler(
    IAppDbContext context,
    HybridCache cache
) : IRequestHandler<UpdateRepairTaskCommand, Result<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(UpdateRepairTaskCommand request, CancellationToken cancellationToken)
    {
        var repairTask = await _context.RepairTasks
                                        .Include(r => r.Parts)
                                        .FirstOrDefaultAsync(r => r.Id == request.RepairTaskId,cancellationToken);

        if(repairTask is null)
            return ApplicationErrors.RepairTaskNotFound;

        var usedParts = new List<Part>();

        foreach(var p in request.Parts)
        {
            var partId = Guid.NewGuid();

            var part = Part.Create(partId,p.Name,p.Cost,p.Quantity);

            if(part.IsError)
                return part.Errors!;

            usedParts.Add(part.Value);    
        }

        var updatedRepairTask = repairTask.Update(request.Name,request.LaborCost,request.EstimatedDurationInMins);

        if(updatedRepairTask.IsError)
            return updatedRepairTask.Errors!;

        var updatedPart = repairTask.UpsertParts(usedParts);

        if(updatedPart.IsError)
            return updatedPart.Errors!;

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync("repair-task",cancellationToken);

        return Result.updated;        
    }
}