using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;

namespace MechanicShop.Application.UnitTests.Features.Billing;

public class SettleInvoiceCommandValidatorTests
{
    private readonly SettleInvoiceCommandValidator _validator = new();

    [Fact]
    public void Validate_WithInvoiceId_IsValid()
    {
        var result = _validator.Validate(new SettleInvoiceCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyInvoiceId_IsInvalidWithExpectedCode()
    {
        var result = _validator.Validate(new SettleInvoiceCommand(Guid.Empty));

        Assert.Contains(result.Errors, e => e.ErrorCode == "InvoiceId_Is_Required");
    }
}
