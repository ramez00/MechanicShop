using MechanicShop.Application.Features.BookableServices.Dtos;

namespace MechanicShop.Application.Features.BookableServices.Mappers;

public static class BookableServiceMapper
{
    // EstimatedTotal is labor plus parts (RepairTask.totalCost), so Parts must be loaded.
    public static BookableServiceDto ToBookableServiceDto(this RepairTask repairTask)
        => new(
            repairTask.Id,
            repairTask.Name,
            (int)repairTask.EstimatedDurationInMins,
            repairTask.LaborCost,
            repairTask.totalCost);

    public static List<BookableServiceDto> ToBookableServiceDtos(this IEnumerable<RepairTask> repairTasks)
        => [.. repairTasks.Select(ToBookableServiceDto)];
}
