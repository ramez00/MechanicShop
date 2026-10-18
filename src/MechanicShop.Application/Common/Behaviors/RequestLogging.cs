using MechanicShop.Application.Common.Interfaces;

namespace MechanicShop.Application.Common.Behaviors;

public static class RequestLogging
{
    public const string Redacted = "[redacted]";

    // What a behavior may write for {@Request}.
    public static object Payload(object request) => request is ISensitiveRequest ? Redacted : request;
}
