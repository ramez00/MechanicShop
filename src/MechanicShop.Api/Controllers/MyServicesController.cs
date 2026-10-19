using MechanicShop.Application.Features.BookableServices.Dtos;
using MechanicShop.Application.Features.BookableServices.Queries.GetBookableServices;

namespace MechanicShop.Api.Controllers;

// The services a signed-in customer can book.
[Route("api/v{version:apiVersion}/me/services")]
[ApiVersion("1.0")]
[Authorize(Policy = "CustomerOnly")]
public sealed class MyServicesController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType<List<BookableServiceDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookableServicesQuery(), cancellationToken);

        return result.Match(Ok, Problem);
    }
}
