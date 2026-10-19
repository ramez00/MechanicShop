using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Identity.Commands.ResendEmailConfirmation;

public sealed record ResendEmailConfirmationCommand(string Email) : IRequest<Result<Success>>;
