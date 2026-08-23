using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Enums;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrder;

public sealed record UpdateWorkOrderStateCommand(
    Guid WorkOrderId,
    WorkOrderState WorkOrderState
) : IRequest<Result<Updated>>;