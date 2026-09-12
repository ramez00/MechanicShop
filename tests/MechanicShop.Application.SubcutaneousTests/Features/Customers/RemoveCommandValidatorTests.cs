using MechanicShop.Application.Features.Customers.Commands.Remove;

namespace MechanicShop.Application.UnitTests.Features.Customers;

public class RemoveCommandValidatorTests
{
    private readonly RemoveCommandValidator _validator = new();

    [Fact]
    public void Validate_WithCustomerId_IsValid()
    {
        var result = _validator.Validate(new RemoveCustomerCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyCustomerId_IsInvalid()
    {
        var result = _validator.Validate(new RemoveCustomerCommand(Guid.Empty));

        Assert.False(result.IsValid);
    }
}
