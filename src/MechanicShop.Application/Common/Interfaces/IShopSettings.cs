namespace MechanicShop.Application.Common.Interfaces;

public interface IShopSettings
{
    TimeOnly OpeningTime { get; }
    TimeOnly ClosingTime { get; }
    int MaxSpots { get; }
    int BookingLeadTimeMinutes { get; }
    int BookingHorizonDays { get; }
}
