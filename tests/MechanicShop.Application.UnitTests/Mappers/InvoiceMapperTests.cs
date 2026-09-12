using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.workOrders.Billing;
using MechanicShop.Domain.workOrders.Enums;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.UnitTests.Mappers;

public class InvoiceMapperTests
{
    // Invoice.ToDto walks Invoice -> WorkOrder -> Car -> Customer, so the whole graph must be wired up.
    private static Invoice BuildInvoiceGraph(decimal discount = 0m, decimal tax = 0m)
    {
        var customer = new CustomerBuilder().Build();
        var car = new CarBuilder().Build();
        car.Customer = customer;

        var workOrder = new WorkOrderBuilder().Build();
        workOrder.Car = car;

        var invoice = new InvoiceBuilder()
            .WithWorkOrderId(workOrder.Id)
            .WithDiscount(discount)
            .WithTax(tax)
            .Build();
        invoice.WorkOrder = workOrder;

        return invoice;
    }

    [Fact]
    public void ToDto_ShouldMapInvoiceHeaderCustomerCarAndLineItems()
    {
        var invoice = BuildInvoiceGraph(discount: 10m, tax: 5m);
        var lineItem = invoice.LineItems[0];

        var dto = invoice.ToDto();

        Assert.Equal(invoice.Id, dto.InvoiceId);
        Assert.Equal(invoice.WorkOrderId, dto.WorkOrderId);
        Assert.Equal(invoice.IssuedAtUtc, dto.IssuedAtUtc);

        Assert.NotNull(dto.Customer);
        Assert.Equal(invoice.WorkOrder!.Car!.Customer!.Id, dto.Customer!.Id);
        Assert.NotNull(dto.car);
        Assert.Equal(invoice.WorkOrder.Car!.Id, dto.car!.Id);

        // Default line item: unit price 120 x qty 1 => subtotal 120; total = 120 - 10 + 5 = 115.
        Assert.Equal(120m, dto.Subtotal);
        Assert.Equal(10m, dto.DiscountAmount);
        Assert.Equal(5m, dto.TaxAmount);
        Assert.Equal(115m, dto.Total);
        Assert.Equal(InvoiceStatus.unpaid.ToString(), dto.PaymentStatus);

        Assert.Single(dto.items);
        Assert.Equal(lineItem.LineNumber, dto.items[0].LineNumber);
        Assert.Equal(lineItem.LineTotal, dto.items[0].LineTotal);
    }

    [Fact]
    public void ToDto_ForLineItem_ShouldMapAllFields()
    {
        var lineItem = new InvoiceLineItemBuilder()
            .WithLineNumber(3)
            .WithDescription("Brake Pads")
            .WithQuantity(2)
            .WithUnitPrice(40m)
            .Build();

        var dto = lineItem.ToDto();

        Assert.Equal(lineItem.InvoiceId, dto.InvoiceId);
        Assert.Equal(3, dto.LineNumber);
        Assert.Equal("Brake Pads", dto.Description);
        Assert.Equal(2, dto.Quantity);
        Assert.Equal(40m, dto.UnitPrice);
        Assert.Equal(80m, dto.LineTotal); // 2 * 40
    }

    [Fact]
    public void ToDtos_ForInvoices_ShouldMapEach()
    {
        var invoices = new List<Invoice> { BuildInvoiceGraph(), BuildInvoiceGraph() };

        var dtos = InvoiceMapper.ToDtos(invoices);

        Assert.Equal(2, dtos.Count);
        Assert.All(dtos, d => Assert.NotNull(d.Customer));
    }

    [Fact]
    public void ToDtos_ForLineItems_ShouldMapEach()
    {
        var items = new[]
        {
            new InvoiceLineItemBuilder().WithLineNumber(1).Build(),
            new InvoiceLineItemBuilder().WithLineNumber(2).Build(),
        };

        var dtos = items.ToDtos();

        Assert.Equal(2, dtos.Count);
        Assert.Equal(1, dtos[0].LineNumber);
        Assert.Equal(2, dtos[1].LineNumber);
    }
}
