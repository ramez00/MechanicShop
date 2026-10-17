namespace MechanicShop.Application.Common.Interfaces;

public interface IEmailSender
{
    // Queues the email and returns immediately; delivery and retries happen in the background,
    // so a mail-server outage never fails the request that triggered the email.
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
