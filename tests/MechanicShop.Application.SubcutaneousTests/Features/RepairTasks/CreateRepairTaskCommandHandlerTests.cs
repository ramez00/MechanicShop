using MechanicShop.Application.Features.RepairTasks.Commands.Create;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.UnitTests.Features.RepairTasks;

public class CreateRepairTaskCommandHandlerTests : HandlerTestBase
{
    private CreateRepairTaskCommandHandler CreateSut() =>
        new(Logger<CreateRepairTaskCommandHandler>(), Context, Cache);

    private static CreateRepairTaskCommand ValidCommand(string name = "Brake Inspection") =>
        new(
            Name: name,
            LaborCost: 150m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: [new CreateRepairTaskPartCommand("Brake Pad", 40m, 2)]);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsRepairTaskAndReturnsDto()
    {
        var sut = CreateSut();

        var result = await sut.Handle(ValidCommand(), CancellationToken.None);

        var dto = result.ShouldBeSuccess();
        Assert.Equal("Brake Inspection", dto.Name);
        Assert.Single(dto.Parts);
        Assert.Equal(1, await Context.RepairTasks.CountAsync());
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ReturnsDuplicateName()
    {
        await SeedAsync(new RepairTaskBuilder().WithId(Guid.NewGuid()).WithName("Oil Change").Build());
        var sut = CreateSut();

        var result = await sut.Handle(ValidCommand(name: "Oil Change"), CancellationToken.None);

        result.ShouldBeError(RepairTaskErrors.DuplicateName);
    }

    [Fact]
    public async Task Handle_WithInvalidPart_ReturnsErrorAndPersistsNothing()
    {
        var sut = CreateSut();
        var command = ValidCommand() with
        {
            Parts = [new CreateRepairTaskPartCommand("Bad Part", 0m, 1)] // cost must be > 0
        };

        var result = await sut.Handle(command, CancellationToken.None);

        result.ShouldBeError();
        Assert.Equal(0, await Context.RepairTasks.CountAsync());
    }
}
