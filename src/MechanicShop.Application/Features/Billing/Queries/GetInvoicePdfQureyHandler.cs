using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Queries;

public class GetInvoicePdfQureyHandler(
    ILogger<GetInvoicePdfQureyHandler> logger,
    IInvoicePdfGenerator pdfGenerator,
    IAppDbContext context
) : IRequestHandler<GetInvoicePdfQuery, Result<InvoicePdfDto>>
{
    private readonly ILogger<GetInvoicePdfQureyHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly IInvoicePdfGenerator _pdfGenerator = pdfGenerator;
    public async Task<Result<InvoicePdfDto>> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        var invoice =await _context.Invoices.AsNoTracking()
                                    .Include(i => i.LineItems)
                                    .FirstOrDefaultAsync(i => i.Id == request.InvoiceId);

        if(invoice is null)
            return ApplicationErrors.InvoiceNotFound;

        try
        {
            var pdfBytes = pdfGenerator.Generate(invoice);

            var invoicePdf = new InvoicePdfDto
            {
                Content = pdfBytes,
                FileName = $"invoice-{invoice.Id}.pdf"
            };

            return invoicePdf;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate PDF for InvoiceId: {InvoiceId}", request.InvoiceId);
            return Error.Failure("An error occurred while generating the invoice PDF.");
        }
    }
}