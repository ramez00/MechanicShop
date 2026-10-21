using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Appointments.Commands.SubmitAppointmentRequest;

public sealed record SubmitAppointmentRequestCommand(
    Guid CustomerId,
    Guid CarId,
    List<Guid> RepairTaskIds,
    DateTimeOffset RequestedStartAt,
    string? Notes) : IRequest<Result<Guid>>;
