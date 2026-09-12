using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;

namespace MechanicShop.Application.UnitTests.Features.Identity;

public class GenerateTokenQueryValidatorTests
{
    private readonly GenerateTokenQueryValidator _validator = new();

    private static GenerateTokenQuery Valid() => new("user@example.com", "password");

    [Fact]
    public void Validate_WithValidQuery_IsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_IsInvalid(string email)
    {
        var result = _validator.Validate(Valid() with { Email = email });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GenerateTokenQuery.Email));
    }

    [Fact]
    public void Validate_WithEmptyPassword_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Password = "" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GenerateTokenQuery.Password));
    }
}
