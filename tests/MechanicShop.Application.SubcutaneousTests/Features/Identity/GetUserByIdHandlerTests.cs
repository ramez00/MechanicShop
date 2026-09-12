using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries.GetUserInfo;
using Microsoft.Extensions.Logging.Abstractions;

namespace MechanicShop.Application.UnitTests.Features.Identity;

public class GetUserByIdHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();

    private GetUserByIdHandler CreateSut() => new(NullLogger<GetUserByIdHandler>.Instance, _identity);

    [Fact]
    public async Task Handle_WhenUserFound_ReturnsDto()
    {
        var user = new AppUserDto("user-1", "user@example.com", [], []);
        _identity.GetUserByIdAsync("user-1").Returns((Result<AppUserDto>)user);
        var sut = CreateSut();

        var result = await sut.Handle(new GetUserByIdQuery("user-1"), CancellationToken.None);

        var dto = result.ShouldBeSuccess();
        Assert.Equal("user-1", dto.UserId);
    }

    [Fact]
    public async Task Handle_WhenUserMissing_ReturnsError()
    {
        _identity.GetUserByIdAsync(Arg.Any<string>()).Returns((Result<AppUserDto>)ApplicationErrors.UserNotFound);
        var sut = CreateSut();

        var result = await sut.Handle(new GetUserByIdQuery("missing"), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.UserNotFound);
    }
}
