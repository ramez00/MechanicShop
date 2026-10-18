using MechanicShop.Application.Common.Email;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Commands.RegisterCustomer;

public class RegisterCustomerCommandHandler(
    IAppDbContext context,
    IIdentityService identityService,
    IEmailSender emailSender,
    IAppLinkBuilder linkBuilder,
    ILogger<RegisterCustomerCommandHandler> logger)
    : IRequestHandler<RegisterCustomerCommand, Result<RegisteredCustomerDto>>
{
    private readonly IAppDbContext _context = context;
    private readonly IIdentityService _identityService = identityService;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IAppLinkBuilder _linkBuilder = linkBuilder;
    private readonly ILogger<RegisterCustomerCommandHandler> _logger = logger;

    public async Task<Result<RegisteredCustomerDto>> Handle(RegisterCustomerCommand command, CancellationToken ct)
    {
        var userName = command.UserName.Trim();
        var email = command.Email.Trim();
        var fullName = command.FullName.Trim();

        // UserManager writes through the same DbContext, so the user and the customer
        // are committed together or not at all. Returning early disposes (rolls back) the transaction.
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        var createUserResult = await _identityService.CreateUserAsync(userName, email, command.Password, RoleNames.Customer);

        if (createUserResult.IsError)
        {
            _logger.LogWarning("Customer registration rejected for username {UserName}: {Error}", userName, createUserResult.TopError.Code);
            return createUserResult.Errors!;
        }

        var userId = createUserResult.Value;

        var customerResult = Customer.CreateSelfRegistered(Guid.NewGuid(), userId, fullName, email, command.PhoneNumber.Trim());

        if (customerResult.IsError)
        {
            _logger.LogWarning("Customer registration failed creating customer for user {UserId}: {Error}", userId, customerResult.TopError.Code);
            return customerResult.Errors!;
        }

        _context.Customers.Add(customerResult.Value);

        await _context.SaveChangesAsync(ct);

        var token = await _identityService.GenerateEmailConfirmationTokenAsync(userId);

        await transaction.CommitAsync(ct);

        var content = EmailTemplates.ConfirmEmail(fullName, _linkBuilder.EmailConfirmationLink(userId, token));

        await _emailSender.SendAsync(email, content.Subject, content.HtmlBody, ct);

        _logger.LogInformation("Customer registered: user {UserId}, customer {CustomerId}.", userId, customerResult.Value.Id);

        return new RegisteredCustomerDto(userId);
    }
}
