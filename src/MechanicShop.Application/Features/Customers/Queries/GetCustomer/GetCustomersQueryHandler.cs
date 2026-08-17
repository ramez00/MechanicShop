using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.customers.Mappers;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomer;

public class GetCustomersQueryHandler(IAppDbContext context) : IRequestHandler<GetCustomersQuery, Result<List<CustomerDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Customers.Include(c => c.Cars)
                                        .AsNoTracking()
                                        .Select(c => c.ToDto())
                                        .ToListAsync(cancellationToken);
    }
}

