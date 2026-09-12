using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.AssignLabor;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class AssignLaborCommandHandlerTests : HandlerTestBase
{
    private readonly IWorkOrderPolicy _policy = Substitute.For<IWorkOrderPolicy>();

    private AssignLaborCommandHandler CreateSut() => new(Context, _policy);

    private async Task<Guid> SeedScheduledWorkOrderAsync()
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().Build();
        await SeedAsync(customer, labor, workOrder);
        return workOrder.Id;
    }

    [Fact]
    public async Task Handle_WhenValid_AssignsLabor()
    {
        var workOrderId = await SeedScheduledWorkOrderAsync();
        var newLabor = new EmployeeBuilder().WithId(Guid.NewGuid()).Build();
        await SeedAsync(newLabor);
        _policy.IsLaborOccupied(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
               .Returns(false);
        var sut = CreateSut();

        var result = await sut.Handle(new AssignLaborCommand(workOrderId, newLabor.Id), CancellationToken.None);

        result.ShouldBeSuccess();
        var persisted = await Context.WorkOrders.FindAsync(workOrderId);
        Assert.Equal(newLabor.Id, persisted!.LaborId);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderMissing_ReturnsWorkOrderNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new AssignLaborCommand(Guid.NewGuid(), TestConstants.Employees.Id), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.WorkOrderNotFound);
    }

    [Fact]
    public async Task Handle_WhenLaborMissing_ReturnsLaborNotFound()
    {
        var workOrderId = await SeedScheduledWorkOrderAsync();
        var sut = CreateSut();

        var result = await sut.Handle(new AssignLaborCommand(workOrderId, Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.LaborNotFound);
    }

    [Fact]
    public async Task Handle_WhenLaborOccupied_ReturnsLaborOccupied()
    {
        var workOrderId = await SeedScheduledWorkOrderAsync();
        var newLabor = new EmployeeBuilder().WithId(Guid.NewGuid()).Build();
        await SeedAsync(newLabor);
        _policy.IsLaborOccupied(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
               .Returns(true);
        var sut = CreateSut();

        var result = await sut.Handle(new AssignLaborCommand(workOrderId, newLabor.Id), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.LaborOccupied);
    }
}
