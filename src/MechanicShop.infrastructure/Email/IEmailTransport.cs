namespace MechanicShop.infrastructure.Email;

// Delivers one message; throws on failure so the dispatcher can retry.
public interface IEmailTransport
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
