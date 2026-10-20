using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.AppointmentRequests.Events;

public sealed class AppointmentRescheduled : DomainEvent
{
    public Guid AppointmentRequestId { get; set; }
}
