using MechanicShop.infrastructure.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MechanicShop.infrastructure.Email;

public sealed class EmailDispatcherService(
    EmailQueue queue,
    IEmailTransport transport,
    IOptions<SmtpSettings> options,
    ILogger<EmailDispatcherService> logger) : BackgroundService
{
    private readonly SmtpSettings _settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var message in queue.Reader.ReadAllAsync(stoppingToken))
            {
                await SendWithRetryAsync(message, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is shutting down.
        }
    }

    private async Task SendWithRetryAsync(EmailMessage message, CancellationToken stoppingToken)
    {
        var maxAttempts = Math.Max(1, _settings.MaxSendAttempts);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await transport.SendAsync(message, stoppingToken);

                logger.LogInformation("Email sent to {To} (subject: {Subject}).", message.To, message.Subject);
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (attempt == maxAttempts)
                {
                    logger.LogError(ex, "Email send failed after {Attempts} attempts. To: {To}, Subject: {Subject}.",
                        attempt, message.To, message.Subject);
                    return;
                }

                logger.LogWarning(ex, "Email send attempt {Attempt}/{MaxAttempts} failed. To: {To}. Retrying.",
                    attempt, maxAttempts, message.To);

                // Exponential backoff: base, 2x base, 4x base, ...
                var delay = TimeSpan.FromMilliseconds(_settings.RetryBaseDelayMilliseconds * Math.Pow(2, attempt - 1));
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
