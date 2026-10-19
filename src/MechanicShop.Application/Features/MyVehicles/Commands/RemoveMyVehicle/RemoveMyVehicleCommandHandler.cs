using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.MyVehicles.Commands.RemoveMyVehicle;

public class RemoveMyVehicleCommandHandler(
    IAppDbContext context,
    HybridCache cache,
    ILogger<RemoveMyVehicleCommandHandler> logger)
    : IRequestHandler<RemoveMyVehicleCommand, Result<Deleted>>
{
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<RemoveMyVehicleCommandHandler> _logger = logger;

    public async Task<Result<Deleted>> Handle(RemoveMyVehicleCommand command, CancellationToken ct)
    {
        var customer = await _context.Customers
            .Include(c => c.Cars)
            .FirstOrDefaultAsync(c => c.Id == command.CustomerId, ct);

        // Another customer's car is reported as missing, so ids can't be probed.
        if (customer is null || customer.Cars.All(c => c.Id != command.VehicleId))
            return ApplicationErrors.VehicleNotFound;

        var workOrderStates = await _context.WorkOrders
            .Where(w => w.CarId == command.VehicleId)
            .Select(w => w.State)
            .ToListAsync(ct);

        // TODO(SPEC-10): also block on Pending/Approved appointment requests once they exist.
        if (workOrderStates.Any(s => s is WorkOrderState.Scheduled or WorkOrderState.InProgress))
            return CarErrors.HasActiveBookings;

        // Deleting a car cascades to its work orders, which would erase the shop's service records.
        if (workOrderStates.Count > 0)
            return CarErrors.HasServiceHistory;

        var removeResult = customer.RemoveCar(command.VehicleId);

        if (removeResult.IsError)
            return removeResult.Errors!;

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(MyVehicleCacheTags.Affected, ct);

        _logger.LogInformation("Customer {CustomerId} removed vehicle {VehicleId}.", command.CustomerId, command.VehicleId);

        return Result.deleted;
    }
}
