using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class RelocateWorkOrderCommandHandlerTests : HandlerTestBase
{
    private readonly IWorkOrderPolicy _policy = Substitute.For<IWorkOrderPolicy>();

    private static readonly Result<Success> Ok = Result.success;

    private RelocateWorkOrderCommandHandler CreateSut() => new(Context, _policy, Cache);

    private async Task<Guid> SeedScheduledWorkOrderAsync()
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Scheduled).Build();
        await SeedAsync(customer, labor, workOrder);
        return workOrder.Id;
    }

    private void ConfigureSpotAvailable() =>
        _policy.CheckSpotAvailabilityAsync(Arg.Any<Spot>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
               .Returns(Task.FromResult(Ok));

    private RelocateWorkOrderCommand Command(Guid id) =>
        new(id, TestConstants.UtcNow.AddDays(1), Spot.B);

    [Fact]
    public async Task Handle_WhenValid_RelocatesWorkOrder()
    {
        var id = await SeedScheduledWorkOrderAsync();
        ConfigureSpotAvailable();
        var sut = CreateSut();

        var result = await sut.Handle(Command(id), CancellationToken.None);

        result.ShouldBeSuccess();
        var persisted = await Context.WorkOrders.FindAsync(id);
        Assert.Equal(Spot.B, persisted!.Spot);
    }

    [Fact]
    public async Task Handle_WhenMissing_ReturnsWorkOrderNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(Command(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.WorkOrderNotFound);
    }

    [Fact]
    public async Task Handle_WhenSpotOccupied_ReturnsSpotInvalid()
    {
        var id = await SeedScheduledWorkOrderAsync();
        _policy.CheckSpotAvailabilityAsync(Arg.Any<Spot>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
               .Returns(Task.FromResult((Result<Success>)Error.Conflict("SpotTaken", "occupied")));
        var sut = CreateSut();

        var result = await sut.Handle(Command(id), CancellationToken.None);

        result.ShouldBeError(WorkOrderErrors.SpotInvalid);
    }

    [Fact]
    public async Task Handle_WhenLaborOccupied_ReturnsLaborOccupied()
    {
        var id = await SeedScheduledWorkOrderAsync();
        ConfigureSpotAvailable();
        _policy.IsLaborOccupied(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
               .Returns(true);
        var sut = CreateSut();

        var result = await sut.Handle(Command(id), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.LaborOccupied);
    }

    [Fact]
    public async Task Handle_WhenVehicleAlreadyScheduled_ReturnsConflict()
    {
        var id = await SeedScheduledWorkOrderAsync();
        ConfigureSpotAvailable();
        _policy.IsLaborOccupied(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
               .Returns(false);
        _policy.IsVehicleAlreadyScheduled(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<Guid?>())
               .Returns(true);
        var sut = CreateSut();

        var result = await sut.Handle(Command(id), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.VehicleSchedulingConflict);
    }
}
