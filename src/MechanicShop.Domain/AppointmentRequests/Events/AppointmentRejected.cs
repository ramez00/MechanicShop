using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.AppointmentRequests.Events;

public sealed class AppointmentRejected : DomainEvent
{
    public Guid AppointmentRequestId { get; set; }
}
