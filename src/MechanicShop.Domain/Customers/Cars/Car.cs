using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers.cars;

public class Car : AuditableEntity
{
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
        if (string.IsNullOrWhiteSpace(make))
            return CarErrors.MakeRequired;

        if (string.IsNullOrWhiteSpace(model))
            return CarErrors.ModelRequired;

        if (year < 1886 || year > DateTime.UtcNow.Year)
            return CarErrors.YearInvalid;

        if (string.IsNullOrWhiteSpace(licensePlate))
            return CarErrors.LicensePlateRequired;

        return new Car(id, make, model, year, licensePlate);
    }

    public Result<Updated> Update(string make, string model, int year, string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(make))
            return CarErrors.MakeRequired;

        if (string.IsNullOrWhiteSpace(model))
            return CarErrors.ModelRequired;

        if (year < 1886 || year > DateTime.UtcNow.Year)
            return CarErrors.YearInvalid;

        if (string.IsNullOrWhiteSpace(licensePlate))
            return CarErrors.LicensePlateRequired;

        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;

        return Result.updated;
    }
}