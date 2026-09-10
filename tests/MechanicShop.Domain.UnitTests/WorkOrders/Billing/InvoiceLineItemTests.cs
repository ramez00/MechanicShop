using MechanicShop.Domain.workOrders.Billing;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Domain.UnitTests.WorkOrders.Billing;

public class InvoiceLineItemTests
{
    private static readonly Guid InvoiceId = TestConstants.Invoices.Id;

    // ---------- Create ----------

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = InvoiceLineItem.Create(InvoiceId, 1, "Labor", 2, 50m);

        var item = result.ShouldBeSuccess();
        Assert.Equal(1, item.LineNumber);
        Assert.Equal("Labor", item.Description);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(50m, item.UnitPrice);
    }

    [Fact]
    public void Create_WithEmptyInvoiceId_ReturnsInvoiceIdRequired()
    {
        var result = InvoiceLineItem.Create(Guid.Empty, 1, "Labor", 1, 50m);

        result.ShouldBeError(InvoiceLineItemErrors.InvoiceIdRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveLineNumber_ReturnsLineNumberInvalid(int lineNumber)
    {
        var result = InvoiceLineItem.Create(InvoiceId, lineNumber, "Labor", 1, 50m);

        result.ShouldBeError(InvoiceLineItemErrors.LineNumberInvalid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithBlankDescription_ReturnsDescriptionRequired(string description)
    {
        var result = InvoiceLineItem.Create(InvoiceId, 1, description, 1, 50m);

        result.ShouldBeError(InvoiceLineItemErrors.DescriptionRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveQuantity_ReturnsQuantityInvalid(int quantity)
    {
        var result = InvoiceLineItem.Create(InvoiceId, 1, "Labor", quantity, 50m);

        result.ShouldBeError(InvoiceLineItemErrors.QuantityInvalid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveUnitPrice_ReturnsUnitPriceInvalid(decimal unitPrice)
    {
        var result = InvoiceLineItem.Create(InvoiceId, 1, "Labor", 1, unitPrice);

        result.ShouldBeError(InvoiceLineItemErrors.UnitPriceInvalid);
    }

    // ---------- LineTotal ----------

    [Fact]
    public void LineTotal_IsQuantityTimesUnitPrice()
    {
        var item = new InvoiceLineItemBuilder().WithQuantity(3).WithUnitPrice(25m).Build();

        Assert.Equal(75m, item.LineTotal);
    }
}
