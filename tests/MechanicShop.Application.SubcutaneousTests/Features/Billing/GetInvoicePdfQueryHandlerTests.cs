using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Queries;
using MechanicShop.Domain.workOrders.Billing;

namespace MechanicShop.Application.UnitTests.Features.Billing;

public class GetInvoicePdfQueryHandlerTests : HandlerTestBase
{
    private readonly IInvoicePdfGenerator _pdfGenerator = Substitute.For<IInvoicePdfGenerator>();

    private GetInvoicePdfQureyHandler CreateSut() =>
        new(Logger<GetInvoicePdfQureyHandler>(), _pdfGenerator, Context);

    /// <summary>An Invoice has a required FK to a WorkOrder, which itself needs a Car and Labor — seed the whole graph.</summary>
    private async Task<Invoice> SeedInvoiceAsync()
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().Build();
        var invoice = new InvoiceBuilder().WithWorkOrderId(workOrder.Id).WithTimeProvider(Clock).Build();

        await SeedAsync(customer, labor, workOrder, invoice);
        return invoice;
    }

    [Fact]
    public async Task Handle_WhenInvoiceExists_ReturnsPdfBytesWithFileName()
    {
        var invoice = await SeedInvoiceAsync();
        _pdfGenerator.Generate(Arg.Any<Invoice>()).Returns([1, 2, 3, 4]);
        var sut = CreateSut();

        var result = await sut.Handle(new GetInvoicePdfQuery(invoice.Id), CancellationToken.None);

        var dto = result.ShouldBeSuccess();
        Assert.Equal([1, 2, 3, 4], dto.Content);
        Assert.Equal($"invoice-{invoice.Id}.pdf", dto.FileName);
    }

    [Fact]
    public async Task Handle_WhenInvoiceMissing_ReturnsInvoiceNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new GetInvoicePdfQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.InvoiceNotFound);
    }

    [Fact]
    public async Task Handle_WhenGeneratorThrows_ReturnsFailure()
    {
        var invoice = await SeedInvoiceAsync();
        _pdfGenerator.Generate(Arg.Any<Invoice>()).Returns(_ => throw new InvalidOperationException("boom"));
        var sut = CreateSut();

        var result = await sut.Handle(new GetInvoicePdfQuery(invoice.Id), CancellationToken.None);

        result.ShouldBeErrorOfKind(ErrorKind.failure);
    }
}
