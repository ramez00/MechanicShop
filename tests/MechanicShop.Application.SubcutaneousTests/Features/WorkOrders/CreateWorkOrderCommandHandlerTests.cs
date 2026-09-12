using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class CreateWorkOrderCommandHandlerTests : HandlerTestBase
{
    private readonly IWorkOrderPolicy _policy = Substitute.For<IWorkOrderPolicy>();

    private static readonly Result<Success> Ok = Result.success;

    private CreateWorkOrderCommandHandler CreateSut() =>
        new(Logger<CreateWorkOrderCommandHandler>(), Context, Cache, _policy);

    private void ConfigurePolicyPermissive()
    {
        _policy.IsOutsideOperatingHours(Arg.Any<DateTimeOffset>(), Arg.Any<TimeSpan>()).Returns(false);
        _policy.ValidateMinimumRequirement(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>()).Returns(Ok);
        _policy.CheckSpotAvailabilityAsync(Arg.Any<Spot>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
               .Returns(Task.FromResult(Ok));
    }

    /// <summary>Seeds a customer (owns the car), a labor, and a repair task; returns a command wired to those ids.</summary>
    private async Task<CreateWorkOrderCommand> SeedAndBuildCommandAsync()
    {
        var customer = new CustomerBuilder().Build();          // car id = Cars.Id
        var labor = new EmployeeBuilder().Build();             // id = Employees.Id
        var repairTask = new RepairTaskBuilder().Build();      // id = RepairTasks.Id
        await SeedAsync(customer, labor, repairTask);

        return new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: TestConstants.Cars.Id,
            StartAt: TestConstants.UtcNow.AddDays(1),
            RepairTaskIds: [TestConstants.RepairTasks.Id],
            LaborId: TestConstants.Employees.Id);
    }

    // NOTE: the fully-valid happy path is exercised in the SQL Server integration tests, not here.
    // The handler's vehicle-conflict guard uses `StartAtUtc.Date`, which the SQLite provider cannot
    // translate, so the success path cannot be driven end-to-end against the in-memory SQLite store.

    [Fact]
    public async Task Handle_WhenRepairTaskMissing_ReturnsRepairTaskNotFound()
    {
        // No repair task seeded, but the command references one.
        var command = new CreateWorkOrderCommand(
            Spot.A, TestConstants.Cars.Id, TestConstants.UtcNow.AddDays(1), [Guid.NewGuid()], TestConstants.Employees.Id);
        var sut = CreateSut();

        var result = await sut.Handle(command, CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.RepairTaskNotFound);
    }

    [Fact]
    public async Task Handle_WhenOutsideOperatingHours_ReturnsConflict()
    {
        var command = await SeedAndBuildCommandAsync();
        _policy.IsOutsideOperatingHours(Arg.Any<DateTimeOffset>(), Arg.Any<TimeSpan>()).Returns(true);
        var sut = CreateSut();

        var result = await sut.Handle(command, CancellationToken.None);

        result.ShouldBeErrorWithCode("ApplicationErrors.WorkOrder.Outside.OperatingHours");
    }

    [Fact]
    public async Task Handle_WhenBelowMinimumDuration_ReturnsError()
    {
        var command = await SeedAndBuildCommandAsync();
        _policy.IsOutsideOperatingHours(Arg.Any<DateTimeOffset>(), Arg.Any<TimeSpan>()).Returns(false);
        _policy.ValidateMinimumRequirement(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
               .Returns((Result<Success>)Error.Conflict("MinDuration", "too short"));
        var sut = CreateSut();

        var result = await sut.Handle(command, CancellationToken.None);

        result.ShouldBeErrorWithCode("MinDuration");
    }

    [Fact]
    public async Task Handle_WhenSpotUnavailable_ReturnsError()
    {
        var command = await SeedAndBuildCommandAsync();
        _policy.IsOutsideOperatingHours(Arg.Any<DateTimeOffset>(), Arg.Any<TimeSpan>()).Returns(false);
        _policy.ValidateMinimumRequirement(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>()).Returns(Ok);
        _policy.CheckSpotAvailabilityAsync(Arg.Any<Spot>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
               .Returns(Task.FromResult((Result<Success>)Error.Conflict("SpotTaken", "occupied")));
        var sut = CreateSut();

        var result = await sut.Handle(command, CancellationToken.None);

        result.ShouldBeErrorWithCode("SpotTaken");
    }

    [Fact]
    public async Task Handle_WhenVehicleMissing_ReturnsVehicleNotFound()
    {
        // Seed only the labor + repair task; the vehicle referenced by the command does not exist.
        var labor = new EmployeeBuilder().Build();
        var repairTask = new RepairTaskBuilder().Build();
        await SeedAsync(labor, repairTask);
        ConfigurePolicyPermissive();
        var command = new CreateWorkOrderCommand(
            Spot.A, Guid.NewGuid(), TestConstants.UtcNow.AddDays(1), [TestConstants.RepairTasks.Id], TestConstants.Employees.Id);
        var sut = CreateSut();

        var result = await sut.Handle(command, CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.VehicleNotFound);
    }
}
