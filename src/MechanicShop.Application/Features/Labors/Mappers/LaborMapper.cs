using MechanicShop.Application.Features.Labors.Dtos;

namespace MechanicShop.Application.Features.Labors.Mappers;

public static class LaborMapper
{
    public static LaborDto ToDto(this Employee employee)
        => new LaborDto{ LaborId = employee.Id , Name = employee.FullName};

    public static List<LaborDto> ToDtos(this IEnumerable<Employee> entities)
        => [.. entities.Select(e => e.ToDto())];
}