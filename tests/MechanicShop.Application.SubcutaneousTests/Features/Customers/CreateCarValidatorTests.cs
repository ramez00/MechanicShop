using MechanicShop.Application.Features.Customers.Cars;
using MechanicShop.Application.Features.Customers.Commands;

namespace MechanicShop.Application.UnitTests.Features.Customers;

public class CreateCarValidatorTests
{
    private readonly CreateCarValidator _validator = new();

    private static CreateCarCommand Valid() => new("Toyota", "Corolla", 2020, "ABC-1234");

    [Fact]
    public void Validate_WithValidCar_IsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyMake_IsInvalid(string make)
    {
        var result = _validator.Validate(Valid() with { Make = make });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCarCommand.Make));
    }

    [Fact]
    public void Validate_WithTooLongModel_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Model = new string('m', 51) });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCarCommand.Model));
    }

    [Fact]
    public void Validate_WithEmptyLicensePlate_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { LicensePlate = "" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCarCommand.LicensePlate));
    }
}
