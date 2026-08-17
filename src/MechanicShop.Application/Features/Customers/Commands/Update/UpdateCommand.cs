using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.Update;

public sealed record UpdateCommand(
    Guid CustomerId,
    string Name,
    string PhoneNumber,
    string Email,
    List<UpdateCarCommand> Cars

) : IRequest<Result<Updated>>;