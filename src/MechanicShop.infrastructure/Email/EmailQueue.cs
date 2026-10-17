using System.Threading.Channels;
using MechanicShop.infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace MechanicShop.infrastructure.Email;

// In-memory hand-off between request threads (writers) and EmailDispatcherService (single reader).
// Messages still queued when the process stops are lost; acceptable for notification emails.
public sealed class EmailQueue(IOptions<SmtpSettings> options)
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(
        new BoundedChannelOptions(options.Value.QueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true
        });

    public ChannelReader<EmailMessage> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken)
        => _channel.Writer.WriteAsync(message, cancellationToken);
}
