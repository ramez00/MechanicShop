using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.Billing;

public class IssueInvoiceCommandHandlerTests : HandlerTestBase
{
    private IssueInvoiceCommandHandler CreateSut() =>
        new(Logger<IssueInvoiceCommandHandler>(), Context, Cache, Clock);

    /// <summary>Seeds a customer (with a car), a labor, and a work order that all line up by the shared test ids.</summary>
    private async Task SeedWorkOrderAsync(WorkOrderState state)
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().WithState(state).Build();

        await SeedAsync(customer, labor, workOrder);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderCompleted_IssuesInvoiceAndPersistsIt()
    {
        await SeedWorkOrderAsync(WorkOrderState.Completed);
        var sut = CreateSut();

        var result = await sut.Handle(new IssueInvoiceCommand(TestConstants.WorkOrders.Id), CancellationToken.None);

        var dto = result.ShouldBeSuccess();
        Assert.Equal(TestConstants.WorkOrders.Id, dto.WorkOrderId);
        Assert.NotEmpty(dto.items);

        var persisted = await Context.Invoices.FirstOrDefaultAsync(i => i.WorkOrderId == TestConstants.WorkOrders.Id);
        Assert.NotNull(persisted);
    }

    [Fact]
    public async Task Handle_ComputesTaxAsFifteenPercentOfSubtotal()
    {
        await SeedWorkOrderAsync(WorkOrderState.Completed);
        var sut = CreateSut();

        var result = await sut.Handle(new IssueInvoiceCommand(TestConstants.WorkOrders.Id), CancellationToken.None);

        var dto = result.ShouldBeSuccess();
        Assert.Equal(Math.Round(dto.Subtotal * 0.15m, 4), Math.Round(dto.TaxAmount, 4));
    }

    [Fact]
    public async Task Handle_WhenWorkOrderMissing_ReturnsWorkOrderNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new IssueInvoiceCommand(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.WorkOrderNotFound);
    }

    [Theory]
    [InlineData(WorkOrderState.Scheduled)]
    [InlineData(WorkOrderState.InProgress)]
    [InlineData(WorkOrderState.Cancelled)]
    public async Task Handle_WhenWorkOrderNotCompleted_ReturnsInvalidState(WorkOrderState state)
    {
        await SeedWorkOrderAsync(state);
        var sut = CreateSut();

        var result = await sut.Handle(new IssueInvoiceCommand(TestConstants.WorkOrders.Id), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.WorkOrderMustBeCompletedForInvoicing);
        Assert.Equal(0, await Context.Invoices.CountAsync());
    }
}
