using MechanicShop.Application.Features.RepairTasks.Commands.Create;
using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.UnitTests.Features.RepairTasks;

public class CreateRepairTaskCommandValidatorTests
{
    private readonly CreateRepairTaskCommandValidator _validator = new();

    private static CreateRepairTaskCommand Valid() =>
        new("Oil Change", 120m, RepairDurationInMinutes.Min30, [new CreateRepairTaskPartCommand("Filter", 20m, 1)]);

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_IsInvalid(string name)
    {
        var result = _validator.Validate(Valid() with { Name = name });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskCommand.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_WithNonPositiveLaborCost_IsInvalid(decimal cost)
    {
        var result = _validator.Validate(Valid() with { LaborCost = cost });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskCommand.LaborCost));
    }

    [Fact]
    public void Validate_WithNullDuration_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { EstimatedDurationInMins = null });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskCommand.EstimatedDurationInMins));
    }

    [Fact]
    public void Validate_WithUndefinedDuration_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { EstimatedDurationInMins = (RepairDurationInMinutes)999 });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskCommand.EstimatedDurationInMins));
    }

    [Fact]
    public void Validate_WithNoParts_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Parts = [] });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskCommand.Parts));
    }
}
