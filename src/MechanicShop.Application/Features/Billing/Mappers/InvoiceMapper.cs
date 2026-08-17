using MechanicShop.Application.customers.Mappers;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Features.Billing.Mappers;

public static class InvoiceMapper
{
    public static InvoiceDto ToDto(this Invoice invoice)
    {
        return new InvoiceDto
        {
            InvoiceId = invoice.Id,
            WorkOrderId = invoice.WorkOrderId,
            Customer = invoice.WorkOrder!.Car!.Customer!.ToDto(),
            car = invoice.WorkOrder.Car.ToDto(),
            IssuedAtUtc = invoice.IssuedAtUtc,
            Subtotal = invoice.Subtotal,
            TaxAmount = invoice.TaxAmount,
            DiscountAmount = invoice.DiscountAmount,
            Total = invoice.Total,
            PaymentStatus = invoice.Status.ToString(),
            items = invoice.LineItems.Select(x => x.ToDto()).ToList()
        };
    }

    public static List<InvoiceDto> ToDtos(List<Invoice> invoices) 
        => [.. invoices.Select(i => i.ToDto())];   

    public static InvoiceLineItemDto ToDto(this InvoiceLineItem item)
    {
        return new InvoiceLineItemDto
        {
            InvoiceId = item.InvoiceId,
            LineNumber = item.LineNumber,
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.LineTotal
        };
    }

    public static List<InvoiceLineItemDto> ToDtos(this IEnumerable<InvoiceLineItem> entities)
        =>  [.. entities.Select(e => e.ToDto())];
}