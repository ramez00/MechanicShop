using System.Security.Claims;
using MechanicShop.Application.Features.Identity.Commands.RegisterCustomer;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Contracts.Requests.Identity;
using Microsoft.AspNetCore.RateLimiting;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using MechanicShop.Application.Features.Identity.Queries.GetUserInfo;
using MechanicShop.Application.Features.Identity.Queries.RefreshTokens;

namespace MechanicShop.Api.Controllers;

[Route("identity")]
[ApiVersionNeutral]
public sealed class IdentityController(ISender sender) : ApiController
{
    [HttpPost("register")]
    [EnableRateLimiting(RateLimitPolicies.Registration)]
    [ProducesResponseType<RegisteredCustomerDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCustomerCommand(
            request.UserName,
            request.Password,
            request.ConfirmPassword,
            request.Email,
            request.FullName,
            request.PhoneNumber);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            registered => StatusCode(StatusCodes.Status201Created, registered),
            Problem);
    }

   [HttpPost("token/generate")]
    public async Task<IActionResult> GenerateToken([FromBody] GenerateTokenQuery request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken);

        return result.Match(
            token => Ok(token),
            errors => Problem(errors)
        );
    }

    [HttpPost("token/refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenQuery request, CancellationToken ct)
    {
        var result = await sender.Send(request, ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("current-user/claims")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserInfo(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await sender.Send(new GetUserByIdQuery(userId!), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

}