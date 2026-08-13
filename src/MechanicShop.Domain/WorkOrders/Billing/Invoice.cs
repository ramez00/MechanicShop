using MechanicShop.Domain.Common;
using MechanicShop.Domain.workOrders.Enums;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Domain.workOrders.Billing;

public sealed class Invoice : AuditableEntity
{
    private readonly List<InvoiceLineItem> _lineItems = [];
    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems;
    public InvoiceStatus Status { get; private set; }
    public Guid WorkOrderId { get; }
    public DateTimeOffset IssuedAtUtc { get; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; }
    public decimal Subtotal => LineItems.Sum(x => x.LineTotal);
    public decimal Total => Subtotal - DiscountAmount + TaxAmount;
    public DateTimeOffset? PaidAt { get; private set; }
    public WorkOrder? WorkOrder { get; set; }
}