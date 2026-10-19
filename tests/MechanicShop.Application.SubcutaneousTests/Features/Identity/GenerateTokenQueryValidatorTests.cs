using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;

namespace MechanicShop.Application.UnitTests.Features.Identity;

public class GenerateTokenQueryValidatorTests
{
    private readonly GenerateTokenQueryValidator _validator = new();

    private static GenerateTokenQuery Valid() => new("ramez01", "password");

    [Theory]
    [InlineData("ramez01")]
    [InlineData("pm@localhost")] // seeded staff log in with their email as username
    public void Validate_WithValidQuery_IsValid(string userName)
    {
        Assert.True(_validator.Validate(Valid() with { UserName = userName }).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyUserName_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { UserName = "" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GenerateTokenQuery.UserName));
    }

    [Fact]
    public void Validate_WithEmptyPassword_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { Password = "" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GenerateTokenQuery.Password));
    }
}
