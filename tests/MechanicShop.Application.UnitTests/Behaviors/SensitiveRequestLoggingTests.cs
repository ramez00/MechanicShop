using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Commands.RegisterCustomer;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using MechanicShop.Application.Features.Identity.Queries.RefreshTokens;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.UnitTests.Common.Behaviors;

public class SensitiveRequestLoggingTests
{
    private const string Password = "Sup3rSecret!";

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<(string Message, object? RequestValue)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var requestValue = state is IEnumerable<KeyValuePair<string, object?>> values
                ? values.FirstOrDefault(v => v.Key == "@Request").Value
                : null;

            Entries.Add((formatter(state, exception), requestValue));
        }
    }

    public sealed record PlainRequest(string Value) : IRequest<Result<string>>;

    private static RegisterCustomerCommand SensitiveCommand() =>
        new("ramez01", Password, Password, "ramez@test.local", "Ramez Test", "+201000000000");

    [Fact]
    public void Payload_redacts_sensitive_requests()
    {
        Assert.Equal(RequestLogging.Redacted, RequestLogging.Payload(SensitiveCommand()));
    }

    [Fact]
    public void Payload_keeps_ordinary_requests()
    {
        var request = new PlainRequest("visible");

        Assert.Same(request, RequestLogging.Payload(request));
    }

    [Fact]
    public void Login_and_refresh_queries_are_marked_sensitive()
    {
        Assert.True(typeof(ISensitiveRequest).IsAssignableFrom(typeof(GenerateTokenQuery)));
        Assert.True(typeof(ISensitiveRequest).IsAssignableFrom(typeof(RefreshTokenQuery)));
    }

    [Fact]
    public async Task Unhandled_exception_log_does_not_contain_the_password()
    {
        var logger = new CapturingLogger<RegisterCustomerCommand>();
        var sut = new UnhandledExceptionBehaviour<RegisterCustomerCommand, Result<RegisteredCustomerDto>>(logger);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.Handle(SensitiveCommand(), _ => throw new InvalidOperationException("boom"), CancellationToken.None));

        var entry = Assert.Single(logger.Entries);
        Assert.DoesNotContain(Password, entry.Message);
        Assert.Equal(RequestLogging.Redacted, entry.RequestValue);
    }
}
