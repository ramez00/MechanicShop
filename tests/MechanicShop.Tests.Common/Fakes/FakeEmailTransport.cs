using System.Collections.Concurrent;
using MechanicShop.infrastructure.Email;

namespace MechanicShop.Tests.Common.Fakes;

// Captures delivered emails instead of talking to an SMTP server.
public sealed class FakeEmailTransport : IEmailTransport
{
    private readonly ConcurrentQueue<EmailMessage> _sent = new();
    private int _failNextAttempts;
    private int _attempts;

    public IReadOnlyCollection<EmailMessage> Sent => _sent.ToArray();

    public int Attempts => _attempts;

    // The next N send attempts throw, simulating an SMTP outage.
    public void FailNextAttempts(int count) => Interlocked.Exchange(ref _failNextAttempts, count);

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _attempts);

        if (Interlocked.Decrement(ref _failNextAttempts) >= 0)
            throw new InvalidOperationException("Simulated SMTP failure.");

        _sent.Enqueue(message);
        return Task.CompletedTask;
    }

    public async Task<EmailMessage> WaitForAsync(Func<EmailMessage, bool> predicate, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(5));

        while (DateTime.UtcNow < deadline)
        {
            var match = _sent.FirstOrDefault(predicate);

            if (match is not null)
                return match;

            await Task.Delay(20);
        }

        throw new TimeoutException("Expected email was not delivered in time.");
    }
}
