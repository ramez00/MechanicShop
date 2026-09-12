using MechanicShop.Application.Features.RepairTasks.Commands.Remove;

namespace MechanicShop.Application.UnitTests.Features.RepairTasks;

public class RemoveRepairTaskCommandValidatorTests
{
    private readonly RemoveRepairTaskCommandValidator _validator = new();

    [Fact]
    public void Validate_WithId_IsValid()
    {
        Assert.True(_validator.Validate(new RemoveRepairTaskCommand(Guid.NewGuid())).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_IsInvalid()
    {
        var result = _validator.Validate(new RemoveRepairTaskCommand(Guid.Empty));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RemoveRepairTaskCommand.RepairTaskId));
    }
}
