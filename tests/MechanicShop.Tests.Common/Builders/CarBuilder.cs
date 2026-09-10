using MechanicShop.Domain.Customers.cars;
using MechanicShop.Tests.Common.Constants;

namespace MechanicShop.Tests.Common.Builders;

public sealed class CarBuilder
{
    private Guid _id = TestConstants.Cars.Id;
    private string _make = TestConstants.Cars.Make;
    private string _model = TestConstants.Cars.Model;
    private int _year = TestConstants.Cars.Year;
    private string _licensePlate = TestConstants.Cars.LicensePlate;

    public CarBuilder WithId(Guid id) { _id = id; return this; }
    public CarBuilder WithMake(string make) { _make = make; return this; }
    public CarBuilder WithModel(string model) { _model = model; return this; }
    public CarBuilder WithYear(int year) { _year = year; return this; }
    public CarBuilder WithLicensePlate(string plate) { _licensePlate = plate; return this; }

    public Car Build()
    {
        var result = Car.Create(_id, _make, _model, _year, _licensePlate);
        if (result.IsError)
            throw new InvalidOperationException($"CarBuilder produced an invalid Car: {result.TopError.Code}");
        return result.Value;
    }
}
