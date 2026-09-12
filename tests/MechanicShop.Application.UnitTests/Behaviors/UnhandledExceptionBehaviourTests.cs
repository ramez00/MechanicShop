using MechanicShop.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace MechanicShop.Application.UnitTests.Common.Behaviors;

public class UnhandledExceptionBehaviourTests
{
    public sealed record SampleRequest : IRequest<Result<string>>;

    private UnhandledExceptionBehaviour<SampleRequest, Result<string>> CreateSut() =>
        new(NullLogger<SampleRequest>.Instance);

    [Fact]
    public async Task Handle_WhenNoException_ReturnsResponse()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new SampleRequest(), _ => Task.FromResult((Result<string>)"ok"), CancellationToken.None);

        Assert.Equal("ok", result.ShouldBeSuccess());
    }

    [Fact]
    public async Task Handle_WhenNextThrows_RethrowsSameException()
    {
        var sut = CreateSut();
        var boom = new InvalidOperationException("boom");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.Handle(new SampleRequest(), _ => throw boom, CancellationToken.None));

        Assert.Same(boom, ex);
    }
}
