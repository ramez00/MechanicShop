using MechanicShop.Application.Features.Customers.Commands.Update;

namespace MechanicShop.Application.UnitTests.Features.Customers;

public class UpdateCarCommandValidatorTests
{
    private readonly UpdateCarCommandValidator _validator = new();

    private static UpdateCarCommand Valid() => new(null, "Toyota", "Corolla", 2020, "ABC-1234");

    [Fact]
    public void Validate_WithValidCar_IsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyMake_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Make = "" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCarCommand.Make));
    }

    [Fact]
    public void Validate_WithEmptyModel_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Model = "" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCarCommand.Model));
    }

    [Fact]
    public void Validate_WithTooLongLicensePlate_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { LicensePlate = new string('X', 11) });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCarCommand.LicensePlate));
    }
}
