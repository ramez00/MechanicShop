using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrder;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class UpdateWorkOrderStateCommandValidatorTests
{
    private readonly UpdateWorkOrderStateCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        var result = _validator.Validate(new UpdateWorkOrderStateCommand(Guid.NewGuid(), WorkOrderState.InProgress));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyWorkOrderId_IsInvalid()
    {
        var result = _validator.Validate(new UpdateWorkOrderStateCommand(Guid.Empty, WorkOrderState.InProgress));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateWorkOrderStateCommand.WorkOrderId));
    }

    [Fact]
    public void Validate_WithUndefinedState_IsInvalid()
    {
        var result = _validator.Validate(new UpdateWorkOrderStateCommand(Guid.NewGuid(), (WorkOrderState)99));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateWorkOrderStateCommand.WorkOrderState));
    }
}
