using MechanicShop.Application.Features.Customers.Commands;
using MechanicShop.Application.Features.Customers.Create.Commands;
using MechanicShop.Domain.Customers;

namespace MechanicShop.Application.UnitTests.Features.Customers;

public class CreateCustomerHandlerTests : HandlerTestBase
{
    private CreateCustomerHandler CreateSut() =>
        new(Context, Logger<CreateCustomerHandler>(), Cache);

    private static CreateCustomerCommand ValidCommand(string email = "new.customer@example.com") =>
        new(
            Name: "New Customer",
            PhoneNumber: "+12025550111",
            Email: email,
            cars: [new CreateCarCommand("Toyota", "Corolla", 2020, "NEW-1234")]);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsCustomerAndReturnsDto()
    {
        var sut = CreateSut();
        var command = ValidCommand();

        var result = await sut.Handle(command, CancellationToken.None);

        var dto = result.ShouldBeSuccess();
        Assert.Equal("New Customer", dto.Name);
        Assert.Single(dto.Cars);

        var persisted = await Context.Customers.FindAsync(dto.Id);
        Assert.NotNull(persisted);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsCustomerAlreadyExists()
    {
        await SeedAsync(new CustomerBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("dupe@example.com")
            .Build());

        var sut = CreateSut();

        var result = await sut.Handle(ValidCommand(email: "dupe@example.com"), CancellationToken.None);

        result.ShouldBeError(CustomerErrors.CustomerAlreadyExists);
    }

    [Fact]
    public async Task Handle_WithInvalidCar_ReturnsCarError()
    {
        var sut = CreateSut();
        var command = ValidCommand() with
        {
            cars = [new CreateCarCommand("", "Corolla", 2020, "NEW-1234")] // blank make
        };

        var result = await sut.Handle(command, CancellationToken.None);

        result.ShouldBeError();
        Assert.Equal(0, await Context.Customers.CountAsync());
    }
}
