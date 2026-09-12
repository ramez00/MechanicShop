using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderByIdQuery;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class GetWorkOrderByIdQueryValidatorTests
{
    private readonly GetWorkOrderByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WithId_IsValid()
    {
        Assert.True(_validator.Validate(new GetWorkOrderByIdQuery(Guid.NewGuid())).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_IsInvalid()
    {
        var result = _validator.Validate(new GetWorkOrderByIdQuery(Guid.Empty));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetWorkOrderByIdQuery.WorkOrderId));
    }

    [Fact]
    public void Query_ExposesWorkOrderCacheContract()
    {
        var id = Guid.NewGuid();
        var query = new GetWorkOrderByIdQuery(id);

        Assert.Equal($"work-order:{id}", query.CacheKey);
        Assert.Contains("work-order", query.Tags);
    }
}
