using MechanicShop.Domain.Customers.cars;

namespace MechanicShop.Domain.UnitTests.Customers;

public class CarTests
{
    // ---------- Create ----------

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = Car.Create(TestConstants.Cars.Id, "Toyota", "Corolla", 2020, "ABC-1234");

        var car = result.ShouldBeSuccess();
        Assert.Equal("Toyota", car.Make);
        Assert.Equal("Corolla", car.Model);
        Assert.Equal(2020, car.Year);
        Assert.Equal("ABC-1234", car.LicensePlate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithBlankMake_ReturnsMakeRequired(string make)
    {
        var result = Car.Create(TestConstants.Cars.Id, make, "Corolla", 2020, "ABC-1234");

        result.ShouldBeError(CarErrors.MakeRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithBlankModel_ReturnsModelRequired(string model)
    {
        var result = Car.Create(TestConstants.Cars.Id, "Toyota", model, 2020, "ABC-1234");

        result.ShouldBeError(CarErrors.ModelRequired);
    }

    [Theory]
    [InlineData(1885)]
    [InlineData(3000)]
    public void Create_WithYearOutOfRange_ReturnsYearInvalid(int year)
    {
        var result = Car.Create(TestConstants.Cars.Id, "Toyota", "Corolla", year, "ABC-1234");

        result.ShouldBeError(CarErrors.YearInvalid);
    }

    [Fact]
    public void Create_WithBlankLicensePlate_ReturnsLicensePlateRequired()
    {
        var result = Car.Create(TestConstants.Cars.Id, "Toyota", "Corolla", 2020, " ");

        result.ShouldBeError(CarErrors.LicensePlateRequired);
    }

    // ---------- Update ----------

    [Fact]
    public void Update_WithValidInputs_UpdatesFields()
    {
        var car = new CarBuilder().Build();

        var result = car.Update("Honda", "Civic", 2022, "XYZ-9999");

        result.ShouldBeSuccess();
        Assert.Equal("Honda", car.Make);
        Assert.Equal("Civic", car.Model);
        Assert.Equal(2022, car.Year);
        Assert.Equal("XYZ-9999", car.LicensePlate);
    }

    [Fact]
    public void Update_WithInvalidYear_ReturnsYearInvalid()
    {
        var car = new CarBuilder().Build();

        var result = car.Update("Honda", "Civic", 1800, "XYZ-9999");

        result.ShouldBeError(CarErrors.YearInvalid);
    }

    // ---------- CarInfo ----------

    [Fact]
    public void CarInfo_FormatsYearMakeModelPlate()
    {
        var car = new CarBuilder()
            .WithMake("Toyota").WithModel("Corolla").WithYear(2020).WithLicensePlate("ABC-1234")
            .Build();

        Assert.Equal("2020 Toyota Corolla (ABC-1234)", car.CarInfo);
    }
}
