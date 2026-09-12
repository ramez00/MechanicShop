using MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

namespace MechanicShop.Application.UnitTests.Features.Billing;

public class GetInvoiceByIdQueryValidatorTests
{
    private readonly GetInvoiceByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WithInvoiceId_IsValid()
    {
        var result = _validator.Validate(new GetInvoiceByIdQuery(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyInvoiceId_IsInvalid()
    {
        var result = _validator.Validate(new GetInvoiceByIdQuery(Guid.Empty));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetInvoiceByIdQuery.InvoiceId));
    }

    [Fact]
    public void CacheKey_IncludesInvoiceId()
    {
        var id = Guid.NewGuid();
        var query = new GetInvoiceByIdQuery(id);

        Assert.Equal($"invoice_{id}", query.CacheKey);
        Assert.Contains("invoice", query.Tags);
    }
}
