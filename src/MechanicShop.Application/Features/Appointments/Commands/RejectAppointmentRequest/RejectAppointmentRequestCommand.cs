using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Appointments.Commands.RejectAppointmentRequest;

public sealed record RejectAppointmentRequestCommand(
    Guid Id,
    string ReviewedBy,
    string? Reason
) : IRequest<Result<Updated>>;
