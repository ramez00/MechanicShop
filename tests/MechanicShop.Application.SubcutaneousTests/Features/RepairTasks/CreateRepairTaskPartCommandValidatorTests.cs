using MechanicShop.Application.Features.RepairTasks.Commands.Create;

namespace MechanicShop.Application.UnitTests.Features.RepairTasks;

public class CreateRepairTaskPartCommandValidatorTests
{
    private readonly CreateRepairTaskPartCommandValidator _validator = new();

    private static CreateRepairTaskPartCommand Valid() => new("Oil Filter", 25m, 1);

    [Fact]
    public void Validate_WithValidPart_IsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_IsInvalid(string name)
    {
        var result = _validator.Validate(Valid() with { Name = name });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskPartCommand.Name));
    }

    [Fact]
    public void Validate_WithNonPositiveCost_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Cost = 0m });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskPartCommand.Cost));
    }

    [Fact]
    public void Validate_WithNonPositiveQuantity_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Quantity = 0 });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskPartCommand.Quantity));
    }
}
