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

    public static Error PlateTaken =>
        Error.Conflict("Car_PlateTaken", "A vehicle with this license plate is already registered.");

    public static Error HasActiveBookings =>
        Error.Conflict("Car_HasActiveBookings", "This vehicle has upcoming or in-progress bookings and can't be removed.");

    public static Error HasServiceHistory =>
        Error.Conflict("Car_HasServiceHistory", "This vehicle has service history at the shop and can't be removed.");
}
