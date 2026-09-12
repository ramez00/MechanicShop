using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Features.RepairTasks.Commands.Update;
using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.UnitTests.Features.RepairTasks;

public class UpdateRepairTaskCommandHandlerTests : HandlerTestBase
{
    private UpdateRepairTaskCommandHandler CreateSut() => new(Context, Cache);

    private static UpdateRepairTaskCommand Command(Guid id) =>
        new(
            RepairTaskId: id,
            Name: "Renamed Task",
            LaborCost: 200m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min60,
            Parts: [new UpdateRepairTaskPartCommand(null, "New Part", 30m, 2)]);

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesRepairTask()
    {
        var repairTask = new RepairTaskBuilder().Build();
        await SeedAsync(repairTask);
        var sut = CreateSut();

        var result = await sut.Handle(Command(repairTask.Id), CancellationToken.None);

        result.ShouldBeSuccess();

        var persisted = await Context.RepairTasks.Include(r => r.Parts).FirstAsync(r => r.Id == repairTask.Id);
        Assert.Equal("Renamed Task", persisted.Name);
        Assert.Equal(200m, persisted.LaborCost);
        Assert.Equal(RepairDurationInMinutes.Min60, persisted.EstimatedDurationInMins);
    }

    [Fact]
    public async Task Handle_WhenRepairTaskMissing_ReturnsNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(Command(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.RepairTaskNotFound);
    }

    [Fact]
    public async Task Handle_WithInvalidPart_ReturnsError()
    {
        var repairTask = new RepairTaskBuilder().Build();
        await SeedAsync(repairTask);
        var sut = CreateSut();

        var command = Command(repairTask.Id) with
        {
            Parts = [new UpdateRepairTaskPartCommand(null, "", 30m, 2)] // blank part name
        };

        var result = await sut.Handle(command, CancellationToken.None);

        result.ShouldBeError();
    }
}
