using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers.cars;

public class CarErrors
{
    public static Error MakeRequired => Error.Validation("Car make is required.");
    public static Error ModelRequired => Error.Validation("Car model is required.");
    public static Error YearRequired => Error.Validation("Car year is required.");
    public static Error LicensePlateRequired => Error.Validation("Car license plate is required.");
    public static Error CarNotFound(Guid carId) => Error.NotFound($"Car with ID '{carId}' was not found.");
       public static Error YearInvalid =>
        Error.Validation("Vehicle_Year_Invalid", "Year must be between 1886 and next year.");
}