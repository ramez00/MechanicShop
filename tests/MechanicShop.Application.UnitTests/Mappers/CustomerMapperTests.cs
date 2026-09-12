using MechanicShop.Application.customers.Mappers;

namespace MechanicShop.Application.UnitTests.Mappers;

public class CustomerMapperTests
{
    [Fact]
    public void ToDto_ShouldMapScalarFieldsAndCars()
    {
        var customer = new CustomerBuilder().Build();
        var car = customer.Cars.First();

        var dto = customer.ToDto();

        Assert.Equal(customer.Id, dto.Id);
        Assert.Equal(customer.Name, dto.Name);
        Assert.Equal(customer.PhoneNumber, dto.PhoneNumber);
        Assert.Equal(customer.Email, dto.Email);

        Assert.Single(dto.Cars);
        var carDto = dto.Cars[0];
        Assert.Equal(car.Id, carDto.Id);
        Assert.Equal(car.Make, carDto.Make);
        Assert.Equal(car.Model, carDto.Model);
        Assert.Equal(car.Year, carDto.Year);
        Assert.Equal(car.LicensePlate, carDto.LicensePlate);
    }

    [Fact]
    public void ToDto_ShouldMapMultipleCars()
    {
        var customer = new CustomerBuilder()
            .WithCars(
                new CarBuilder().WithId(Guid.NewGuid()).WithLicensePlate("AAA-111").Build(),
                new CarBuilder().WithId(Guid.NewGuid()).WithLicensePlate("BBB-222").Build())
            .Build();

        var dto = customer.ToDto();

        Assert.Equal(2, dto.Cars.Count);
    }

    [Fact]
    public void ToDtoList_ShouldMapEachCustomer()
    {
        var customers = new[]
        {
            new CustomerBuilder().WithId(Guid.NewGuid()).WithEmail("a@example.com")
                .WithCars(new CarBuilder().WithId(Guid.NewGuid()).Build()).Build(),
            new CustomerBuilder().WithId(Guid.NewGuid()).WithEmail("b@example.com")
                .WithCars(new CarBuilder().WithId(Guid.NewGuid()).Build()).Build(),
        };

        var dtos = customers.ToDtoList();

        Assert.Equal(2, dtos.Count);
        Assert.Equal(customers[0].Id, dtos[0].Id);
        Assert.Equal(customers[1].Id, dtos[1].Id);
    }
}
