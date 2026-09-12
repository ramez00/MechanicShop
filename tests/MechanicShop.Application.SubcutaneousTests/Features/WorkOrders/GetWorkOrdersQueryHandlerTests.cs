using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class GetWorkOrdersQueryHandlerTests : HandlerTestBase
{
    private GetWorkOrdersQueryHandler CreateSut() => new(Context);

    private static WorkOrder BuildWorkOrder(WorkOrderState state, Spot spot)
    {
        var task = new RepairTaskBuilder()
            .WithId(Guid.NewGuid())
            .WithName($"Task-{Guid.NewGuid():N}")
            .WithParts(new PartBuilder().WithId(Guid.NewGuid()).Build())
            .Build();

        return new WorkOrderBuilder()
            .WithId(Guid.NewGuid())
            .WithState(state)
            .WithSpot(spot)
            .WithRepairTasks(task)
            .Build();
    }

    private async Task SeedTwoWorkOrdersAsync()
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        await SeedAsync(
            customer,
            labor,
            BuildWorkOrder(WorkOrderState.Scheduled, Spot.A),
            BuildWorkOrder(WorkOrderState.Completed, Spot.B));
    }

    // Sort by "spot" (an enum) — SQLite cannot ORDER BY the DateTimeOffset default ("createdAt").
    private static GetWorkOrdersQuery Query(WorkOrderState? state = null) =>
        new(Page: 1, PageSize: 10, SearchTerm: null, SortColumn: "spot", SortDirection: "asc", State: state);

    [Fact]
    public async Task Handle_ReturnsPaginatedList()
    {
        await SeedTwoWorkOrdersAsync();
        var sut = CreateSut();

        var result = await sut.Handle(Query(), CancellationToken.None);

        var page = result.ShouldBeSuccess();
        Assert.Equal(2, page.TotalCount);
        Assert.Equal(1, page.PageNumber);
        Assert.NotNull(page.Items);
        Assert.Equal(2, page.Items!.Count);
    }

    [Fact]
    public async Task Handle_WithStateFilter_ReturnsOnlyMatching()
    {
        await SeedTwoWorkOrdersAsync();
        var sut = CreateSut();

        var result = await sut.Handle(Query(state: WorkOrderState.Completed), CancellationToken.None);

        var page = result.ShouldBeSuccess();
        Assert.Equal(1, page.TotalCount);
        Assert.All(page.Items!, i => Assert.Equal(WorkOrderState.Completed, i.State));
    }

    [Fact]
    public async Task Handle_WhenEmpty_ReturnsEmptyPage()
    {
        var sut = CreateSut();

        var result = await sut.Handle(Query(), CancellationToken.None);

        var page = result.ShouldBeSuccess();
        Assert.Equal(0, page.TotalCount);
        Assert.Empty(page.Items!);
    }
}
