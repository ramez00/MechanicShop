using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.customers.Mappers;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdHandler(IAppDbContext context , ILogger<GetCustomerByIdHandler> logger) 
    : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
 {

    private readonly IAppDbContext _context = context;

    private readonly ILogger<GetCustomerByIdHandler> _logger =logger;

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
                                    .AsNoTracking()
                                    .Include(c => c.Cars)
                                    .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if(customer is null)
        {
            _logger.LogWarning($"can not find Customer with ID {request.Id}");
            return CustomerErrors.CustomerNotFound(request.Id);
        }

        return customer.ToDto();
    }
}