using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Billing;

namespace MechanicShop.Domain.WorkOrders.Billing;

public sealed class InvoiceLineItem : AuditableEntity
{
    public Guid InvoiceId { get; }
    public int LineNumber { get; }
    public string Description { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal LineTotal => Quantity * UnitPrice;

    private InvoiceLineItem() { } // For EF Core

    private InvoiceLineItem(Guid invoiceId, int lineNumber, string description,
         int quantity, decimal unitPrice)
    {
        InvoiceId = invoiceId;
        LineNumber = lineNumber;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static Result<InvoiceLineItem> Create(Guid invoiceId, int lineNumber, string description,
        int quantity, decimal unitPrice)
    {
        if (invoiceId == Guid.Empty)
            return InvoiceLineItemErrors.InvoiceIdRequired;

        if (lineNumber <= 0)
            return InvoiceLineItemErrors.LineNumberInvalid;

        if (string.IsNullOrWhiteSpace(description))
            return InvoiceLineItemErrors.DescriptionRequired;

        if (quantity <= 0)
            return InvoiceLineItemErrors.QuantityInvalid;

        if (unitPrice <= 0)
            return InvoiceLineItemErrors.UnitPriceInvalid;

        return new InvoiceLineItem(invoiceId, lineNumber, description, quantity, unitPrice);
    }
}