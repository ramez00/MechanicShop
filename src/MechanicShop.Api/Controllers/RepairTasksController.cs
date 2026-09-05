namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/repair-tasks")]
[ApiVersion("1.0")]
[Authorize]
public sealed class RepairTasksController(ISender sender) : ApiController
{
    [HttpGet]
    [EndpointName("GetRepairTasks")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetRepairTasksQuery(), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("{repairTaskId:guid}", Name = nameof(GetById))]
    [EndpointName("GetRepairTaskById")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60)]
    public async Task<IActionResult> GetById(Guid repairTaskId, CancellationToken ct)
    {
        var result = await sender.Send(new GetRepairTaskByIdQuery(repairTaskId), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Manager))]
    [EndpointName("CreateRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create(CreateRepairTaskCommand request, CancellationToken ct)
    {

       var parts = request.Parts.ConvertAll(part => new CreateRepairTaskPartCommand(part.Name, part.Cost, part.Quantity));

       var command = new CreateRepairTaskCommand(
            request.Name,
            request.LaborCost,
            request.EstimatedDurationInMins is not null ? (RepairDurationInMinutes)request.EstimatedDurationInMins : null,
            parts);

        var result = await sender.Send(command, ct);

        return result.Match(
            response => CreatedAtAction(nameof(GetById), new { repairTaskId = response.RepairTaskId }, response),
            Problem);
    }

    [HttpPut("{repairTaskId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [EndpointName("UpdateRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update(Guid repairTaskId, UpdateRepairTaskCommand request, CancellationToken ct)
    {
        var parts = request.Parts.ConvertAll(part => new UpdateRepairTaskPartCommand(part.PartId, part.Name, part.Cost, part.Quantity));

        var command = new UpdateRepairTaskCommand(
            repairTaskId,
            request.Name,
            request.LaborCost,
            (RepairDurationInMinutes)request.EstimatedDurationInMins,
            parts);

        var result = await sender.Send(command, ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpDelete("{repairTaskId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [EndpointName("RemoveRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Remove(Guid repairTaskId, CancellationToken ct)
    {
        var result = await sender.Send(new RemoveRepairTaskCommand(repairTaskId), ct);

        return result.Match(
            response => NoContent(),
            Problem);
    }

}