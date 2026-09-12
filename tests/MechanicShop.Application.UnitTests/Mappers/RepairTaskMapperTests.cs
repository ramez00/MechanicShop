using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.UnitTests.Mappers;

public class RepairTaskMapperTests
{
    [Fact]
    public void ToDto_ShouldMapFieldsTotalCostAndParts()
    {
        var part = new PartBuilder().WithPrice(25m).WithQuantity(2).Build();
        var repairTask = new RepairTaskBuilder()
            .WithName("Brake Service")
            .WithLaborCost(120m)
            .WithDuration(RepairDurationInMinutes.Min45)
            .WithParts(part)
            .Build();

        var dto = repairTask.ToDto();

        Assert.Equal(repairTask.Id, dto.RepairTaskId);
        Assert.Equal("Brake Service", dto.Name);
        Assert.Equal(120m, dto.LaborCost);
        Assert.Equal(RepairDurationInMinutes.Min45, dto.EstimatedDurationInMins);
        // totalCost = parts (25 * 2) + labor 120 = 170
        Assert.Equal(170m, dto.TotalCost);
        Assert.Single(dto.Parts);
    }

    [Fact]
    public void ToDto_ForPart_ShouldMapAllFields()
    {
        var part = new PartBuilder()
            .WithName("Oil Filter")
            .WithPrice(25m)
            .WithQuantity(3)
            .Build();

        var dto = part.ToDto();

        Assert.Equal(part.Id, dto.PartId);
        Assert.Equal("Oil Filter", dto.Name);
        Assert.Equal(25m, dto.Cost);
        Assert.Equal(3, dto.Quantity);
    }

    [Fact]
    public void ToDtos_ForRepairTasks_ShouldMapEach()
    {
        var tasks = new[]
        {
            new RepairTaskBuilder().WithId(Guid.NewGuid()).WithName("Task A")
                .WithParts(new PartBuilder().WithId(Guid.NewGuid()).Build()).Build(),
            new RepairTaskBuilder().WithId(Guid.NewGuid()).WithName("Task B")
                .WithParts(new PartBuilder().WithId(Guid.NewGuid()).Build()).Build(),
        };

        var dtos = tasks.ToDtos();

        Assert.Equal(2, dtos.Count);
        Assert.Equal("Task A", dtos[0].Name);
        Assert.Equal("Task B", dtos[1].Name);
    }

    [Fact]
    public void ToDtos_ForParts_ShouldMapEach()
    {
        var repairTask = new RepairTaskBuilder()
            .WithParts(
                new PartBuilder().WithId(Guid.NewGuid()).WithName("Part 1").Build(),
                new PartBuilder().WithId(Guid.NewGuid()).WithName("Part 2").Build())
            .Build();

        var dtos = RepairTaskMapper.ToDtos(repairTask.Parts);

        Assert.Equal(2, dtos.Count);
    }
}
