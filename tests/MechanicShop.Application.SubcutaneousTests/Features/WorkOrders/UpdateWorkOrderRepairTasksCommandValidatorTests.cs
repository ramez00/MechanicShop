using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class UpdateWorkOrderRepairTasksCommandValidatorTests
{
    private readonly UpdateWorkOrderRepairTasksCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        var result = _validator.Validate(new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), [Guid.NewGuid()]));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyWorkOrderId_IsInvalid()
    {
        var result = _validator.Validate(new UpdateWorkOrderRepairTasksCommand(Guid.Empty, [Guid.NewGuid()]));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateWorkOrderRepairTasksCommand.WorkOrderId));
    }

    [Fact]
    public void Validate_WithNoRepairTasks_IsInvalid()
    {
        var result = _validator.Validate(new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), []));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateWorkOrderRepairTasksCommand.RepairTasksId));
    }
}
