using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace MechanicShop.Application.UnitTests.Common.Behaviors;

public class PerformanceBehaviourTests
{
    public sealed record SampleRequest : IRequest<Result<string>>;

    private readonly IUser _user = Substitute.For<IUser>();
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();

    private PerformanceBehaviour<SampleRequest, Result<string>> CreateSut() =>
        new(NullLogger<SampleRequest>.Instance, _user, _identity);

    [Fact]
    public async Task Handle_WhenFast_ReturnsResponseWithoutResolvingUserName()
    {
        _user.Id.Returns("user-1");
        var sut = CreateSut();

        var result = await sut.Handle(new SampleRequest(), _ => Task.FromResult((Result<string>)"ok"), CancellationToken.None);

        Assert.Equal("ok", result.ShouldBeSuccess());
        // The request completes well under the 500ms threshold, so no user lookup happens.
        await _identity.DidNotReceive().GetUserNameAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenLongRunning_ResolvesUserNameAndStillReturnsResponse()
    {
        _user.Id.Returns("user-1");
        _identity.GetUserNameAsync("user-1").Returns("Jane Smith");
        var sut = CreateSut();

        var result = await sut.Handle(
            new SampleRequest(),
            async _ => { await Task.Delay(700); return (Result<string>)"ok"; },
            CancellationToken.None);

        Assert.Equal("ok", result.ShouldBeSuccess());
        // Elapsed > 500ms and a user id is present, so the user name is resolved for the warning log.
        await _identity.Received(1).GetUserNameAsync("user-1");
    }

    [Fact]
    public async Task Handle_WhenLongRunningButNoUser_DoesNotResolveUserName()
    {
        _user.Id.Returns((string?)null);
        var sut = CreateSut();

        var result = await sut.Handle(
            new SampleRequest(),
            async _ => { await Task.Delay(700); return (Result<string>)"ok"; },
            CancellationToken.None);

        Assert.Equal("ok", result.ShouldBeSuccess());
        await _identity.DidNotReceive().GetUserNameAsync(Arg.Any<string>());
    }
}
