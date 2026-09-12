using MechanicShop.Application.Features.Customers.Commands.Update;

namespace MechanicShop.Application.UnitTests.Features.Customers;

public class UpdateCommandValidatorTests
{
    private readonly UpdateCommandValidator _validator = new();

    private static UpdateCustomerCommand Valid() =>
        new(
            CustomerId: Guid.NewGuid(),
            Name: "John Doe",
            PhoneNumber: "+12025550123",
            Email: "john@example.com",
            Cars: [new UpdateCarCommand(null, "Toyota", "Corolla", 2020, "ABC-1234")]);

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyCustomerId_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { CustomerId = Guid.Empty });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCustomerCommand.CustomerId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_IsInvalid(string name)
    {
        var result = _validator.Validate(Valid() with { Name = name });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCustomerCommand.Name));
    }

    [Theory]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_IsInvalid(string email)
    {
        var result = _validator.Validate(Valid() with { Email = email });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCustomerCommand.Email));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    public void Validate_WithInvalidPhone_IsInvalid(string phone)
    {
        var result = _validator.Validate(Valid() with { PhoneNumber = phone });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCustomerCommand.PhoneNumber));
    }

    [Fact]
    public void Validate_WithNoCars_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Cars = [] });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCustomerCommand.Cars));
    }
}
