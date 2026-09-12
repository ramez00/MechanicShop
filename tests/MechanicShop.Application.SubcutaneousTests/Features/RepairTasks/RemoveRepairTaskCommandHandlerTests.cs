using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Features.RepairTasks.Commands.Remove;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.RepairTasks;

public class RemoveRepairTaskCommandHandlerTests : HandlerTestBase
{
    private RemoveRepairTaskCommandHandler CreateSut() => new(Context, Cache);

    [Fact]
    public async Task Handle_WhenNotUsed_DeletesRepairTask()
    {
        var repairTask = new RepairTaskBuilder().Build();
        await SeedAsync(repairTask);
        var sut = CreateSut();

        var result = await sut.Handle(new RemoveRepairTaskCommand(repairTask.Id), CancellationToken.None);

        result.ShouldBeSuccess();
        Assert.Equal(0, await Context.RepairTasks.CountAsync());
    }

    [Fact]
    public async Task Handle_WhenRepairTaskMissing_ReturnsNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new RemoveRepairTaskCommand(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.RepairTaskNotFound);
    }

    [Fact]
    public async Task Handle_WhenUsedByInProgressWorkOrder_ReturnsInUse()
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.InProgress).Build();

        await SeedAsync(customer, labor, workOrder);
        var sut = CreateSut();

        // The work order's default repair task carries TestConstants.RepairTasks.Id.
        var result = await sut.Handle(new RemoveRepairTaskCommand(TestConstants.RepairTasks.Id), CancellationToken.None);

        result.ShouldBeError(RepairTaskErrors.InUse);
    }
}
