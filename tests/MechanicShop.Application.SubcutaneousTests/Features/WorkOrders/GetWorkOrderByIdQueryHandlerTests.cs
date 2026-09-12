using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderByIdQuery;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class GetWorkOrderByIdQueryHandlerTests : HandlerTestBase
{
    private GetWorkOrderByIdQueryHandler CreateSut() => new(Context);

    [Fact]
    public async Task Handle_WhenExists_ReturnsDto()
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Scheduled).Build();
        await SeedAsync(customer, labor, workOrder);
        var sut = CreateSut();

        var result = await sut.Handle(new GetWorkOrderByIdQuery(workOrder.Id), CancellationToken.None);

        var dto = result.ShouldBeSuccess();
        Assert.Equal(workOrder.Id, dto.WorkOrderId);
        Assert.NotEmpty(dto.RepairTasks);
        Assert.NotNull(dto.Vehicle);
        Assert.NotNull(dto.Labor);
    }

    [Fact]
    public async Task Handle_WhenMissing_ReturnsWorkOrderNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new GetWorkOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.WorkOrderNotFound);
    }
}
