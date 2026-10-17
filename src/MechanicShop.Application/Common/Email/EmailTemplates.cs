using System.Globalization;
using System.Net;
using System.Text;

namespace MechanicShop.Application.Common.Email;

public sealed record EmailContent(string Subject, string HtmlBody);

// Every value passed in is HTML-encoded. Times are shown exactly as given, so callers
// must convert them to the shop's time zone first.
public static class EmailTemplates
{
    private const string ShopName = "MechanicShop";
    private const string DateFormat = "dddd, MMMM d, yyyy 'at' HH:mm";

    public static EmailContent ConfirmEmail(string fullName, string confirmationLink)
        => new(
            $"Confirm your {ShopName} account",
            Layout(
                fullName,
                "<p>Thanks for registering. Please confirm your email address to activate your account.</p>" +
                $"<p><a href=\"{Encode(confirmationLink)}\">Confirm my email</a></p>" +
                "<p>This link expires in 24 hours. If you didn't create an account, you can ignore this email.</p>"));

    public static EmailContent AppointmentRequestReceived(string fullName, DateTimeOffset startAt, string vehicle, IReadOnlyCollection<string> services)
        => new(
            "We received your appointment request",
            Layout(
                fullName,
                "<p>We received your request. The shop will confirm it shortly.</p>" +
                AppointmentDetails(startAt, vehicle, services)));

    public static EmailContent AppointmentApproved(string fullName, DateTimeOffset startAt, string vehicle, IReadOnlyCollection<string> services)
        => new(
            "Your appointment is confirmed",
            Layout(
                fullName,
                "<p>Your appointment is confirmed. Please arrive on time — late arrivals may have their booking cancelled.</p>" +
                AppointmentDetails(startAt, vehicle, services)));

    public static EmailContent AppointmentRejected(string fullName, DateTimeOffset startAt, string vehicle, string reason)
        => new(
            "We couldn't confirm your appointment",
            Layout(
                fullName,
                $"<p>Unfortunately we couldn't confirm your appointment on {Encode(FormatDate(startAt))} for {Encode(vehicle)}.</p>" +
                $"<p><strong>Reason:</strong> {Encode(reason)}</p>" +
                "<p>You're welcome to book another time.</p>"));

    public static EmailContent AppointmentExpired(string fullName, DateTimeOffset startAt, string vehicle)
        => new(
            "Your appointment request expired",
            Layout(
                fullName,
                $"<p>Your request for {Encode(FormatDate(startAt))} ({Encode(vehicle)}) wasn't confirmed in time and has expired.</p>" +
                "<p>Please book another time or call the shop.</p>"));

    public static EmailContent AppointmentCancelled(string fullName, DateTimeOffset startAt, string vehicle)
        => new(
            "Your appointment was cancelled",
            Layout(
                fullName,
                $"<p>Your appointment on {Encode(FormatDate(startAt))} for {Encode(vehicle)} has been cancelled.</p>"));

    public static EmailContent AppointmentRescheduled(string fullName, DateTimeOffset newStartAt, string vehicle)
        => new(
            "Your new appointment time is awaiting confirmation",
            Layout(
                fullName,
                $"<p>You moved your appointment for {Encode(vehicle)} to {Encode(FormatDate(newStartAt))}.</p>" +
                "<p>The new time needs to be confirmed by the shop. We'll email you once it is.</p>"));

    private static string AppointmentDetails(DateTimeOffset startAt, string vehicle, IReadOnlyCollection<string> services)
    {
        var html = new StringBuilder();

        html.Append("<table cellpadding=\"4\">");
        html.Append($"<tr><td><strong>When</strong></td><td>{Encode(FormatDate(startAt))}</td></tr>");
        html.Append($"<tr><td><strong>Vehicle</strong></td><td>{Encode(vehicle)}</td></tr>");
        html.Append($"<tr><td><strong>Services</strong></td><td>{string.Join("<br>", services.Select(Encode))}</td></tr>");
        html.Append("</table>");

        return html.ToString();
    }

    private static string Layout(string fullName, string content)
        => "<!DOCTYPE html><html><body style=\"font-family:Arial,sans-serif;color:#222;\">" +
           $"<p>Hi {Encode(fullName)},</p>" +
           content +
           $"<p>— The {ShopName} team</p>" +
           "</body></html>";

    private static string FormatDate(DateTimeOffset value) => value.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
