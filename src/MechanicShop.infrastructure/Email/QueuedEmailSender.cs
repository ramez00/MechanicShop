using System.Net.Mail;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MechanicShop.infrastructure.Email;

public sealed class QueuedEmailSender(EmailQueue queue, ILogger<QueuedEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        // Same rule as the domain's email validation; MimeKit alone accepts addresses without a domain.
        if (!MailAddress.TryCreate(to, out _))
        {
            logger.LogWarning("Email to invalid address '{To}' was dropped (subject: {Subject}).", to, subject);
            return;
        }

        await queue.EnqueueAsync(new EmailMessage(to, subject, htmlBody), cancellationToken);
    }
}
