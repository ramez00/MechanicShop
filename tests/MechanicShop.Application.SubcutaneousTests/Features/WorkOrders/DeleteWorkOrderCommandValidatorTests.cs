using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class DeleteWorkOrderCommandValidatorTests
{
    private readonly DeleteWorkOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithId_IsValid()
    {
        Assert.True(_validator.Validate(new DeleteWorkOrderCommand(Guid.NewGuid())).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_IsInvalid()
    {
        var result = _validator.Validate(new DeleteWorkOrderCommand(Guid.Empty));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(DeleteWorkOrderCommand.WorkOrderId));
    }
}
