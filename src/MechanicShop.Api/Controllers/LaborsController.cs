namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/labors")]
[ApiVersion("1.0")]
[Authorize]
public sealed class LaborsController(ISender sender) : ApiController
{

    [HttpGet]
    [EndpointName("GetLabors")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetLaborsQuery(), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }
}