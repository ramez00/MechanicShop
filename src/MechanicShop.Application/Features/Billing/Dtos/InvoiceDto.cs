using MechanicShop.Application.Features.Customers.Dtos;

namespace MechanicShop.Application.Features.Billing.Dtos;

public class InvoiceDto
{
    public Guid InvoiceId {get; set;}
    public Guid WorkOrderId {get; set ;}
    public CustomerDto? Customer { get; set; }
    public CarDto? car { get; set; }
    public DateTimeOffset IssuedAtUtc {get; set;}
    public decimal? DiscountAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string? PaymentStatus { get; set; }
    public List<InvoiceLineItemDto> items {get; set;} = [];
}