using MechanicShop.Tests.Common.Constants;

namespace MechanicShop.Tests.Common.Fakes;

/// <summary>
/// A deterministic <see cref="TimeProvider"/> that always reports a fixed instant.
/// Domain (<c>Invoice</c>) and handlers (<c>SettleInvoice</c>) take a <see cref="TimeProvider"/>,
/// so tests inject this to make time assertions stable.
/// </summary>
public sealed class FixedTimeProvider : TimeProvider
{
    private DateTimeOffset _now;

    public FixedTimeProvider(DateTimeOffset now) => _now = now;

    public FixedTimeProvider() : this(TestConstants.UtcNow) { }

    public override DateTimeOffset GetUtcNow() => _now;

    /// <summary>Advance (or rewind) the clock for multi-step scenarios.</summary>
    public void Advance(TimeSpan by) => _now = _now.Add(by);

    public void Set(DateTimeOffset now) => _now = now;
}
