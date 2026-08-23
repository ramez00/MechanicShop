using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;


namespace MechanicShop.Application.Features.WorkOrders.Mappers;

public static class WorkOrderMapper
{
   public static WorkOrderDto ToDto(this WorkOrder entity)
    {
        return new WorkOrderDto
        {
            WorkOrderId = entity.Id,
            Spot = entity.Spot,
            StartAtUtc = entity.StartAtUtc,
            EndAtUtc = entity.EndAtUtc,
            Labor = entity.Labor is null ? null : new LaborDto
            {
                LaborId = entity.LaborId,
                Name = $"{entity.Labor.FirstName} {entity.Labor.LastName}"
            },
            RepairTasks = entity.RepairTasks.ToDtos(),
            Vehicle = entity.Car is null ? null : entity.Car.ToDto(),
            State = entity.State,
            TotalPartCost = entity.RepairTasks.SelectMany(t => t.Parts).Sum(p => p.Price * p.Quantity),
            TotalLaborCost = entity.RepairTasks.Sum(p => p.LaborCost),
            TotalCost = entity.RepairTasks.Sum(rt => rt.totalCost),
            TotalDurationInMins = entity.RepairTasks.Sum(rt => (int)rt.EstimatedDurationInMins),
            InvoiceId = entity.Invoice?.Id,
            CreatedAt = entity.CreatedAtUtc
        };
    }

    public static List<WorkOrderDto> ToDtos(this IEnumerable<WorkOrder> entities)
        => [.. entities.Select(e => e.ToDto())];

    public static WorkOrderListItemDto ToListItemDto(this WorkOrder entity)
    {
        return new WorkOrderListItemDto
        {
            WorkOrderId = entity.Id,
            Spot = entity.Spot,
            StartAtUtc = entity.StartAtUtc,
            EndAtUtc = entity.EndAtUtc,
            car = entity.Car!.ToDto(),
            Labor = entity.Labor is null ? null :
                $"{entity.Labor.FirstName} {entity.Labor.LastName}",
            State = entity.State,
            RepairTasks = entity.RepairTasks.Select(rt => rt.Name).ToList()
        };
    }
}