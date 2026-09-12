using MechanicShop.Application.Features.RepairTasks.Commands.Update;
using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.UnitTests.Features.RepairTasks;

public class UpdateRepairTaskCommandValidatorTests
{
    private readonly UpdateRepairTaskCommandValidator _validator = new();

    private static UpdateRepairTaskCommand Valid() =>
        new(Guid.NewGuid(), "Oil Change", 120m, RepairDurationInMinutes.Min30,
            [new UpdateRepairTaskPartCommand(null, "Filter", 20m, 1)]);

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { RepairTaskId = Guid.Empty });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskCommand.RepairTaskId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10_001)]
    public void Validate_WithLaborCostOutOfRange_IsInvalid(decimal cost)
    {
        var result = _validator.Validate(Valid() with { LaborCost = cost });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskCommand.LaborCost));
    }

    [Fact]
    public void Validate_WithNoParts_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Parts = [] });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskCommand.Parts));
    }
}
