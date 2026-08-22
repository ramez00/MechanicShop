using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;

public class GetRepairTasksQueryHandler(
    IAppDbContext context
) : IRequestHandler<GetRepairTasksQuery, Result<List<RepairTaskDto>>>
{
    private readonly IAppDbContext _context = context;
    public async Task<Result<List<RepairTaskDto>>> Handle(GetRepairTasksQuery request, CancellationToken cancellationToken)
    {
        var repairTasks = await _context.RepairTasks
                                  .Include(r => r.Parts)
                                  .AsNoTracking()
                                  .ToListAsync(cancellationToken);

        return repairTasks.ToDtos(); 
    }
}