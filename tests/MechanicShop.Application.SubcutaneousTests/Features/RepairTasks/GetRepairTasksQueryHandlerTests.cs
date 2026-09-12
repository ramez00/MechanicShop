using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;

namespace MechanicShop.Application.UnitTests.Features.RepairTasks;

public class GetRepairTasksQueryHandlerTests : HandlerTestBase
{
    private GetRepairTasksQueryHandler CreateSut() => new(Context);

    [Fact]
    public async Task Handle_WhenNone_ReturnsEmptyList()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new GetRepairTasksQuery(), CancellationToken.None);

        Assert.Empty(result.ShouldBeSuccess());
    }

    [Fact]
    public async Task Handle_ReturnsAllRepairTasksWithParts()
    {
        await SeedAsync(
            new RepairTaskBuilder().WithId(Guid.NewGuid()).WithName("Task A")
                .WithParts(new PartBuilder().WithId(Guid.NewGuid()).Build()).Build(),
            new RepairTaskBuilder().WithId(Guid.NewGuid()).WithName("Task B")
                .WithParts(new PartBuilder().WithId(Guid.NewGuid()).Build()).Build());
        var sut = CreateSut();

        var result = await sut.Handle(new GetRepairTasksQuery(), CancellationToken.None);

        var list = result.ShouldBeSuccess();
        Assert.Equal(2, list.Count);
        Assert.All(list, t => Assert.NotEmpty(t.Parts));
    }
}
