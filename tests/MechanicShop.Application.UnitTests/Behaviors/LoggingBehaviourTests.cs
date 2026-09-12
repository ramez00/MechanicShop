using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace MechanicShop.Application.UnitTests.Common.Behaviors;

public class LoggingBehaviourTests
{
    public sealed record SampleRequest(string Value) : IRequest<Result<string>>;

    private readonly IUser _user = Substitute.For<IUser>();
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();

    private LoggingBehaviour<SampleRequest> CreateSut() =>
        new(NullLogger<SampleRequest>.Instance, _user, _identity);

    [Fact]
    public async Task Process_WhenUserAuthenticated_ResolvesUserName()
    {
        _user.Id.Returns("user-1");
        _identity.GetUserNameAsync("user-1").Returns("Jane Smith");
        var sut = CreateSut();

        await sut.Process(new SampleRequest("payload"), CancellationToken.None);

        await _identity.Received(1).GetUserNameAsync("user-1");
    }

    [Fact]
    public async Task Process_WhenNoUser_DoesNotResolveUserName()
    {
        _user.Id.Returns((string?)null);
        var sut = CreateSut();

        await sut.Process(new SampleRequest("payload"), CancellationToken.None);

        await _identity.DidNotReceive().GetUserNameAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Process_WhenUserIdEmpty_DoesNotResolveUserName()
    {
        _user.Id.Returns(string.Empty);
        var sut = CreateSut();

        await sut.Process(new SampleRequest("payload"), CancellationToken.None);

        await _identity.DidNotReceive().GetUserNameAsync(Arg.Any<string>());
    }
}
