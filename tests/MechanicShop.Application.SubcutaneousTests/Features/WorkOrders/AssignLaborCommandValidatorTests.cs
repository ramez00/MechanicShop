using MechanicShop.Application.Features.WorkOrders.AssignLabor;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class AssignLaborCommandValidatorTests
{
    private readonly AssignLaborCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        Assert.True(_validator.Validate(new AssignLaborCommand(Guid.NewGuid(), Guid.NewGuid())).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyWorkOrderId_IsInvalid()
    {
        var result = _validator.Validate(new AssignLaborCommand(Guid.Empty, Guid.NewGuid()));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AssignLaborCommand.WorkOrderId));
    }

    [Fact]
    public void Validate_WithEmptyLaborId_IsInvalid()
    {
        var result = _validator.Validate(new AssignLaborCommand(Guid.NewGuid(), Guid.Empty));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AssignLaborCommand.LaborId));
    }
}
