using MechanicShop.Application.Features.Customers.Queries.GetCustomer;

namespace MechanicShop.Application.UnitTests.Features.Customers;

public class GetCustomersQueryHandlerTests : HandlerTestBase
{
    private GetCustomersQueryHandler CreateSut() => new(Context);

    [Fact]
    public async Task Handle_WhenNoCustomers_ReturnsEmptyList()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new GetCustomersQuery(), CancellationToken.None);

        var list = result.ShouldBeSuccess();
        Assert.Empty(list);
    }

    [Fact]
    public async Task Handle_ReturnsAllCustomersWithCars()
    {
        await SeedAsync(
            new CustomerBuilder().WithId(Guid.NewGuid()).WithEmail("a@example.com").WithCars(new CarBuilder().WithId(Guid.NewGuid()).Build()).Build(),
            new CustomerBuilder().WithId(Guid.NewGuid()).WithEmail("b@example.com").WithCars(new CarBuilder().WithId(Guid.NewGuid()).Build()).Build());
        var sut = CreateSut();

        var result = await sut.Handle(new GetCustomersQuery(), CancellationToken.None);

        var list = result.ShouldBeSuccess();
        Assert.Equal(2, list.Count);
        Assert.All(list, c => Assert.NotEmpty(c.Cars));
    }

    [Fact]
    public void Query_ExposesCustomerCacheContract()
    {
        var query = new GetCustomersQuery();

        Assert.Equal("customers", query.CacheKey);
        Assert.Contains("customer", query.Tags);
    }
}
