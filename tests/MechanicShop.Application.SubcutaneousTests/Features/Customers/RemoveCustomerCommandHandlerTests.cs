using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Features.Customers.Commands.Remove;
using MechanicShop.Domain.Customers;

namespace MechanicShop.Application.UnitTests.Features.Customers;

public class RemoveCustomerCommandHandlerTests : HandlerTestBase
{
    private RemoveCustomerCommandHandler CreateSut() =>
        new(Context, Logger<RemoveCustomerCommandHandler>(), Cache);

    [Fact]
    public async Task Handle_WhenCustomerHasNoWorkOrders_DeletesCustomer()
    {
        var customer = new CustomerBuilder().Build();
        await SeedAsync(customer);
        var sut = CreateSut();

        var result = await sut.Handle(new RemoveCustomerCommand(customer.Id), CancellationToken.None);

        result.ShouldBeSuccess();
        Assert.Equal(0, await Context.Customers.CountAsync());
    }

    [Fact]
    public async Task Handle_WhenCustomerMissing_ReturnsCustomerNotFound()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new RemoveCustomerCommand(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.CustomerNotFound);
    }

    [Fact]
    public async Task Handle_WhenCustomerHasWorkOrders_ReturnsCannotDelete()
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().Build(); // CarId lines up with the customer's default car

        await SeedAsync(customer, labor, workOrder);
        var sut = CreateSut();

        var result = await sut.Handle(new RemoveCustomerCommand(customer.Id), CancellationToken.None);

        result.ShouldBeError(CustomerErrors.CannotDeleteCustomerWithActiveOrders);
        Assert.Equal(1, await Context.Customers.CountAsync());
    }
}
