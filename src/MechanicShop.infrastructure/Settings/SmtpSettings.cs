using MailKit.Security;

namespace MechanicShop.infrastructure.Settings;

public class SmtpSettings
{
    public const string SectionName = "Smtp";

    // Empty Host outside Production means "don't send, just log" (see LoggingEmailTransport).
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public SecureSocketOptions Security { get; set; } = SecureSocketOptions.Auto;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? FromAddress { get; set; }
    public string FromName { get; set; } = "MechanicShop";
    public int MaxSendAttempts { get; set; } = 3;
    public int RetryBaseDelayMilliseconds { get; set; } = 1000;
    public int QueueCapacity { get; set; } = 1000;
}
