using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.Update;

public sealed record UpdateCarCommand(
 Guid? CarId,
 string Make,
 string Model,
 int Year,
 string LicensePlate
 
) : IRequest<Result<Updated>>;