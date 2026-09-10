using MechanicShop.infrastructure.Data;
using MechanicShop.Tests.Common.Data;
using MechanicShop.Tests.Common.Fakes;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging.Abstractions;

namespace MechanicShop.Tests.Common.Application;

/// <summary>
/// Base class for Application handler tests. Owns a SQLite-backed <see cref="AppDbContext"/>,
/// a real <see cref="HybridCache"/>, and a deterministic <see cref="TimeProvider"/>.
/// Derive from it and dispose is handled by xUnit through <see cref="IDisposable"/>.
/// </summary>
public abstract class HandlerTestBase : IDisposable
{
    private readonly TestDbContext _testDb = new();

    protected AppDbContext Context => _testDb.Context;

    protected HybridCache Cache { get; } = TestCache.Create();

    protected FixedTimeProvider Clock { get; } = new();

    /// <summary>The substitute MediatR publisher used by <see cref="AppDbContext"/> for domain events.</summary>
    protected MediatR.IPublisher Publisher => _testDb.Publisher;

    /// <summary>A shared <see cref="NullLogger{T}"/> — logging is not under test.</summary>
    protected static NullLogger<T> Logger<T>() => NullLogger<T>.Instance;

    /// <summary>Adds entities and persists them, then clears the change tracker so reads hit the store.</summary>
    protected async Task SeedAsync(params object[] entities)
    {
        Context.AddRange(entities);
        await Context.SaveChangesAsync(CancellationToken.None);
        Context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        _testDb.Dispose();
        GC.SuppressFinalize(this);
    }
}
