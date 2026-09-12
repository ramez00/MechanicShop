using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasksById;

namespace MechanicShop.Application.UnitTests.Features.RepairTasks;

public class GetRepairTaskByIdQueryHandlerTests : HandlerTestBase
{
    private GetRepairTaskByIdQueryHandler CreateSut() => new(Context);

    [Fact]
    public async Task Handle_WhenExists_ReturnsDto()
    {
        var repairTask = new RepairTaskBuilder().Build();
        await SeedAsync(repairTask);
        var sut = CreateSut();

        var result = await sut.Handle(new GetRepairTaskByIdQuery(repairTask.Id), CancellationToken.None);

        var dto = result.ShouldBeSuccess();
        Assert.Equal(repairTask.Id, dto.RepairTaskId);
    }

    [Fact]
    public async Task Handle_WhenMissing_ReturnsNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new GetRepairTaskByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.RepairTaskNotFound);
    }
}
