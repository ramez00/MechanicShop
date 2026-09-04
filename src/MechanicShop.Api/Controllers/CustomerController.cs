using MechanicShop.Application.Features.Customers.Commands.Remove;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/customers")]
[ApiVersion("1.0")]
[Authorize]
public sealed class CustomerController(ISender sender) : ApiController
{
    [HttpGet("GetCustomers")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomersQuery(), cancellationToken);

        return result.Match(
            customers => Ok(customers),
            errors => Problem(errors)
        );
    }

    [HttpGet("{customerId:guid}", Name = "GetCustomerById")]
    public async Task<IActionResult> GetById(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(customerId), cancellationToken);

        return result.Match(
            customer => Ok(customer),
            errors => Problem(errors)
        );
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]

    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var cars = command.cars.ConvertAll(
            c => new CreateCarCommand(c.Make, c.Model, c.Year, c.LicensePlate));

        var result = await sender.Send(new CreateCustomerCommand(command.Name,command.PhoneNumber,command.Email,cars), cancellationToken);

        return result.Match(
            response => CreatedAtRoute("GetCustomerById", new { customerId = response.Id }, null),
            errors => Problem(errors)
        );
    }

    [HttpPut("{customerId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    public async Task<IActionResult> Update(Guid customerId, [FromBody] UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var cars = command.Cars.ConvertAll(
            c => new UpdateCarCommand(c.CarId, c.Make, c.Model, c.Year, c.LicensePlate));

        var result = await sender.Send(new UpdateCustomerCommand(customerId, command.Name, command.PhoneNumber, command.Email,cars), cancellationToken);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpDelete("{customerId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    public async Task<IActionResult> Delete(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveCustomerCommand(customerId), cancellationToken);

        return result.Match(
            response => NoContent(),
            Problem);
    }
}
