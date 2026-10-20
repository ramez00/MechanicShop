using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.AppointmentRequests.Events;

public sealed class AppointmentApproved : DomainEvent
{
    public Guid AppointmentRequestId { get; set; }
    public Guid WorkOrderId { get; set; }
}
