using MechanicShop.Application.Features.Billing.Queries;

namespace MechanicShop.Application.UnitTests.Features.Billing;

public class GetInvoicePdfQueryValidatorTests
{
    private readonly GetInvoicePdfQueryValidator _validator = new();

    [Fact]
    public void Validate_WithInvoiceId_IsValid()
    {
        var result = _validator.Validate(new GetInvoicePdfQuery(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyInvoiceId_IsInvalid()
    {
        var result = _validator.Validate(new GetInvoicePdfQuery(Guid.Empty));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetInvoicePdfQuery.InvoiceId));
    }
}
