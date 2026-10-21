using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Appointments.Commands.ExpireAppointmentRequest;

public sealed record ExpireAppointmentRequestCommand(
    Guid Id
) : IRequest<Result<Updated>>;
