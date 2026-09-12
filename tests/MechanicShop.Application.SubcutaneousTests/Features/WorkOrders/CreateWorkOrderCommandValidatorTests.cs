using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class CreateWorkOrderCommandValidatorTests
{
    private readonly CreateWorkOrderCommandValidator _validator = new();

    private static CreateWorkOrderCommand Valid() =>
        new(Spot.A, Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), [Guid.NewGuid()], Guid.NewGuid());

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyVehicleId_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { VehicleId = Guid.Empty });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateWorkOrderCommand.VehicleId));
    }

    [Fact]
    public void Validate_WithPastStartAt_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { StartAt = DateTimeOffset.UtcNow.AddDays(-1) });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateWorkOrderCommand.StartAt));
    }

    [Fact]
    public void Validate_WithNoRepairTasks_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { RepairTaskIds = [] });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateWorkOrderCommand.RepairTaskIds));
    }

    [Fact]
    public void Validate_WithEmptyLaborId_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { LaborId = Guid.Empty });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateWorkOrderCommand.LaborId));
    }

    [Fact]
    public void Validate_WithNullLaborId_IsValid()
    {
        // LaborId is optional; null is allowed by the validator.
        Assert.True(_validator.Validate(Valid() with { LaborId = null }).IsValid);
    }

    [Fact]
    public void Validate_WithInvalidSpot_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Spot = (Spot)99 });

        Assert.Contains(result.Errors, e => e.ErrorCode == "Spot_Invalid");
    }
}
