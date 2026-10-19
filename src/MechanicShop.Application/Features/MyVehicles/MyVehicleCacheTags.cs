namespace MechanicShop.Application.Features.MyVehicles;

internal static class MyVehicleCacheTags
{
    // Cars appear in cached customer and work-order results, so changes must evict both.
    public static readonly string[] Affected = ["customer", "work-order"];
}
