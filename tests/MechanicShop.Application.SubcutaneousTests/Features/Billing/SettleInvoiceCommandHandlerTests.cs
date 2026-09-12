using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
using MechanicShop.Domain.workOrders.Billing;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.Billing;

public class SettleInvoiceCommandHandlerTests : HandlerTestBase
{
    private SettleInvoiceCommandHandler CreateSut() =>
        new(Logger<SettleInvoiceCommandHandler>(), Context, Cache, Clock);

    /// <summary>An Invoice has a required FK to a WorkOrder, which itself needs a Car and Labor — seed the whole graph.</summary>
    private async Task<Invoice> SeedInvoiceAsync(bool markPaid = false)
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().Build();
        var invoice = new InvoiceBuilder().WithWorkOrderId(workOrder.Id).WithTimeProvider(Clock).Build();

        if (markPaid)
            invoice.MarkAsPaid(Clock);

        await SeedAsync(customer, labor, workOrder, invoice);
        return invoice;
    }

    [Fact]
    public async Task Handle_WhenInvoiceUnpaid_MarksItPaid()
    {
        var invoice = await SeedInvoiceAsync();
        var sut = CreateSut();

        var result = await sut.Handle(new SettleInvoiceCommand(invoice.Id), CancellationToken.None);

        result.ShouldBeSuccess();

        var persisted = await Context.Invoices.FindAsync(invoice.Id);
        Assert.Equal(InvoiceStatus.paid, persisted!.Status);
        Assert.Equal(Clock.GetUtcNow(), persisted.PaidAt);
    }

    [Fact]
    public async Task Handle_WhenInvoiceMissing_ReturnsInvoiceNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new SettleInvoiceCommand(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.InvoiceNotFound);
    }

    [Fact]
    public async Task Handle_WhenInvoiceAlreadyPaid_ReturnsInvoiceLocked()
    {
        var invoice = await SeedInvoiceAsync(markPaid: true);
        var sut = CreateSut();

        var result = await sut.Handle(new SettleInvoiceCommand(invoice.Id), CancellationToken.None);

        result.ShouldBeError(InvoiceErrors.InvoiceLocked);
    }
}
