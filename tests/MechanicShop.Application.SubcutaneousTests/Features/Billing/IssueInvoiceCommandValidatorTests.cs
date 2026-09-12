using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;

namespace MechanicShop.Application.UnitTests.Features.Billing;

public class IssueInvoiceCommandValidatorTests
{
    private readonly IssueInvoiceCommandValidator _validator = new();

    [Fact]
    public void Validate_WithWorkOrderId_IsValid()
    {
        var result = _validator.Validate(new IssueInvoiceCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyWorkOrderId_IsInvalid()
    {
        var result = _validator.Validate(new IssueInvoiceCommand(Guid.Empty));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(IssueInvoiceCommand.WorkOrderId));
    }
}
