using MechanicShop.Application.Features.RepairTasks.Commands.Update;

namespace MechanicShop.Application.UnitTests.Features.RepairTasks;

public class UpdateRepairTaskPartCommandValidatorTests
{
    private readonly UpdateRepairTaskPartCommandValidator _validator = new();

    private static UpdateRepairTaskPartCommand Valid() => new(null, "Filter", 20m, 1);

    [Fact]
    public void Validate_WithValidPart_IsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Name = "" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskPartCommand.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10_001)]
    public void Validate_WithCostOutOfRange_IsInvalid(decimal cost)
    {
        var result = _validator.Validate(Valid() with { Cost = cost });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskPartCommand.Cost));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Validate_WithQuantityOutOfRange_IsInvalid(int quantity)
    {
        var result = _validator.Validate(Valid() with { Quantity = quantity });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskPartCommand.Quantity));
    }
}
