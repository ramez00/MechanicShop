using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Enums;
using MediatR;

namespace MechanicShop.Application.Features.Appointments.Commands.ApproveAppointmentRequest;

public sealed record ApproveAppointmentRequestCommand(
    Guid Id,
    Guid LaborId,
    Spot Spot,
    string ReviewedBy
) : IRequest<Result<Guid>>;
