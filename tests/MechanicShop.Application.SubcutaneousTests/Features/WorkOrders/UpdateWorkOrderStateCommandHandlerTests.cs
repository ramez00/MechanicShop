using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrder;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class UpdateWorkOrderStateCommandHandlerTests : HandlerTestBase
{
    private UpdateWorkOrderStateCommandHandler CreateSut() => new(Context, Cache);

    private async Task<Guid> SeedWorkOrderAsync(WorkOrderState state)
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().WithState(state).Build();
        await SeedAsync(customer, labor, workOrder);
        return workOrder.Id;
    }

    [Fact]
    public async Task Handle_WhenValidTransition_UpdatesState()
    {
        var id = await SeedWorkOrderAsync(WorkOrderState.Scheduled);
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateWorkOrderStateCommand(id, WorkOrderState.InProgress), CancellationToken.None);

        result.ShouldBeSuccess();
        var persisted = await Context.WorkOrders.FindAsync(id);
        Assert.Equal(WorkOrderState.InProgress, persisted!.State);
    }

    [Fact]
    public async Task Handle_WhenMissing_ReturnsWorkOrderNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateWorkOrderStateCommand(Guid.NewGuid(), WorkOrderState.InProgress), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.WorkOrderNotFound);
    }

    [Fact]
    public async Task Handle_WhenTransitionInvalid_ReturnsError()
    {
        // Scheduled -> Completed is not an allowed direct transition.
        var id = await SeedWorkOrderAsync(WorkOrderState.Scheduled);
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateWorkOrderStateCommand(id, WorkOrderState.Completed), CancellationToken.None);

        result.ShouldBeErrorWithCode("WorkOrderErrors.InvalidStateTransition");
    }
}
