namespace MechanicShop.Application.Features.Customers.Cars;

public static class LicensePlateQueries
{
    // Compares in normalized form, matching how Car stores plates and the unique index on them.
    public static Task<bool> IsLicensePlateTakenAsync(this IQueryable<Car> cars, string licensePlate, CancellationToken ct)
    {
        var plate = Car.NormalizeLicensePlate(licensePlate);

        return cars.AnyAsync(c => c.LicensePlate == plate, ct);
    }

    public static bool HasDuplicateLicensePlates(this IEnumerable<string> licensePlates)
    {
        var plates = licensePlates.Select(Car.NormalizeLicensePlate).ToList();

        return plates.Distinct().Count() != plates.Count;
    }
}
