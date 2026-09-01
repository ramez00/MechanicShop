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

        // services.AddAuthentication(options =>
        // {
        //     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        // }).AddJwtBearer(options =>
        // {
        //     var jwtSettings = configuration.GetSection("JwtSettings");

        //     options.TokenValidationParameters = new TokenValidationParameters
        //     {
        //         ValidateIssuer = true,
        //         ValidateAudience = true,
        //         ValidateLifetime = true,
        //         ClockSkew = TimeSpan.Zero,
        //         ValidateIssuerSigningKey = true,
        //         ValidIssuer = jwtSettings["Issuer"],
        //         ValidAudience = jwtSettings["Audience"],
        //         IssuerSigningKey = new SymmetricSecurityKey(
        //                Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
        //     };
        // });

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
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IAuthorizationHandler, LaborAssignedHandler>();

        services.AddAuthorizationBuilder()
              .AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"))
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

        services.AddSignalR();
        services.AddScoped<IWorkOrderNotifier, SignalRWorkOrderNotifier>();

        services.AddHostedService<OverdueBookingCleanupService>();
        
        return services;
    }
}