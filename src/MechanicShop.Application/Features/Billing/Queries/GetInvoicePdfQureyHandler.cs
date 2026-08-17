using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Billing.Queries;

public class GetInvoicePdfQureyHandler : IRequestHandler<GetInvoicePdfQuery, Result<InvoicePdfDto>>
{
    public Task<Result<InvoicePdfDto>> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}