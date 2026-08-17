using System.Linq.Expressions;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.Common.Constants;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Enums;
using MechanicShop.Domain.WorkOrders.Billing;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice;

public class IssueInvoiceCommandHandler(

    ILogger<IssueInvoiceCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    TimeProvider datetime
) : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
{
    private readonly ILogger<IssueInvoiceCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly TimeProvider _datetime = datetime;
    private readonly HybridCache _cache = cache;    
    public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
                                    .Include(w => w.Car)
                                    .ThenInclude(c => c.Customer)
                                    .Include(s => s.RepairTasks)
                                    .ThenInclude(r => r.Parts)
                                    .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId);


        if(workOrder is null)
        {
            _logger.LogWarning("This WorkOrder not Exist wit Id {workOrderId}",request.WorkOrderId);
            return ApplicationErrors.WorkOrderNotFound;
        }

        if(workOrder.State != WorkOrderState.Completed)
        {
            _logger.LogWarning("Invoice issuance rejected. WorkOrder {WorkOrderId} is not in completed.", request.WorkOrderId);

            return ApplicationErrors.WorkOrderMustBeCompletedForInvoicing;
        }

        Guid invoiceId = Guid.NewGuid();

        var lineItems = new List<InvoiceLineItem>();

        var lineNumber = 1;

        foreach (var (task, taskIndex) in workOrder.RepairTasks.Select((t, i) => (t, i + 1)))
        {
            var partsSummary = task.Parts.Any()
               ? string.Join(Environment.NewLine, task.Parts.Select(p => $"    • {p.Name} x{p.Quantity} @ {p.Price:C}"))
               : "    • No parts";

            var lineDescription =
                $"{taskIndex}: {task.Name}{Environment.NewLine}" +
                $"  Labor = {task.LaborCost:C}{Environment.NewLine}" +
                $"  Parts:{Environment.NewLine}{partsSummary}";

            var totalPartsCost = task.Parts.Sum(p => p.Price * p.Quantity);
            var totalTaskCost = task.LaborCost + totalPartsCost;

            var lineItemResult = InvoiceLineItem.Create(invoiceId,
                lineNumber: lineNumber++,
                description: lineDescription,
                quantity: 1,
                unitPrice: totalTaskCost);

            if (lineItemResult.IsError)
                return lineItemResult.Errors!;
            

            lineItems.Add(lineItemResult.Value);
        }

         var subtotal = lineItems.Sum(x => x.LineTotal);

        var taxAmount = subtotal * MechanicShopConstants.TaxRate;

        var discountAmount = workOrder.Discount ?? 0m;

        var createInvoiceResult = Invoice.Create(
            id: invoiceId,
            workOrderId: workOrder.Id,
            items: lineItems,
            discountAmount: discountAmount,
            taxAmount: taxAmount,
            datetime: _datetime);

        if (createInvoiceResult.IsError)
        {
            _logger.LogWarning(
                 "Invoice creation failed for WorkOrderId: {WorkOrderId}. Errors: {@Errors}",
                 request.WorkOrderId,
                 createInvoiceResult.Errors);

            return createInvoiceResult.Errors!;
        }
        var invoice = createInvoiceResult.Value;

        await _context.Invoices.AddAsync(invoice, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("invoice", cancellationToken);

        _logger.LogInformation("Invoice {InvoiceId} issued for WorkOrder {WorkOrderId}.", invoice.Id, workOrder.Id);

        return invoice.ToDto();

    }
}