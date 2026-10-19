using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Cars;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.MyVehicles.Commands.AddMyVehicle;

public class AddMyVehicleCommandHandler(
    IAppDbContext context,
    HybridCache cache,
    ILogger<AddMyVehicleCommandHandler> logger)
    : IRequestHandler<AddMyVehicleCommand, Result<CarDto>>
{
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<AddMyVehicleCommandHandler> _logger = logger;

    public async Task<Result<CarDto>> Handle(AddMyVehicleCommand command, CancellationToken ct)
    {
        var customer = await _context.Customers
            .Include(c => c.Cars)
            .FirstOrDefaultAsync(c => c.Id == command.CustomerId, ct);

        if (customer is null)
            return ApplicationErrors.CustomerNotFound;

        if (await _context.Vehicles.IsLicensePlateTakenAsync(command.LicensePlate, ct))
            return CarErrors.PlateTaken;

        var carResult = Car.Create(Guid.NewGuid(), command.Make.Trim(), command.Model.Trim(), command.Year, command.LicensePlate);

        if (carResult.IsError)
            return carResult.Errors!;

        customer.AddCar(carResult.Value);

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(MyVehicleCacheTags.Affected, ct);

        _logger.LogInformation("Customer {CustomerId} added vehicle {VehicleId}.", customer.Id, carResult.Value.Id);

        return carResult.Value.ToDto();
    }
}
