using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.AssignLabor;

public sealed record AssignLaborCommand(
   Guid WorkOrderId,
   Guid LaborId
) : IRequest<Result<Updated>>;