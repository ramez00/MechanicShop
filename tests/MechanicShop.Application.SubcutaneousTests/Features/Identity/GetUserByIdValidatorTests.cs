using MechanicShop.Application.Features.Identity.Queries.GetUserInfo;

namespace MechanicShop.Application.UnitTests.Features.Identity;

public class GetUserByIdValidatorTests
{
    private readonly GetUserByIdValidator _validator = new();

    [Fact]
    public void Validate_WithUserId_IsValid()
    {
        Assert.True(_validator.Validate(new GetUserByIdQuery("user-1")).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyUserId_IsInvalid()
    {
        var result = _validator.Validate(new GetUserByIdQuery(""));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetUserByIdQuery.userId));
    }
}
