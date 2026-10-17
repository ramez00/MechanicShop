using MechanicShop.infrastructure.Email;
using MechanicShop.infrastructure.Settings;
using MechanicShop.Tests.Common.Fakes;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MechanicShop.Application.UnitTests.Common.Email;

public sealed class EmailDispatcherServiceTests : IAsyncLifetime
{
    private readonly FakeEmailTransport _transport = new();
    private readonly EmailQueue _queue;
    private readonly QueuedEmailSender _sender;
    private readonly EmailDispatcherService _dispatcher;

    public EmailDispatcherServiceTests()
    {
        var options = Options.Create(new SmtpSettings { MaxSendAttempts = 3, RetryBaseDelayMilliseconds = 0 });

        _queue = new EmailQueue(options);
        _sender = new QueuedEmailSender(_queue, NullLogger<QueuedEmailSender>.Instance);
        _dispatcher = new EmailDispatcherService(_queue, _transport, options, NullLogger<EmailDispatcherService>.Instance);
    }

    public Task InitializeAsync() => _dispatcher.StartAsync(CancellationToken.None);

    public Task DisposeAsync() => _dispatcher.StopAsync(CancellationToken.None);

    [Fact]
    public async Task Queued_email_is_delivered()
    {
        await _sender.SendAsync("customer@test.local", "Hello", "<p>Body</p>");

        var sent = await _transport.WaitForAsync(m => m.To == "customer@test.local");

        Assert.Equal("Hello", sent.Subject);
        Assert.Equal("<p>Body</p>", sent.HtmlBody);
    }

    [Fact]
    public async Task Transient_failures_are_retried_until_success()
    {
        _transport.FailNextAttempts(2);

        await _sender.SendAsync("retry@test.local", "Retry", "<p>x</p>");

        await _transport.WaitForAsync(m => m.To == "retry@test.local");
        Assert.Equal(3, _transport.Attempts);
    }

    [Fact]
    public async Task Gives_up_after_max_attempts_and_keeps_processing_next_messages()
    {
        _transport.FailNextAttempts(3);

        await _sender.SendAsync("lost@test.local", "Lost", "<p>x</p>");
        await _sender.SendAsync("next@test.local", "Next", "<p>x</p>");

        await _transport.WaitForAsync(m => m.To == "next@test.local");

        Assert.DoesNotContain(_transport.Sent, m => m.To == "lost@test.local");
        Assert.Equal(4, _transport.Attempts);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public async Task Invalid_recipient_is_dropped_without_throwing(string to)
    {
        await _sender.SendAsync(to, "Hello", "<p>x</p>");
        await _sender.SendAsync("valid@test.local", "Hello", "<p>x</p>");

        await _transport.WaitForAsync(m => m.To == "valid@test.local");

        Assert.Single(_transport.Sent);
        Assert.Equal(1, _transport.Attempts);
    }
}
