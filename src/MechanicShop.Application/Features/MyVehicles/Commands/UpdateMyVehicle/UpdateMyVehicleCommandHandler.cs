using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Cars;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.MyVehicles.Commands.UpdateMyVehicle;

public class UpdateMyVehicleCommandHandler(
    IAppDbContext context,
    HybridCache cache,
    ILogger<UpdateMyVehicleCommandHandler> logger)
    : IRequestHandler<UpdateMyVehicleCommand, Result<CarDto>>
{
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<UpdateMyVehicleCommandHandler> _logger = logger;

    public async Task<Result<CarDto>> Handle(UpdateMyVehicleCommand command, CancellationToken ct)
    {
        var car = await _context.Vehicles
            .FirstOrDefaultAsync(c => c.Id == command.VehicleId && c.CustomerId == command.CustomerId, ct);

        // Another customer's car is reported as missing, so ids can't be probed.
        if (car is null)
            return ApplicationErrors.VehicleNotFound;

        if (await _context.Vehicles.Where(c => c.Id != car.Id).IsLicensePlateTakenAsync(command.LicensePlate, ct))
            return CarErrors.PlateTaken;

        var updateResult = car.Update(command.Make.Trim(), command.Model.Trim(), command.Year, command.LicensePlate);

        if (updateResult.IsError)
            return updateResult.Errors!;

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(MyVehicleCacheTags.Affected, ct);

        _logger.LogInformation("Customer {CustomerId} updated vehicle {VehicleId}.", command.CustomerId, car.Id);

        return car.ToDto();
    }
}
