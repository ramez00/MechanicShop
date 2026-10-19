using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers.cars;

public class Car : AuditableEntity
{
    public const int EarliestModelYear = 1886;

    public Guid CustomerId { get; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public string LicensePlate { get; private set; }
    public Customer? Customer { get; set; }
    public string CarInfo => $"{Year} {Make} {Model} ({LicensePlate})";

    private Car() { } // For EF Core

    private Car(Guid id, string make, string model, int year, string licensePlate) : base(id)
    {
        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;
    }

    public static Result<Car> Create(Guid id, string make, string model, int year, string licensePlate)
    {
        var validation = Validate(make, model, year, licensePlate);

        if (validation.IsError)
            return validation.Errors!;

        return new Car(id, make, model, year, NormalizeLicensePlate(licensePlate));
    }

    public Result<Updated> Update(string make, string model, int year, string licensePlate)
    {
        var validation = Validate(make, model, year, licensePlate);

        if (validation.IsError)
            return validation.Errors!;

        Make = make;
        Model = model;
        Year = year;
        LicensePlate = NormalizeLicensePlate(licensePlate);

        return Result.updated;
    }

    // Plates are stored uppercase without whitespace, so " ab 12 cd " and "AB12CD" are the same plate.
    public static string NormalizeLicensePlate(string licensePlate)
        => string.Concat(licensePlate.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();

    private static Result<Success> Validate(string make, string model, int year, string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(make))
            return CarErrors.MakeRequired;

        if (string.IsNullOrWhiteSpace(model))
            return CarErrors.ModelRequired;

        // Next year's models go on sale this year.
        if (year < EarliestModelYear || year > DateTime.UtcNow.Year + 1)
            return CarErrors.YearInvalid;

        if (string.IsNullOrWhiteSpace(licensePlate))
            return CarErrors.LicensePlateRequired;

        return Result.success;
    }
}
