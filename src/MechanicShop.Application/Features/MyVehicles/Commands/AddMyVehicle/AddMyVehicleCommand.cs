using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.MyVehicles.Commands.AddMyVehicle;

// CustomerId always comes from the caller's token, never from the request body.
public sealed record AddMyVehicleCommand(
    Guid CustomerId,
    string Make,
    string Model,
    int Year,
    string LicensePlate) : IRequest<Result<CarDto>>;
