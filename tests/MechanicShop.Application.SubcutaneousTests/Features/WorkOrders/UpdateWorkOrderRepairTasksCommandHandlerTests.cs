using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class UpdateWorkOrderRepairTasksCommandHandlerTests : HandlerTestBase
{
    private readonly IWorkOrderPolicy _policy = Substitute.For<IWorkOrderPolicy>();

    private static readonly Result<Success> Ok = Result.success;

    private UpdateWorkOrderRepairTasksCommandHandler CreateSut() => new(Context, _policy, Cache);

    private async Task<(Guid workOrderId, Guid newTaskId)> SeedAsync_WorkOrderAndSpareTask()
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Scheduled).Build();

        var spareTaskId = Guid.NewGuid();
        var spareTask = new RepairTaskBuilder()
            .WithId(spareTaskId)
            .WithName("Extra Task")
            .WithParts(new PartBuilder().WithId(Guid.NewGuid()).Build())
            .Build();

        await SeedAsync(customer, labor, workOrder, spareTask);
        return (workOrder.Id, spareTaskId);
    }

    [Fact]
    public async Task Handle_WhenValid_AddsRepairTask()
    {
        var (workOrderId, newTaskId) = await SeedAsync_WorkOrderAndSpareTask();
        _policy.IsOutsideOperatingHours(Arg.Any<DateTimeOffset>(), Arg.Any<TimeSpan>()).Returns(false);
        _policy.CheckSpotAvailabilityAsync(Arg.Any<Spot>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
               .Returns(Task.FromResult(Ok));
        _policy.IsLaborOccupied(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>()).Returns(false);
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateWorkOrderRepairTasksCommand(workOrderId, [newTaskId]), CancellationToken.None);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Handle_WhenWorkOrderMissing_ReturnsWorkOrderNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), [Guid.NewGuid()]), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.WorkOrderNotFound);
    }

    [Fact]
    public async Task Handle_WhenNoRepairTasks_ReturnsAtLeastOneRequired()
    {
        var (workOrderId, _) = await SeedAsync_WorkOrderAndSpareTask();
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateWorkOrderRepairTasksCommand(workOrderId, []), CancellationToken.None);

        result.ShouldBeError(RepairTaskErrors.AtLeastOneRepairTaskIsRequired);
    }

    [Fact]
    public async Task Handle_WhenOutsideOperatingHours_ReturnsConflict()
    {
        var (workOrderId, newTaskId) = await SeedAsync_WorkOrderAndSpareTask();
        _policy.IsOutsideOperatingHours(Arg.Any<DateTimeOffset>(), Arg.Any<TimeSpan>()).Returns(true);
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateWorkOrderRepairTasksCommand(workOrderId, [newTaskId]), CancellationToken.None);

        result.ShouldBeErrorWithCode("WorkOrder_Outside_OperatingHours");
    }
}
