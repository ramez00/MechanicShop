using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MechanicShop.infrastructure.Services;

public class NotificationService(
    ILogger<NotificationService> logger
) : INotificationService
{
    private readonly ILogger<NotificationService> _logger = logger;
    private const string Message = "Your Car service is complete. You may collect it from the shop at your earliest convenience.";


    public async Task SendEmailAsync(string to, CancellationToken cancellationToken = default)
    {
         _logger.LogInformation("[Email] To: {Email} | Message: {Message}", to, Message);

        // Simulated email send
        await Task.CompletedTask;
    }

    public async Task SendSmsAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[SMS] To: {Phone} | Message: {Message}", phoneNumber, Message);

        // Simulated SMS send
        await Task.CompletedTask;
    }
}