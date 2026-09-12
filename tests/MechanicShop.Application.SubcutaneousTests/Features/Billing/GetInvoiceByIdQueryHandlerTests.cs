using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.Billing;

public class GetInvoiceByIdQueryHandlerTests : HandlerTestBase
{
    private GetInvoiceByIdQueryHandler CreateSut() => new(Context);

    [Fact]
    public async Task Handle_WhenInvoiceExists_ReturnsDtoWithLineItemsAndCustomer()
    {
        // The DTO mapper walks Invoice -> WorkOrder -> Car -> Customer, so all must be present.
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Completed).Build();
        var invoice = new InvoiceBuilder().WithWorkOrderId(workOrder.Id).WithTimeProvider(Clock).Build();

        await SeedAsync(customer, labor, workOrder, invoice);
        var sut = CreateSut();

        var result = await sut.Handle(new GetInvoiceByIdQuery(invoice.Id), CancellationToken.None);

        var dto = result.ShouldBeSuccess();
        Assert.Equal(invoice.Id, dto.InvoiceId);
        Assert.NotEmpty(dto.items);
        Assert.NotNull(dto.Customer);
    }

    [Fact]
    public async Task Handle_WhenInvoiceMissing_ReturnsInvoiceNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new GetInvoiceByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.InvoiceNotFound);
    }
}
