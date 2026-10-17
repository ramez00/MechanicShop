using MechanicShop.infrastructure.Email;
using MechanicShop.infrastructure.Settings;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MechanicShop.Application.UnitTests.Common.Email;

public class EmailRegistrationTests
{
    private static ServiceProvider BuildProvider(string environment, Dictionary<string, string?> smtp)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(smtp.ToDictionary(kv => $"Smtp:{kv.Key}", kv => kv.Value))
            .Build();

        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(environment);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(env);
        services.AddEmail(configuration);

        return services.BuildServiceProvider();
    }

    [Fact]
    public void Production_without_host_fails_validation()
    {
        using var provider = BuildProvider(Environments.Production, new() { ["FromAddress"] = "no-reply@shop.test" });

        var ex = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<SmtpSettings>>().Value);

        Assert.Contains("Smtp:Host", ex.Message);
    }

    [Fact]
    public void Production_with_host_and_from_address_uses_smtp_transport()
    {
        using var provider = BuildProvider(Environments.Production, new()
        {
            ["Host"] = "smtp.shop.test",
            ["FromAddress"] = "no-reply@shop.test"
        });

        Assert.IsType<SmtpEmailTransport>(provider.GetRequiredService<IEmailTransport>());
    }

    [Fact]
    public void Development_without_host_falls_back_to_logging_transport()
    {
        using var provider = BuildProvider(Environments.Development, new());

        Assert.IsType<LoggingEmailTransport>(provider.GetRequiredService<IEmailTransport>());
    }

    [Fact]
    public void Host_without_from_address_fails_validation()
    {
        using var provider = BuildProvider(Environments.Development, new() { ["Host"] = "localhost" });

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<SmtpSettings>>().Value);
    }

    [Fact]
    public void Security_mode_binds_from_string()
    {
        using var provider = BuildProvider(Environments.Development, new()
        {
            ["Host"] = "localhost",
            ["FromAddress"] = "no-reply@shop.test",
            ["Security"] = "None"
        });

        Assert.Equal(MailKit.Security.SecureSocketOptions.None, provider.GetRequiredService<IOptions<SmtpSettings>>().Value.Security);
    }
}
