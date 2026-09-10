using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Domain.UnitTests.RepairTasks;

public class RepairTaskTests
{
    private static List<Part> OnePart() => [new PartBuilder().Build()];

    // ---------- Create ----------

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccessAndTrimsName()
    {
        var result = RepairTask.Create(
            TestConstants.RepairTasks.Id, "  Oil Change  ", 120m, RepairDurationInMinutes.Min30, OnePart());

        var task = result.ShouldBeSuccess();
        Assert.Equal("Oil Change", task.Name);
        Assert.Equal(120m, task.LaborCost);
        Assert.Equal(RepairDurationInMinutes.Min30, task.EstimatedDurationInMins);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_ReturnsNameRequired(string name)
    {
        var result = RepairTask.Create(TestConstants.RepairTasks.Id, name, 120m, RepairDurationInMinutes.Min30, OnePart());

        result.ShouldBeError(RepairTaskErrors.NameRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithNonPositiveLaborCost_ReturnsLaborCostInvalid(decimal laborCost)
    {
        var result = RepairTask.Create(TestConstants.RepairTasks.Id, "Oil Change", laborCost, RepairDurationInMinutes.Min30, OnePart());

        result.ShouldBeError(RepairTaskErrors.LaborCostInvalid);
    }

    [Fact]
    public void Create_WithUndefinedDuration_ReturnsDurationInvalid()
    {
        var result = RepairTask.Create(TestConstants.RepairTasks.Id, "Oil Change", 120m, (RepairDurationInMinutes)7, OnePart());

        result.ShouldBeError(RepairTaskErrors.DurationInvalid);
    }

    // ---------- Update ----------

    [Fact]
    public void Update_WithValidInputs_UpdatesFields()
    {
        var task = new RepairTaskBuilder().Build();

        var result = task.Update("Brake Service", 250m, RepairDurationInMinutes.Min60);

        result.ShouldBeSuccess();
        Assert.Equal("Brake Service", task.Name);
        Assert.Equal(250m, task.LaborCost);
        Assert.Equal(RepairDurationInMinutes.Min60, task.EstimatedDurationInMins);
    }

    [Fact]
    public void Update_WithBlankName_ReturnsNameRequired()
    {
        var task = new RepairTaskBuilder().Build();

        var result = task.Update("  ", 250m, RepairDurationInMinutes.Min60);

        result.ShouldBeError(RepairTaskErrors.NameRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10001)]
    public void Update_WithLaborCostOutOfRange_ReturnsLaborCostInvalid(decimal laborCost)
    {
        var task = new RepairTaskBuilder().Build();

        var result = task.Update("Brake Service", laborCost, RepairDurationInMinutes.Min60);

        result.ShouldBeError(RepairTaskErrors.LaborCostInvalid);
    }

    [Fact]
    public void Update_WithUndefinedDuration_ReturnsDurationInvalid()
    {
        var task = new RepairTaskBuilder().Build();

        var result = task.Update("Brake Service", 250m, (RepairDurationInMinutes)7);

        result.ShouldBeError(RepairTaskErrors.DurationInvalid);
    }

    // ---------- UpsertParts ----------

    [Fact]
    public void UpsertParts_AddsNewParts()
    {
        var task = new RepairTaskBuilder().WithNoParts().Build();
        var newPart = new PartBuilder().WithId(Guid.NewGuid()).Build();

        var result = task.UpsertParts([newPart]);

        result.ShouldBeSuccess();
        Assert.Single(task.Parts);
    }

    [Fact]
    public void UpsertParts_UpdatesExistingPartById()
    {
        var partId = Guid.NewGuid();
        var original = new PartBuilder().WithId(partId).WithName("Old").WithPrice(10m).WithQuantity(1).Build();
        var task = new RepairTaskBuilder().WithParts(original).Build();

        var incoming = new PartBuilder().WithId(partId).WithName("New").WithPrice(20m).WithQuantity(2).Build();
        var result = task.UpsertParts([incoming]);

        result.ShouldBeSuccess();
        var part = Assert.Single(task.Parts);
        Assert.Equal("New", part.Name);
        Assert.Equal(20m, part.Price);
        Assert.Equal(2, part.Quantity);
    }

    [Fact]
    public void UpsertParts_RemovesPartsNotInIncomingSet()
    {
        var keepId = Guid.NewGuid();
        var dropId = Guid.NewGuid();
        var keep = new PartBuilder().WithId(keepId).Build();
        var drop = new PartBuilder().WithId(dropId).Build();
        var task = new RepairTaskBuilder().WithParts(keep, drop).Build();

        var result = task.UpsertParts([new PartBuilder().WithId(keepId).Build()]);

        result.ShouldBeSuccess();
        var part = Assert.Single(task.Parts);
        Assert.Equal(keepId, part.Id);
    }

    // ---------- totalCost ----------

    [Fact]
    public void TotalCost_IsSumOfPartQtyTimesPricePlusLabor()
    {
        var part1 = new PartBuilder().WithId(Guid.NewGuid()).WithPrice(50m).WithQuantity(2).Build(); // 100
        var part2 = new PartBuilder().WithId(Guid.NewGuid()).WithPrice(30m).WithQuantity(1).Build(); // 30
        var task = new RepairTaskBuilder().WithLaborCost(120m).WithParts(part1, part2).Build();

        Assert.Equal(250m, task.totalCost);
    }
}
