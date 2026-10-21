using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Appointments.Commands.ApproveAppointmentRequest;
using MechanicShop.Application.Features.Appointments.Commands.ExpireAppointmentRequest;
using MechanicShop.Application.Features.Appointments.Commands.RejectAppointmentRequest;
using MechanicShop.Application.Features.Appointments.Queries.GetAppointmentRequests;
using MechanicShop.Contracts.Requests.Appointments;
using MechanicShop.Domain.AppointmentRequests.Enums;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/appointments")]
[ApiVersion("1.0")]
[Authorize(Policy = "StaffOnly")]
public sealed class AppointmentsController(ISender sender, IUser currentUser) : ApiController
{
    [HttpGet]
    [EndpointName("GetAppointmentRequests")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Get(
        [FromQuery] AppointmentRequestStatus? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page <= 0)
            return BadRequest("Page must be greater than 0.");

        if (pageSize <= 0 || pageSize > 100)
            return BadRequest("PageSize must be between 1 and 100.");

        var query = new GetAppointmentRequestsQuery(status, from, to, page, pageSize);

        var result = await sender.Send(query, ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpPut("{id:guid}/approve")]
    [EndpointName("ApproveAppointmentRequest")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveAppointmentRequestRequest request,
        CancellationToken ct)
    {
        var command = new ApproveAppointmentRequestCommand(
            id,
            request.LaborId,
            (Spot)(int)request.Spot,
            currentUser.Id!);

        var result = await sender.Send(command, ct);

        return result.Match(
            workOrderId => Ok(new { workOrderId }),
            Problem);
    }

    [HttpPut("{id:guid}/reject")]
    [EndpointName("RejectAppointmentRequest")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectAppointmentRequestRequest request,
        CancellationToken ct)
    {
        var command = new RejectAppointmentRequestCommand(
            id,
            currentUser.Id!,
            request.Reason);

        var result = await sender.Send(command, ct);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPut("{id:guid}/expire")]
    [EndpointName("ExpireAppointmentRequest")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Expire(Guid id, CancellationToken ct)
    {
        var command = new ExpireAppointmentRequestCommand(id);

        var result = await sender.Send(command, ct);

        return result.Match(_ => NoContent(), Problem);
    }
}
