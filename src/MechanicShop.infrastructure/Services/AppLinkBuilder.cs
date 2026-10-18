using MechanicShop.Application.Common.Interfaces;
using MechanicShop.infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace MechanicShop.infrastructure.Services;

public class AppLinkBuilder(IOptions<AppSettings> options) : IAppLinkBuilder
{
    private readonly string _clientBaseUrl = options.Value.ClientBaseUrl.TrimEnd('/');

    public string EmailConfirmationLink(string userId, string token)
        => $"{_clientBaseUrl}/portal/confirm-email?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
}
