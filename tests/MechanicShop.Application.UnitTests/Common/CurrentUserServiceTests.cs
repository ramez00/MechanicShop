using System.Security.Claims;

using MechanicShop.Application.Common.Identity;
using MechanicShop.infrastructure.Identity;

using Microsoft.AspNetCore.Http;

namespace MechanicShop.Application.UnitTests.Common;

public class CurrentUserServiceTests
{
    private static CurrentUserService CreateSut(params Claim[] claims)
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        });

        return new CurrentUserService(accessor);
    }

    [Fact]
    public void CustomerId_ReturnsValueFromClaim()
    {
        var customerId = Guid.NewGuid();

        var sut = CreateSut(new Claim(AppClaimTypes.CustomerId, customerId.ToString()));

        Assert.Equal(customerId, sut.CustomerId);
    }

    [Fact]
    public void CustomerId_ReturnsNull_WhenClaimMissing()
    {
        var sut = CreateSut(new Claim(ClaimTypes.NameIdentifier, "user-1"));

        Assert.Null(sut.CustomerId);
        Assert.Equal("user-1", sut.Id);
    }

    [Fact]
    public void CustomerId_ReturnsNull_WhenClaimIsNotAGuid()
    {
        var sut = CreateSut(new Claim(AppClaimTypes.CustomerId, "not-a-guid"));

        Assert.Null(sut.CustomerId);
    }

    [Fact]
    public void CustomerId_ReturnsNull_WhenNoHttpContext()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        Assert.Null(new CurrentUserService(accessor).CustomerId);
    }
}
