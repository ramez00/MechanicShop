using MechanicShop.infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MechanicShop.Contracts.Responses;
using MechanicShop.infrastructure.Settings;

namespace MechanicShop.Api.Controllers;
[Route("api/[controller]")]
public sealed class SettingsController(
    IOptions<AppSettings> settingsService
) : ApiController
{
    private readonly AppSettings _appSettings = settingsService.Value;

    [HttpGet("GetOperatingHours")]
    public IActionResult GetOperatingHours(CancellationToken cancellationToken)
        => Ok(new OperatingHoursResponse(
            _appSettings.OpeningTime,
            _appSettings.ClosingTime
        ));
    
}