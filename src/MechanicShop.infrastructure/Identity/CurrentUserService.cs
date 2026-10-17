using System.Security.Claims;
using MechanicShop.Application.Common.Identity;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MechanicShop.infrastructure.Identity;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string? Id => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public Guid? CustomerId =>
        Guid.TryParse(httpContextAccessor.HttpContext?.User?.FindFirstValue(AppClaimTypes.CustomerId), out var customerId)
            ? customerId
            : null;
}
