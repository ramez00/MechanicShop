using MailKit.Net.Smtp;
using MechanicShop.infrastructure.Settings;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MechanicShop.infrastructure.Email;

public sealed class SmtpEmailTransport(IOptions<SmtpSettings> options) : IEmailTransport
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        // Host and FromAddress are non-empty here: this transport is only registered when Host is set,
        // and options validation requires FromAddress whenever Host is set.
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress!));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody }.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(_settings.Host!, _settings.Port, _settings.Security, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_settings.UserName))
            await client.AuthenticateAsync(_settings.UserName, _settings.Password ?? string.Empty, cancellationToken);

        await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }
}
