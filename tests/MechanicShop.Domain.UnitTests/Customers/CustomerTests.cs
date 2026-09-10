using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.cars;

namespace MechanicShop.Domain.UnitTests.Customers;

public class CustomerTests
{
    private static List<Car> OneCar() => [new CarBuilder().Build()];

    // ---------- Create ----------

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = Customer.Create(TestConstants.Customers.Id, "John Doe", "john@example.com", "+12025550123", OneCar());

        var customer = result.ShouldBeSuccess();
        Assert.Equal("John Doe", customer.Name);
        Assert.Equal("john@example.com", customer.Email);
        Assert.Equal("+12025550123", customer.PhoneNumber);
        Assert.Single(customer.Cars);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithBlankName_ReturnsNameRequired(string name)
    {
        var result = Customer.Create(TestConstants.Customers.Id, name, "john@example.com", "+12025550123", OneCar());

        result.ShouldBeError(CustomerErrors.NameRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithBlankEmail_ReturnsEmailRequired(string email)
    {
        var result = Customer.Create(TestConstants.Customers.Id, "John Doe", email, "+12025550123", OneCar());

        result.ShouldBeError(CustomerErrors.EmailRequired);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("john@@example.com")]
    public void Create_WithMalformedEmail_ReturnsEmailInvalid(string email)
    {
        var result = Customer.Create(TestConstants.Customers.Id, "John Doe", email, "+12025550123", OneCar());

        result.ShouldBeError(CustomerErrors.EmailInvalid);
    }

    // ---------- Update ----------

    [Fact]
    public void Update_WithValidInputs_UpdatesFields()
    {
        var customer = new CustomerBuilder().Build();

        var result = customer.Update("Jane Roe", "jane@example.com", "+12025550999");

        result.ShouldBeSuccess();
        Assert.Equal("Jane Roe", customer.Name);
        Assert.Equal("jane@example.com", customer.Email);
        Assert.Equal("+12025550999", customer.PhoneNumber);
    }

    [Fact]
    public void Update_WithMalformedEmail_ReturnsEmailInvalid()
    {
        var customer = new CustomerBuilder().Build();

        var result = customer.Update("Jane Roe", "bad-email", "+12025550999");

        result.ShouldBeError(CustomerErrors.EmailInvalid);
    }

    // ---------- UpsertParts (cars) ----------

    [Fact]
    public void UpsertParts_AddsNewCar()
    {
        var customer = new CustomerBuilder().WithNoCars().Build();
        var newCar = new CarBuilder().WithId(Guid.NewGuid()).Build();

        var result = customer.UpsertParts([newCar]);

        result.ShouldBeSuccess();
        Assert.Single(customer.Cars);
    }

    [Fact]
    public void UpsertParts_UpdatesExistingCarById()
    {
        var carId = Guid.NewGuid();
        var original = new CarBuilder().WithId(carId).WithMake("Toyota").Build();
        var customer = new CustomerBuilder().WithCars(original).Build();

        var incoming = new CarBuilder().WithId(carId).WithMake("Honda").WithModel("Civic").Build();
        var result = customer.UpsertParts([incoming]);

        result.ShouldBeSuccess();
        var car = Assert.Single(customer.Cars);
        Assert.Equal("Honda", car.Make);
        Assert.Equal("Civic", car.Model);
    }

    [Fact]
    public void UpsertParts_RemovesCarsNotInIncomingSet()
    {
        var keepId = Guid.NewGuid();
        var dropId = Guid.NewGuid();
        var customer = new CustomerBuilder()
            .WithCars(new CarBuilder().WithId(keepId).Build(), new CarBuilder().WithId(dropId).Build())
            .Build();

        var result = customer.UpsertParts([new CarBuilder().WithId(keepId).Build()]);

        result.ShouldBeSuccess();
        var car = Assert.Single(customer.Cars);
        Assert.Equal(keepId, car.Id);
    }
}
