using MechanicShop.Application.Common.Email;

namespace MechanicShop.Application.UnitTests.Common.Email;

public class EmailTemplatesTests
{
    private const string Name = "Ramez Test";
    private const string Vehicle = "2022 Toyota Corolla (ABC123)";
    private static readonly DateTimeOffset StartAt = new(2026, 10, 20, 10, 30, 0, TimeSpan.FromHours(3));
    private static readonly string[] Services = ["Oil change", "Brake inspection"];

    public static TheoryData<string, EmailContent> AllTemplates => new()
    {
        { nameof(EmailTemplates.ConfirmEmail), EmailTemplates.ConfirmEmail(Name, "https://shop.test/portal/confirm-email?userId=1&token=abc") },
        { nameof(EmailTemplates.AppointmentRequestReceived), EmailTemplates.AppointmentRequestReceived(Name, StartAt, Vehicle, Services) },
        { nameof(EmailTemplates.AppointmentApproved), EmailTemplates.AppointmentApproved(Name, StartAt, Vehicle, Services) },
        { nameof(EmailTemplates.AppointmentRejected), EmailTemplates.AppointmentRejected(Name, StartAt, Vehicle, "No parts in stock") },
        { nameof(EmailTemplates.AppointmentExpired), EmailTemplates.AppointmentExpired(Name, StartAt, Vehicle) },
        { nameof(EmailTemplates.AppointmentCancelled), EmailTemplates.AppointmentCancelled(Name, StartAt, Vehicle) },
        { nameof(EmailTemplates.AppointmentRescheduled), EmailTemplates.AppointmentRescheduled(Name, StartAt, Vehicle) },
    };

    [Theory]
    [MemberData(nameof(AllTemplates))]
    public void Every_template_has_subject_and_greets_customer(string template, EmailContent content)
    {
        Assert.False(string.IsNullOrWhiteSpace(content.Subject), $"{template} has no subject.");
        Assert.Contains($"Hi {Name}", content.HtmlBody);
    }

    [Fact]
    public void ConfirmEmail_contains_link()
    {
        var content = EmailTemplates.ConfirmEmail(Name, "https://shop.test/confirm?userId=1&token=abc");

        // '&' is HTML-encoded inside the href, which browsers decode back.
        Assert.Contains("href=\"https://shop.test/confirm?userId=1&amp;token=abc\"", content.HtmlBody);
    }

    [Fact]
    public void Appointment_templates_contain_date_vehicle_and_services()
    {
        var content = EmailTemplates.AppointmentApproved(Name, StartAt, Vehicle, Services);

        Assert.Contains("Tuesday, October 20, 2026 at 10:30", content.HtmlBody);
        Assert.Contains(Vehicle, content.HtmlBody);
        Assert.Contains("Oil change", content.HtmlBody);
        Assert.Contains("Brake inspection", content.HtmlBody);
    }

    [Fact]
    public void Rejected_template_contains_reason()
    {
        var content = EmailTemplates.AppointmentRejected(Name, StartAt, Vehicle, "No parts in stock");

        Assert.Contains("No parts in stock", content.HtmlBody);
    }

    [Fact]
    public void Templates_html_encode_user_values()
    {
        const string script = "<script>alert(1)</script>";

        var content = EmailTemplates.AppointmentRejected(script, StartAt, script, script);

        Assert.DoesNotContain(script, content.HtmlBody);
        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", content.HtmlBody);
    }

    [Fact]
    public void ConfirmEmail_encodes_quotes_in_link()
    {
        var content = EmailTemplates.ConfirmEmail(Name, "https://x.test/\"onmouseover=\"alert(1)");

        Assert.DoesNotContain("\"onmouseover=\"", content.HtmlBody);
    }
}
