using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.Remove;

public sealed record RemoveCustomerCommand(Guid customerId) : IRequest<Result<Deleted>>;