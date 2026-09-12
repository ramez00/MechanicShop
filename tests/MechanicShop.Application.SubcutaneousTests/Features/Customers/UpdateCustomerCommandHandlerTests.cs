using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Features.Customers.Commands.Update;

namespace MechanicShop.Application.UnitTests.Features.Customers;

public class UpdateCustomerCommandHandlerTests : HandlerTestBase
{
    private UpdateCustomerCommandHandler CreateSut() =>
        new(Logger<UpdateCustomerCommandHandler>(), Context, Cache);

    private static UpdateCarCommand ValidCar(Guid? id = null) =>
        new(id, "Honda", "Civic", 2021, "UPD-1234");

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesCustomerDetails()
    {
        var customer = new CustomerBuilder().Build();
        await SeedAsync(customer);
        var sut = CreateSut();

        var command = new UpdateCustomerCommand(
            customer.Id,
            Name: "Updated Name",
            PhoneNumber: "+12025559999",
            Email: "updated@example.com",
            Cars: [ValidCar(TestConstants.Cars.Id)]);

        var result = await sut.Handle(command, CancellationToken.None);

        result.ShouldBeSuccess();

        var persisted = await Context.Customers.FindAsync(customer.Id);
        Assert.Equal("Updated Name", persisted!.Name);
        Assert.Equal("updated@example.com", persisted.Email);
        Assert.Equal("+12025559999", persisted.PhoneNumber);
    }

    [Fact]
    public async Task Handle_WhenCustomerMissing_ReturnsCustomerNotFound()
    {
        var sut = CreateSut();

        var command = new UpdateCustomerCommand(
            Guid.NewGuid(), "Name", "+12025559999", "e@example.com", [ValidCar()]);

        var result = await sut.Handle(command, CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.CustomerNotFound);
    }

    [Fact]
    public async Task Handle_WithInvalidCar_ReturnsError()
    {
        var customer = new CustomerBuilder().Build();
        await SeedAsync(customer);
        var sut = CreateSut();

        var command = new UpdateCustomerCommand(
            customer.Id,
            "Name",
            "+12025559999",
            "e@example.com",
            Cars: [new UpdateCarCommand(null, "", "Civic", 2021, "UPD-1234")]); // blank make

        var result = await sut.Handle(command, CancellationToken.None);

        result.ShouldBeError();
    }
}
