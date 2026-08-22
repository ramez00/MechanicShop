using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasksById;

public class GetRepairTaskByIdQueryHandler(
    IAppDbContext context
) : IRequestHandler<GetRepairTaskByIdQuery, Result<RepairTaskDto>>
{
    private readonly IAppDbContext _context = context ;
    public async Task<Result<RepairTaskDto>> Handle(GetRepairTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var repairTask = await _context.RepairTasks
                                        .Include(r => r.Parts)
                                        .FirstOrDefaultAsync(r => r.Id == request.TaskId,cancellationToken);

        if(repairTask is null)
            return ApplicationErrors.RepairTaskNotFound;

        return repairTask.ToDto();
    }
}