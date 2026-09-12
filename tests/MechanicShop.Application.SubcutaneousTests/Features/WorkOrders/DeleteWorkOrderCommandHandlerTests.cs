using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class DeleteWorkOrderCommandHandlerTests : HandlerTestBase
{
    private DeleteWorkOrderCommandHandler CreateSut() => new(Context, Cache);

    private async Task<Guid> SeedWorkOrderAsync(WorkOrderState state)
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().WithState(state).Build();
        await SeedAsync(customer, labor, workOrder);
        return workOrder.Id;
    }

    [Fact]
    public async Task Handle_WhenScheduled_DeletesWorkOrder()
    {
        var id = await SeedWorkOrderAsync(WorkOrderState.Scheduled);
        var sut = CreateSut();

        var result = await sut.Handle(new DeleteWorkOrderCommand(id), CancellationToken.None);

        result.ShouldBeSuccess();
        Assert.Equal(0, await Context.WorkOrders.CountAsync());
    }

    [Fact]
    public async Task Handle_WhenMissing_ReturnsWorkOrderNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new DeleteWorkOrderCommand(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.WorkOrderNotFound);
    }

    [Theory]
    [InlineData(WorkOrderState.InProgress)]
    [InlineData(WorkOrderState.Completed)]
    [InlineData(WorkOrderState.Cancelled)]
    public async Task Handle_WhenNotScheduled_ReturnsReadonly(WorkOrderState state)
    {
        var id = await SeedWorkOrderAsync(state);
        var sut = CreateSut();

        var result = await sut.Handle(new DeleteWorkOrderCommand(id), CancellationToken.None);

        result.ShouldBeError(WorkOrderErrors.Readonly);
        Assert.Equal(1, await Context.WorkOrders.CountAsync());
    }
}
