using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Commands.SettleInvoice;


public class SettleInvoiceCommandHandler(
    ILogger<SettleInvoiceCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    TimeProvider datetime
    )
    : IRequestHandler<SettleInvoiceCommand, Result<Success>>
{
    private readonly ILogger<SettleInvoiceCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly TimeProvider _datetime = datetime;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Success>> Handle(SettleInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == request.InvoiceId,cancellationToken);

        if(invoice is null)
            return ApplicationErrors.InvoiceNotFound;

        var payInvoice = invoice.MarkAsPaid(_datetime);

        _logger.LogWarning("Invoice with Id {InvoiceId} PAID at {_datetime}",invoice.Id , _datetime);

        if(payInvoice.IsError)
            return payInvoice.Errors!;    

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("invoice", cancellationToken);

        return Result.success;
    }
}