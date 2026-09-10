using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Domain.UnitTests.RepairTasks;

public class PartTests
{
    // ---------- Create ----------

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = Part.Create(TestConstants.Parts.Id, "Oil Filter", 25m, 2);

        var part = result.ShouldBeSuccess();
        Assert.Equal("Oil Filter", part.Name);
        Assert.Equal(25m, part.Price);
        Assert.Equal(2, part.Quantity);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_ReturnsNameRequired(string name)
    {
        var result = Part.Create(TestConstants.Parts.Id, name, 25m, 1);

        result.ShouldBeError(PartErrors.NameRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10001)]
    public void Create_WithPriceOutOfRange_ReturnsCostInvalid(decimal price)
    {
        var result = Part.Create(TestConstants.Parts.Id, "Oil Filter", price, 1);

        result.ShouldBeError(PartErrors.CostInvalid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void Create_WithQuantityOutOfRange_ReturnsQuantityInvalid(int quantity)
    {
        var result = Part.Create(TestConstants.Parts.Id, "Oil Filter", 25m, quantity);

        result.ShouldBeError(PartErrors.QuantityInvalid);
    }

    // ---------- Update ----------

    [Fact]
    public void Update_WithValidInputs_UpdatesFields()
    {
        var part = new PartBuilder().Build();

        var result = part.Update("Air Filter", 40m, 3);

        result.ShouldBeSuccess();
        Assert.Equal("Air Filter", part.Name);
        Assert.Equal(40m, part.Price);
        Assert.Equal(3, part.Quantity);
    }

    [Fact]
    public void Update_WithBlankName_ReturnsNameRequired()
    {
        var part = new PartBuilder().Build();

        var result = part.Update(" ", 40m, 3);

        result.ShouldBeError(PartErrors.NameRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10001)]
    public void Update_WithPriceOutOfRange_ReturnsCostInvalid(decimal price)
    {
        var part = new PartBuilder().Build();

        var result = part.Update("Air Filter", price, 3);

        result.ShouldBeError(PartErrors.CostInvalid);
    }

    [Fact]
    public void Update_WithNegativeQuantity_ReturnsQuantityInvalid()
    {
        var part = new PartBuilder().Build();

        var result = part.Update("Air Filter", 40m, -1);

        result.ShouldBeError(PartErrors.QuantityInvalid);
    }

    [Fact]
    public void Update_AllowsZeroQuantity_UnlikeCreate()
    {
        // Documents an intentional-looking asymmetry: Create rejects quantity 0 (must be >= 1),
        // but Update only rejects quantity < 0, so 0 is accepted here.
        var part = new PartBuilder().Build();

        var result = part.Update("Air Filter", 40m, 0);

        result.ShouldBeSuccess();
        Assert.Equal(0, part.Quantity);
    }
}
