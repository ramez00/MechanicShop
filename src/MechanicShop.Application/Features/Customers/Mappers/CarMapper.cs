using MechanicShop.Application.Features.Customers.Dtos;

public static class CarMapper
{
    public static CarDto ToDto(this Car car)
    {
        return new CarDto(
            car.Id,
            car.Make,
            car.Model,
            car.Year,
            car.LicensePlate
        );
    }

    public static List<CarDto> ToDtoList(this IEnumerable<Car> cars)
    {
        return cars.Select(car => car.ToDto()).ToList();
    }
}