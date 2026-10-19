using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.MyVehicles.Queries.GetMyVehicleById;

public class GetMyVehicleByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetMyVehicleByIdQuery, Result<CarDto>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<CarDto>> Handle(GetMyVehicleByIdQuery query, CancellationToken ct)
    {
        var car = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.VehicleId && c.CustomerId == query.CustomerId, ct);

        // Another customer's car is reported as missing, so ids can't be probed.
        if (car is null)
            return ApplicationErrors.VehicleNotFound;

        return car.ToDto();
    }
}
