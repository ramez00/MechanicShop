using MechanicShop.Application.Features.RepairTasks.Dtos;

namespace MechanicShop.Application.Features.RepairTasks.Mappers;

public static class RepairTaskMapper
{
    public static RepairTaskDto ToDto(this RepairTask entity)
    {
        return new RepairTaskDto
        {
            RepairTaskId = entity.Id,
            Name = entity.Name!,
            LaborCost = entity.LaborCost,
            TotalCost = entity.totalCost,
            EstimatedDurationInMins = entity.EstimatedDurationInMins,
            Parts = entity.Parts.ToList().ConvertAll(ToDto)
        };
    }

    public static List<RepairTaskDto> ToDtos(this List<RepairTask> entities)
        => [..entities.Select(e => e.ToDto())];

    public static PartDto ToDto(this Part entity)
    {
        return new PartDto
        {
             PartId = entity.Id,
            Name = entity.Name!,
            Cost = entity.Price,
            Quantity = entity.Quantity
        };
    }

    public static List<PartDto> ToDtos(List<Part> entities)
        => [..entities.Select(e => e.ToDto())];
}