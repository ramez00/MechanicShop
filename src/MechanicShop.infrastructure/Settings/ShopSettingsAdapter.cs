using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace MechanicShop.infrastructure.Settings;

public class ShopSettingsAdapter(IOptions<AppSettings> options) : IShopSettings
{
    private readonly AppSettings _s = options.Value;

    public TimeOnly OpeningTime => _s.OpeningTime;
    public TimeOnly ClosingTime => _s.ClosingTime;
    public int MaxSpots => _s.MaxSpots;
    public int BookingLeadTimeMinutes => _s.BookingLeadTimeMinutes;
    public int BookingHorizonDays => _s.BookingHorizonDays;
}
