using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Appointments.Commands.SubmitAppointmentRequest;
using MechanicShop.Application.Features.Scheduling.Queries.GetAvailability;
using MechanicShop.Contracts.Requests.Appointments;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/me")]
[ApiVersion("1.0")]
[Authorize]
public sealed class MeController(ISender sender, IUser currentUser) : ApiController
{
    private Guid CustomerId => currentUser.CustomerId!.Value;
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

    /// <summary>
    /// Submits a new appointment request for the authenticated customer.
    /// </summary>
    [HttpPost("appointments")]
    [EndpointName("SubmitAppointmentRequest")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = "CustomerOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitAppointmentRequest(
        [FromBody] SubmitAppointmentRequestRequest request,
        CancellationToken ct)
    {
        var command = new SubmitAppointmentRequestCommand(
            CustomerId,
            request.CarId,
            request.RepairTaskIds,
            request.RequestedStartAt,
            request.Notes);

        var result = await sender.Send(command, ct);

        return result.Match(
            id => CreatedAtRoute("GetAppointmentRequest", new { appointmentRequestId = id }, id),
            Problem);
    }
}
