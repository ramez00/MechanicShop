using Microsoft.Extensions.Logging;

namespace MechanicShop.infrastructure.Email;

// Used outside Production when no SMTP host is configured, so local runs work with zero setup.
public sealed class LoggingEmailTransport(ILogger<LoggingEmailTransport> logger) : IEmailTransport
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[Email not sent - no SMTP host configured] To: {To} | Subject: {Subject}{NewLine}{Body}",
            message.To, message.Subject, Environment.NewLine, message.HtmlBody);

        return Task.CompletedTask;
    }
}
