using MechanicShop.Application.Features.Scheduling.Queries.GetAvailability;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/me")]
[ApiVersion("1.0")]
[Authorize]
public sealed class MeController(ISender sender) : ApiController
{
    /// <summary>
    /// Returns available booking slots for the given date and repair services.
    /// </summary>
    [HttpGet("availability")]
    [EndpointName("GetAvailability")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetAvailability(
        [FromQuery] DateOnly date,
        [FromQuery] List<Guid> repairTaskIds,
        [FromHeader(Name = "X-TimeZone")] string? tz,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tz))
            return Problem(detail: "Missing time zone in 'X-TimeZone' header.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Time Zone Required");

        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(tz);
        }
        catch
        {
            return Problem(detail: $"Invalid or unknown time zone: '{tz}'.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Time Zone");
        }

        var result = await sender.Send(
            new GetAvailabilityQuery(date, repairTaskIds, timeZone), ct);

        return result.Match(Ok, Problem);
    }
}
