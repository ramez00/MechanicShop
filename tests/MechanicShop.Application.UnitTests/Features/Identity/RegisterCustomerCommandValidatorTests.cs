using FluentValidation.TestHelper;
using MechanicShop.Application.Features.Identity.Commands.RegisterCustomer;

namespace MechanicShop.Application.UnitTests.Features.Identity;

public class RegisterCustomerCommandValidatorTests
{
    private readonly RegisterCustomerCommandValidator _validator = new();

    private static RegisterCustomerCommand Valid() =>
        new("ramez01", "Passw0rd1", "Passw0rd1", "ramez01@test.local", "Ramez Test", "+201000000000");

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]                                // too short
    [InlineData("abcdefghijabcdefghijabcdefghij1")]   // 31 chars
    [InlineData("a@b")]                               // '@' reserved for staff usernames (emails)
    [InlineData("a b")]
    [InlineData("rämez")]
    public void Invalid_username_fails(string userName)
    {
        _validator.TestValidate(Valid() with { UserName = userName })
            .ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Theory]
    [InlineData("ramez01")]
    [InlineData("r.a_m-e")]
    [InlineData("abc")]
    [InlineData("abcdefghijabcdefghijabcdefghij")]    // 30 chars
    public void Valid_username_passes(string userName)
    {
        _validator.TestValidate(Valid() with { UserName = userName })
            .ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Theory]
    [InlineData("abcdefgh")]    // no upper, no digit
    [InlineData("ABCDEFG1")]    // no lower
    [InlineData("Abcdefgh")]    // no digit
    [InlineData("Abc1")]        // too short
    [InlineData("")]
    public void Weak_password_fails(string password)
    {
        _validator.TestValidate(Valid() with { Password = password, ConfirmPassword = password })
            .ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Mismatched_confirm_password_fails()
    {
        _validator.TestValidate(Valid() with { ConfirmPassword = "Different1" })
            .ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Invalid_email_fails(string email)
    {
        _validator.TestValidate(Valid() with { Email = email })
            .ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0123")]               // E.164 can't start with 0
    [InlineData("+12345678901234567")] // too long
    [InlineData("phone")]
    public void Invalid_phone_fails(string phone)
    {
        _validator.TestValidate(Valid() with { PhoneNumber = phone })
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Missing_full_name_fails()
    {
        _validator.TestValidate(Valid() with { FullName = "" })
            .ShouldHaveValidationErrorFor(x => x.FullName);
    }
}
