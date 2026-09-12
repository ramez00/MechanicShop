using MechanicShop.Application.Features.Dashboard.Queries;

namespace MechanicShop.Application.UnitTests.Features.Dashboard;

public class GetWorkOrderStatsQueryValidatorTests
{
    private readonly GetWorkOrderStatsQueryValidator _validator = new();

    [Fact]
    public void Validate_WithRealDate_IsValid()
    {
        var result = _validator.Validate(new GetWorkOrderStatsQuery(new DateOnly(2026, 1, 15)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithDefaultDate_IsInvalid()
    {
        var result = _validator.Validate(new GetWorkOrderStatsQuery(default));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetWorkOrderStatsQuery.date));
    }
}
