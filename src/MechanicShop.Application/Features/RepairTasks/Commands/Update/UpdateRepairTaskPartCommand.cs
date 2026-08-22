using MediatR;

namespace MechanicShop.Application.Features.RepairTasks.Commands.Update;

public sealed record UpdateRepairTaskPartCommand(
    Guid? PartId,
    string Name,
    decimal Cost,
    int Quantity
);