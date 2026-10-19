using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.MyVehicles.Commands.RemoveMyVehicle;

public sealed record RemoveMyVehicleCommand(Guid CustomerId, Guid VehicleId) : IRequest<Result<Deleted>>;
