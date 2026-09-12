namespace MechanicShop.Application.UnitTests.Mappers;

// CarMapper lives in the global namespace, so its extension methods need no using directive.
public class CarMapperTests
{
    [Fact]
    public void ToDto_ShouldMapAllFields()
    {
        var car = new CarBuilder()
            .WithMake("Honda")
            .WithModel("Civic")
            .WithYear(2021)
            .WithLicensePlate("CAR-9876")
            .Build();

        var dto = car.ToDto();

        Assert.Equal(car.Id, dto.Id);
        Assert.Equal("Honda", dto.Make);
        Assert.Equal("Civic", dto.Model);
        Assert.Equal(2021, dto.Year);
        Assert.Equal("CAR-9876", dto.LicensePlate);
    }

    [Fact]
    public void ToDtoList_ShouldMapEachCar()
    {
        var cars = new[]
        {
            new CarBuilder().WithId(Guid.NewGuid()).WithLicensePlate("AAA-111").Build(),
            new CarBuilder().WithId(Guid.NewGuid()).WithLicensePlate("BBB-222").Build(),
        };

        var dtos = cars.ToDtoList();

        Assert.Equal(2, dtos.Count);
        Assert.Equal(cars[0].Id, dtos[0].Id);
        Assert.Equal(cars[1].Id, dtos[1].Id);
    }
}
