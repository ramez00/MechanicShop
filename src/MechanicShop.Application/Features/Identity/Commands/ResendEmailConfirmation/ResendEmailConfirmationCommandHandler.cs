using MechanicShop.Application.Common.Email;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Commands.ResendEmailConfirmation;

// Always succeeds, so callers can't use this endpoint to find out which emails are registered or confirmed.
public class ResendEmailConfirmationCommandHandler(
    IAppDbContext context,
    IIdentityService identityService,
    IEmailSender emailSender,
    IAppLinkBuilder linkBuilder,
    ILogger<ResendEmailConfirmationCommandHandler> logger)
    : IRequestHandler<ResendEmailConfirmationCommand, Result<Success>>
{
    private readonly IAppDbContext _context = context;
    private readonly IIdentityService _identityService = identityService;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IAppLinkBuilder _linkBuilder = linkBuilder;
    private readonly ILogger<ResendEmailConfirmationCommandHandler> _logger = logger;

    public async Task<Result<Success>> Handle(ResendEmailConfirmationCommand command, CancellationToken ct)
    {
        var email = command.Email.Trim();

        var userId = await _identityService.FindUnconfirmedUserIdByEmailAsync(email);

        if (userId is null)
        {
            _logger.LogInformation("Confirmation resend skipped: no unconfirmed account for the given email.");
            return Result.success;
        }

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        var token = await _identityService.GenerateEmailConfirmationTokenAsync(userId);

        var content = EmailTemplates.ConfirmEmail(customer?.Name ?? email, _linkBuilder.EmailConfirmationLink(userId, token));

        await _emailSender.SendAsync(email, content.Subject, content.HtmlBody, ct);

        _logger.LogInformation("Confirmation email re-sent for user {UserId}.", userId);

        return Result.success;
    }
}
