using MechanicShop.Domain.workOrders.Billing;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.Constants;
using MechanicShop.Tests.Common.Fakes;

namespace MechanicShop.Tests.Common.Builders;

public sealed class InvoiceBuilder
{
    private Guid _id = TestConstants.Invoices.Id;
    private Guid _workOrderId = TestConstants.WorkOrders.Id;
    private List<InvoiceLineItem> _lineItems = [new InvoiceLineItemBuilder().Build()];
    private decimal _discountAmount;
    private decimal _taxAmount;
    private TimeProvider _timeProvider = new FixedTimeProvider();

    public InvoiceBuilder WithId(Guid id) { _id = id; return this; }
    public InvoiceBuilder WithWorkOrderId(Guid workOrderId) { _workOrderId = workOrderId; return this; }
    public InvoiceBuilder WithLineItems(params InvoiceLineItem[] items) { _lineItems = [.. items]; return this; }
    public InvoiceBuilder WithNoLineItems() { _lineItems = []; return this; }
    public InvoiceBuilder WithDiscount(decimal discount) { _discountAmount = discount; return this; }
    public InvoiceBuilder WithTax(decimal tax) { _taxAmount = tax; return this; }
    public InvoiceBuilder WithTimeProvider(TimeProvider timeProvider) { _timeProvider = timeProvider; return this; }

    public Invoice Build()
    {
        var result = Invoice.Create(_id, _workOrderId, _lineItems, _discountAmount, _taxAmount, _timeProvider);
        if (result.IsError)
            throw new InvalidOperationException($"InvoiceBuilder produced an invalid Invoice: {result.TopError.Code}");
        return result.Value;
    }
}
