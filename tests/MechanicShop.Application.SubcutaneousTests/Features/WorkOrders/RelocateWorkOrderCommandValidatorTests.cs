using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class RelocateWorkOrderCommandValidatorTests
{
    private readonly RelocateWorkOrderCommandValidator _validator = new();

    private static RelocateWorkOrderCommand Valid() =>
        new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), Spot.A);

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyWorkOrderId_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { WorkOrderId = Guid.Empty });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RelocateWorkOrderCommand.WorkOrderId));
    }

    [Fact]
    public void Validate_WithPastStartAt_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { NewStartAt = DateTimeOffset.UtcNow.AddDays(-1) });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RelocateWorkOrderCommand.NewStartAt));
    }

    [Fact]
    public void Validate_WithInvalidSpot_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { NewSpot = (Spot)99 });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RelocateWorkOrderCommand.NewSpot));
    }
}
