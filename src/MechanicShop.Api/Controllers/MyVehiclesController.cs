using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.MyVehicles.Commands.AddMyVehicle;
using MechanicShop.Application.Features.MyVehicles.Commands.RemoveMyVehicle;
using MechanicShop.Application.Features.MyVehicles.Commands.UpdateMyVehicle;
using MechanicShop.Application.Features.MyVehicles.Queries.GetMyVehicleById;
using MechanicShop.Application.Features.MyVehicles.Queries.GetMyVehicles;
using MechanicShop.Contracts.Requests.Customers;

namespace MechanicShop.Api.Controllers;

// The signed-in customer's own cars. The customer is always taken from the token's
// customer_id claim, never from the request, so one customer can't act on another's cars.
[Route("api/v{version:apiVersion}/me/vehicles")]
[ApiVersion("1.0")]
[Authorize(Policy = "CustomerOnly")]
public sealed class MyVehiclesController(ISender sender, IUser currentUser) : ApiController
{
    // Guaranteed by the CustomerOnly policy, which requires the customer_id claim.
    private Guid CustomerId => currentUser.CustomerId!.Value;

    [HttpGet]
    [ProducesResponseType<List<CarDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyVehiclesQuery(CustomerId), cancellationToken);

        return result.Match(Ok, Problem);
    }

    [HttpGet("{vehicleId:guid}", Name = "GetMyVehicleById")]
    [ProducesResponseType<CarDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid vehicleId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyVehicleByIdQuery(CustomerId, vehicleId), cancellationToken);

        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [ProducesResponseType<CarDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        var command = new AddMyVehicleCommand(CustomerId, request.Make, request.Model, request.Year, request.LicensePlate);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            car => CreatedAtRoute("GetMyVehicleById", new { vehicleId = car.Id }, car),
            Problem);
    }

    // The route id is authoritative; UpdateVehicleRequest.VehicleId is ignored.
    [HttpPut("{vehicleId:guid}")]
    [ProducesResponseType<CarDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid vehicleId, [FromBody] UpdateVehicleRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateMyVehicleCommand(CustomerId, vehicleId, request.Make, request.Model, request.Year, request.LicensePlate);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(Ok, Problem);
    }

    [HttpDelete("{vehicleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Remove(Guid vehicleId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveMyVehicleCommand(CustomerId, vehicleId), cancellationToken);

        return result.Match(_ => NoContent(), Problem);
    }
}
