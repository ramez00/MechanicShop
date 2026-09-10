using MechanicShop.Domain.workOrders.Billing;
using MechanicShop.Domain.workOrders.Enums;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.Fakes;

namespace MechanicShop.Domain.UnitTests.WorkOrders.Billing;

public class InvoiceTests
{
    private static readonly FixedTimeProvider Clock = new(TestConstants.UtcNow);

    private static List<InvoiceLineItem> OneLine(decimal unitPrice = 100m, int qty = 1) =>
        [new InvoiceLineItemBuilder().WithUnitPrice(unitPrice).WithQuantity(qty).Build()];

    // ---------- Create ----------

    [Fact]
    public void Create_WithValidInputs_ReturnsUnpaidInvoiceAtProvidedTime()
    {
        var result = Invoice.Create(TestConstants.Invoices.Id, TestConstants.WorkOrders.Id, OneLine(), 0m, 0m, Clock);

        var invoice = result.ShouldBeSuccess();
        Assert.Equal(InvoiceStatus.unpaid, invoice.Status);
        Assert.Equal(TestConstants.UtcNow, invoice.IssuedAtUtc);
    }

    [Fact]
    public void Create_WithEmptyWorkOrderId_ReturnsWorkOrderIdInvalid()
    {
        var result = Invoice.Create(TestConstants.Invoices.Id, Guid.Empty, OneLine(), 0m, 0m, Clock);

        result.ShouldBeError(InvoiceErrors.WorkOrderIdInvalid);
    }

    [Fact]
    public void Create_WithNoLineItems_ReturnsLineItemsEmpty()
    {
        var result = Invoice.Create(TestConstants.Invoices.Id, TestConstants.WorkOrders.Id, [], 0m, 0m, Clock);

        result.ShouldBeError(InvoiceErrors.LineItemsEmpty);
    }

    // ---------- Subtotal / Total ----------

    [Fact]
    public void Total_IsSubtotalMinusDiscountPlusTax()
    {
        var invoice = new InvoiceBuilder()
            .WithLineItems(new InvoiceLineItemBuilder().WithUnitPrice(100m).WithQuantity(2).Build()) // subtotal 200
            .WithDiscount(20m)
            .WithTax(15m)
            .WithTimeProvider(Clock)
            .Build();

        Assert.Equal(200m, invoice.Subtotal);
        Assert.Equal(195m, invoice.Total); // 200 - 20 + 15
    }

    // ---------- ApplyDiscount ----------

    [Fact]
    public void ApplyDiscount_WhenUnpaidAndValid_SetsDiscount()
    {
        var invoice = new InvoiceBuilder().WithLineItems(OneLine(100m)[0]).WithTimeProvider(Clock).Build();

        var result = invoice.ApplyDiscount(30m);

        result.ShouldBeSuccess();
        Assert.Equal(30m, invoice.DiscountAmount);
    }

    [Fact]
    public void ApplyDiscount_WhenPaid_ReturnsInvoiceLocked()
    {
        var invoice = new InvoiceBuilder().WithLineItems(OneLine(100m)[0]).WithTimeProvider(Clock).Build();
        invoice.MarkAsPaid(Clock).ShouldBeSuccess();

        var result = invoice.ApplyDiscount(10m);

        result.ShouldBeError(InvoiceErrors.InvoiceLocked);
    }

    [Fact]
    public void ApplyDiscount_WhenNegative_ReturnsDiscountNegative()
    {
        var invoice = new InvoiceBuilder().WithLineItems(OneLine(100m)[0]).WithTimeProvider(Clock).Build();

        var result = invoice.ApplyDiscount(-1m);

        result.ShouldBeError(InvoiceErrors.DiscountNegative);
    }

    [Fact]
    public void ApplyDiscount_WhenExceedsSubtotal_ReturnsDiscountExceedsSubtotal()
    {
        var invoice = new InvoiceBuilder().WithLineItems(OneLine(100m)[0]).WithTimeProvider(Clock).Build();

        var result = invoice.ApplyDiscount(1000m);

        result.ShouldBeError(InvoiceErrors.DiscountExceedsSubtotal);
    }

    // ---------- MarkAsPaid ----------

    [Fact]
    public void MarkAsPaid_WhenUnpaid_SetsPaidStatusAndTimestamp()
    {
        var invoice = new InvoiceBuilder().WithTimeProvider(Clock).Build();

        var result = invoice.MarkAsPaid(Clock);

        result.ShouldBeSuccess();
        Assert.Equal(InvoiceStatus.paid, invoice.Status);
        Assert.Equal(TestConstants.UtcNow, invoice.PaidAt);
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_ReturnsInvoiceLocked()
    {
        var invoice = new InvoiceBuilder().WithTimeProvider(Clock).Build();
        invoice.MarkAsPaid(Clock).ShouldBeSuccess();

        var result = invoice.MarkAsPaid(Clock);

        result.ShouldBeError(InvoiceErrors.InvoiceLocked);
    }
}
