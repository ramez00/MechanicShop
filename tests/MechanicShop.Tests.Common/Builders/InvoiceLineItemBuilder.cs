using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.Constants;

namespace MechanicShop.Tests.Common.Builders;

public sealed class InvoiceLineItemBuilder
{
    private Guid _invoiceId = TestConstants.Invoices.Id;
    private int _lineNumber = 1;
    private string _description = "Oil Change - Labor";
    private int _quantity = 1;
    private decimal _unitPrice = 120m;

    public InvoiceLineItemBuilder WithInvoiceId(Guid invoiceId) { _invoiceId = invoiceId; return this; }
    public InvoiceLineItemBuilder WithLineNumber(int lineNumber) { _lineNumber = lineNumber; return this; }
    public InvoiceLineItemBuilder WithDescription(string description) { _description = description; return this; }
    public InvoiceLineItemBuilder WithQuantity(int quantity) { _quantity = quantity; return this; }
    public InvoiceLineItemBuilder WithUnitPrice(decimal unitPrice) { _unitPrice = unitPrice; return this; }

    public InvoiceLineItem Build()
    {
        var result = InvoiceLineItem.Create(_invoiceId, _lineNumber, _description, _quantity, _unitPrice);
        if (result.IsError)
            throw new InvalidOperationException($"InvoiceLineItemBuilder produced an invalid line item: {result.TopError.Code}");
        return result.Value;
    }
}
