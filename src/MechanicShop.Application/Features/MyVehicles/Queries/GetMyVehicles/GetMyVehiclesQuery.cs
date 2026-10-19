using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.MyVehicles.Queries.GetMyVehicles;

public sealed record GetMyVehiclesQuery(Guid CustomerId) : IRequest<Result<List<CarDto>>>;
