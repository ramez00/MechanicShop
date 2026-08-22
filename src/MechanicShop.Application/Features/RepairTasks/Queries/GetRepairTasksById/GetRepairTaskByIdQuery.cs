using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasksById;

public sealed record GetRepairTaskByIdQuery(
    Guid TaskId
) : IRequest<Result<RepairTaskDto>>;