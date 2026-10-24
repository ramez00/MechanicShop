using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.AppointmentRequests.Events;

public sealed class AppointmentRequested : DomainEvent
{
    public Guid AppointmentRequestId { get; set; }
}
