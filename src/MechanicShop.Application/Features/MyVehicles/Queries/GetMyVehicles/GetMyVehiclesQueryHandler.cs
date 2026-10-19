using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.MyVehicles.Queries.GetMyVehicles;

public class GetMyVehiclesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetMyVehiclesQuery, Result<List<CarDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<List<CarDto>>> Handle(GetMyVehiclesQuery query, CancellationToken ct)
    {
        var cars = await _context.Vehicles
            .AsNoTracking()
            .Where(c => c.CustomerId == query.CustomerId)
            .OrderBy(c => c.Make).ThenBy(c => c.Model)
            .ToListAsync(ct);

        return cars.ToDtoList();
    }
}
