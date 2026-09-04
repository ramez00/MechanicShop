namespace MechanicShop.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/dashboard")]
public sealed class DashboardController(ISender sender) : ApiController
{
    [HttpGet("stats")]
    public async Task<IActionResult> GetTodayStats([FromQuery] DateOnly? date, CancellationToken ct)
    {
        var statsDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await sender.Send(new GetWorkOrderStatsQuery(statsDate), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }
}