using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Common.Email;
using MechanicShop.Application.Common.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Email;

[Collection(WebAppFactoryCollection.CollectionName)]
public class EmailDeliveryTests(WebAppFactory webAppFactory)
{
    [Fact]
    public async Task Email_sent_through_IEmailSender_reaches_the_transport()
    {
        var to = $"delivery.{Guid.NewGuid():N}@test.local";
        var content = EmailTemplates.ConfirmEmail("Ramez Test", "https://shop.test/confirm");

        var sender = webAppFactory.Services.GetRequiredService<IEmailSender>();
        await sender.SendAsync(to, content.Subject, content.HtmlBody);

        var sent = await webAppFactory.EmailTransport.WaitForAsync(m => m.To == to);

        Assert.Equal(content.Subject, sent.Subject);
        Assert.Equal(content.HtmlBody, sent.HtmlBody);
    }

    [Fact]
    public async Task Smtp_outage_is_retried_and_does_not_throw_to_the_caller()
    {
        var to = $"outage.{Guid.NewGuid():N}@test.local";
        webAppFactory.EmailTransport.FailNextAttempts(2);

        var sender = webAppFactory.Services.GetRequiredService<IEmailSender>();
        await sender.SendAsync(to, "Retry", "<p>x</p>");

        await webAppFactory.EmailTransport.WaitForAsync(m => m.To == to);
    }
}
