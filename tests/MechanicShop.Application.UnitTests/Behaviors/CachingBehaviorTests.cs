using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Tests.Common.Fakes;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging.Abstractions;

namespace MechanicShop.Application.UnitTests.Common.Behaviors;

public class CachingBehaviorTests
{
    private readonly HybridCache _cache = TestCache.Create();

    // A cached query keyed by the supplied CacheKey.
    public sealed record CachedSampleQuery(string CacheKey) : ICachedQuery<Result<string>>
    {
        public string[] Tags => ["sample"];
        public TimeSpan Expiration => TimeSpan.FromMinutes(5);
    }

    // A request that does NOT implement ICachedQuery — should bypass caching entirely.
    public sealed record PlainSampleQuery : IRequest<Result<string>>;

    private static RequestHandlerDelegate<Result<string>> Next(Func<Result<string>> factory, Action onCalled)
        => _ => { onCalled(); return Task.FromResult(factory()); };

    [Fact]
    public async Task Handle_WhenRequestNotCacheable_CallsNextAndDoesNotCache()
    {
        var behavior = new CachingBehavior<PlainSampleQuery, Result<string>>(
            _cache, NullLogger<CachingBehavior<PlainSampleQuery, Result<string>>>.Instance);
        var calls = 0;

        var result = await behavior.Handle(new PlainSampleQuery(), Next(() => "value", () => calls++), CancellationToken.None);

        Assert.Equal("value", result.ShouldBeSuccess());
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Handle_OnCacheMissThenHit_CachesSuccessfulResultAndSkipsNext()
    {
        var behavior = new CachingBehavior<CachedSampleQuery, Result<string>>(
            _cache, NullLogger<CachingBehavior<CachedSampleQuery, Result<string>>>.Instance);
        var query = new CachedSampleQuery("cache-key-1");
        var calls = 0;

        // First call: cache miss -> next runs and the successful result is cached.
        var first = await behavior.Handle(query, Next(() => "cached-value", () => calls++), CancellationToken.None);
        // Second call: cache hit -> next must NOT run; the cached value is returned.
        var second = await behavior.Handle(query, Next(() => "fresh-value", () => calls++), CancellationToken.None);

        Assert.Equal("cached-value", first.ShouldBeSuccess());
        Assert.Equal("cached-value", second.ShouldBeSuccess());
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Handle_WhenResultIsError_DoesNotCache()
    {
        var behavior = new CachingBehavior<CachedSampleQuery, Result<string>>(
            _cache, NullLogger<CachingBehavior<CachedSampleQuery, Result<string>>>.Instance);
        var query = new CachedSampleQuery("cache-key-2");
        var calls = 0;

        // First call returns an error -> nothing is cached.
        var first = await behavior.Handle(query, Next(() => Error.NotFound("x", "missing"), () => calls++), CancellationToken.None);
        // Second call therefore hits next again (this time succeeding).
        var second = await behavior.Handle(query, Next(() => "recovered", () => calls++), CancellationToken.None);

        first.ShouldBeError();
        Assert.Equal("recovered", second.ShouldBeSuccess());
        Assert.Equal(2, calls);
    }
}
