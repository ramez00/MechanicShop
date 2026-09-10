using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Tests.Common.Fakes;

/// <summary>
/// Builds a real in-process <see cref="HybridCache"/> for handler tests.
/// <para>
/// Handlers both read (<c>GetOrCreateAsync</c>) and invalidate (<c>RemoveByTagAsync</c>) the
/// cache; a real instance exercises that behaviour faithfully without any mock setup.
/// </para>
/// </summary>
public static class TestCache
{
    public static HybridCache Create()
    {
        var provider = new ServiceCollection()
            .AddHybridCache()
            .Services
            .BuildServiceProvider();

        return provider.GetRequiredService<HybridCache>();
    }
}
