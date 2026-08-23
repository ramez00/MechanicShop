using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.workOrders.Events;

public sealed class WorkOrderCompleted : DomainEvent
{
    public Guid WorkOrderId { get; set; }
}