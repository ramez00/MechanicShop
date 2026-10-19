using MechanicShop.Application.Features.BookableServices.Mappers;
using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.UnitTests.Mappers;

public class BookableServiceMapperTests
{
    [Fact]
    public void Services_query_maps_duration_and_price()
    {
        var repairTask = new RepairTaskBuilder()
            .WithName("Brake pads")
            .WithLaborCost(100m)
            .WithDuration(RepairDurationInMinutes.Min45)
            .WithParts(
                new PartBuilder().WithId(Guid.NewGuid()).WithPrice(25m).WithQuantity(2).Build(),
                new PartBuilder().WithId(Guid.NewGuid()).WithPrice(10m).WithQuantity(1).Build())
            .Build();

        var dto = repairTask.ToBookableServiceDto();

        Assert.Equal(repairTask.Id, dto.Id);
        Assert.Equal("Brake pads", dto.Name);
        Assert.Equal(45, dto.EstimatedDurationInMins);
        Assert.Equal(100m, dto.LaborCost);
        Assert.Equal(160m, dto.EstimatedTotal); // labor 100 + parts (2 x 25 + 1 x 10)
    }

    [Fact]
    public void Service_without_parts_costs_labor_only()
    {
        var dto = new RepairTaskBuilder().WithLaborCost(80m).WithNoParts().Build().ToBookableServiceDto();

        Assert.Equal(80m, dto.EstimatedTotal);
    }
}
