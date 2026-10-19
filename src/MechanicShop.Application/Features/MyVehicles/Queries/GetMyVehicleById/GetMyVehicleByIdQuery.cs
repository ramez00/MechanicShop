using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.MyVehicles.Queries.GetMyVehicleById;

public sealed record GetMyVehicleByIdQuery(Guid CustomerId, Guid VehicleId) : IRequest<Result<CarDto>>;
