using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.AppointmentRequests.Events;

public sealed class AppointmentExpired : DomainEvent
{
    public Guid AppointmentRequestId { get; set; }
}
