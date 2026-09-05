using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler(IAppDbContext context) : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
                                    .AsNoTracking()
                                    .Include(i => i.LineItems)
                                    .Include(i => i.WorkOrder!)
                                    .ThenInclude(wo => wo.Car!)
                                    .ThenInclude(c => c.Customer!)
                                    .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if(invoice is null)
          return ApplicationErrors.InvoiceNotFound;

        return invoice.ToDto();
    }
}