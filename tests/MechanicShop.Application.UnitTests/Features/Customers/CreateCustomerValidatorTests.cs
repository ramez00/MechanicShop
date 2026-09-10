using MechanicShop.Application.Features.Customers.Commands;
using MechanicShop.Application.Features.Customers.Create.Commands;

namespace MechanicShop.Application.UnitTests.Features.Customers;

public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _validator = new();

    private static CreateCustomerCommand Valid() =>
        new(
            Name: "John Doe",
            PhoneNumber: "+12025550123",
            Email: "john@example.com",
            cars: [new CreateCarCommand("Toyota", "Corolla", 2020, "ABC-1234")]);

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_IsInvalid(string name)
    {
        var result = _validator.Validate(Valid() with { Name = name });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomerCommand.Name));
    }

    [Fact]
    public void Validate_WithTooLongName_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Name = new string('x', 101) });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomerCommand.Name));
    }

    [Theory]
    [InlineData("not-a-phone")]
    [InlineData("")]
    public void Validate_WithInvalidPhone_IsInvalid(string phone)
    {
        var result = _validator.Validate(Valid() with { PhoneNumber = phone });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomerCommand.PhoneNumber));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void Validate_WithInvalidEmail_IsInvalid(string email)
    {
        var result = _validator.Validate(Valid() with { Email = email });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomerCommand.Email));
    }

    [Fact]
    public void Validate_WithNoCars_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { cars = [] });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomerCommand.cars));
    }
}
