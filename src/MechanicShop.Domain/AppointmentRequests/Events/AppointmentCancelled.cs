using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.AppointmentRequests.Events;

public sealed class AppointmentCancelled : DomainEvent
{
    public Guid AppointmentRequestId { get; set; }
}
