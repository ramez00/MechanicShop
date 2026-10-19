using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandler(
    IIdentityService identityService,
    ILogger<ConfirmEmailCommandHandler> logger)
    : IRequestHandler<ConfirmEmailCommand, Result<Success>>
{
    private readonly IIdentityService _identityService = identityService;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger = logger;

    public async Task<Result<Success>> Handle(ConfirmEmailCommand command, CancellationToken ct)
    {
        var result = await _identityService.ConfirmEmailAsync(command.UserId.Trim(), command.Token.Trim());

        if (result.IsError)
        {
            _logger.LogWarning("Email confirmation rejected for user {UserId}.", command.UserId);
            return result.Errors!;
        }

        _logger.LogInformation("Email confirmed for user {UserId}.", command.UserId);

        return Result.success;
    }
}
