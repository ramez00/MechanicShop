using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Domain.Customers;

namespace MechanicShop.Application.UnitTests.Features.Customers;

public class GetCustomerByIdHandlerTests : HandlerTestBase
{
    private GetCustomerByIdHandler CreateSut() =>
        new(Context, Logger<GetCustomerByIdHandler>());

    [Fact]
    public async Task Handle_WhenCustomerExists_ReturnsDto()
    {
        var customer = new CustomerBuilder().Build();
        await SeedAsync(customer);
        var sut = CreateSut();

        var result = await sut.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        var dto = result.ShouldBeSuccess();
        Assert.Equal(customer.Id, dto.Id);
        Assert.NotEmpty(dto.Cars);
    }

    [Fact]
    public async Task Handle_WhenCustomerMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var sut = CreateSut();

        var result = await sut.Handle(new GetCustomerByIdQuery(id), CancellationToken.None);

        result.ShouldBeError(CustomerErrors.CustomerNotFound(id));
    }
}
