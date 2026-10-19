using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.MyVehicles.Commands.UpdateMyVehicle;

public sealed record UpdateMyVehicleCommand(
    Guid CustomerId,
    Guid VehicleId,
    string Make,
    string Model,
    int Year,
    string LicensePlate) : IRequest<Result<CarDto>>;
