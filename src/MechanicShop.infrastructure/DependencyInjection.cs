using MechanicShop.infrastructure.Data;
using MechanicShop.infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MechanicShop.infrastructure.BackgroundJobs;
using MechanicShop.infrastructure.Identity.Policies;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.infrastructure.Services;
using MechanicShop.infrastructure.RealTime;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MechanicShop.infrastructure.Email;
using MechanicShop.infrastructure.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddHttpContextAccessor();
        services.AddScoped<IUser, CurrentUserService>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        ArgumentNullException.ThrowIfNullOrEmpty(connectionString);

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();


        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection("JwtSettings");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                       Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
            };

            // Browsers can't set headers on WebSocket/SSE requests, so SignalR sends the token in the query string.
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    if (!string.IsNullOrEmpty(accessToken) &&
                        context.HttpContext.Request.Path.StartsWithSegments(WorkOrderHub.HubURl))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        services
        .AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredUniqueChars = 1;
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Tokens.EmailConfirmationTokenProvider = EmailConfirmationTokenProviderOptions.ProviderName;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders()
        .AddTokenProvider<EmailConfirmationTokenProvider<AppUser>>(EmailConfirmationTokenProviderOptions.ProviderName);

        // Without persisted keys every restart or redeploy would invalidate outstanding email links.
        services.AddDataProtection()
            .SetApplicationName("MechanicShop")
            .PersistKeysToDbContext<AppDbContext>();

        services.AddScoped<IAuthorizationHandler, LaborAssignedHandler>();

        services.AddAuthorizationBuilder()
              .AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"))
              .AddPolicy("StaffOnly", policy => policy.RequireRole("Manager", "Mechanic", "Receptionist"))
              .AddPolicy("SelfScopedWorkOrderAccess", policy =>
                policy.Requirements.Add(new LaborAssignedRequirement()));
        
        services.AddTransient<IIdentityService, IdentityService>();
         
        services.AddHybridCache(options => options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(10), // L2, L3
            LocalCacheExpiration = TimeSpan.FromSeconds(30), // L1
        });
        services.AddScoped<IWorkOrderPolicy, WorkOrderPolicy>();

        services.AddScoped<ITokenProvider, TokenProvider>();

        services.AddScoped<IInvoicePdfGenerator, InvoicePdfGenerator>();

        services.AddScoped<INotificationService, NotificationService>();

        services.AddSingleton<IAppLinkBuilder, AppLinkBuilder>();

        services.AddSignalR();
        services.AddScoped<IWorkOrderNotifier, SignalRWorkOrderNotifier>();

        services.AddHostedService<OverdueBookingCleanupService>();

        services.AddEmail(configuration);

        return services;
    }

    public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SmtpSettings>()
            .Bind(configuration.GetSection(SmtpSettings.SectionName))
            .Validate<IHostEnvironment>(
                (settings, env) => !env.IsProduction()
                    || (!string.IsNullOrWhiteSpace(settings.Host) && !string.IsNullOrWhiteSpace(settings.FromAddress)),
                "Smtp:Host and Smtp:FromAddress must be configured in Production.")
            .Validate(
                settings => string.IsNullOrWhiteSpace(settings.Host) || !string.IsNullOrWhiteSpace(settings.FromAddress),
                "Smtp:FromAddress is required when Smtp:Host is set.")
            .ValidateOnStart();

        services.AddSingleton<EmailQueue>();
        services.AddSingleton<IEmailSender, QueuedEmailSender>();

        services.AddSingleton<IEmailTransport>(sp =>
            string.IsNullOrWhiteSpace(sp.GetRequiredService<IOptions<SmtpSettings>>().Value.Host)
                ? ActivatorUtilities.CreateInstance<LoggingEmailTransport>(sp)
                : ActivatorUtilities.CreateInstance<SmtpEmailTransport>(sp));

        services.AddHostedService<EmailDispatcherService>();

        return services;
    }
}