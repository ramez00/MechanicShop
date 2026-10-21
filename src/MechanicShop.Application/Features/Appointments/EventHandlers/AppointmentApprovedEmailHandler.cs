using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.AppointmentRequests.Events;
using MediatR;

namespace MechanicShop.Application.Features.Appointments.EventHandlers;

public sealed class AppointmentApprovedEmailHandler(
    IAppDbContext context,
    INotificationService notification
) : INotificationHandler<AppointmentApproved>
{
    private readonly IAppDbContext _context = context;
    private readonly INotificationService _notify = notification;

    public async Task Handle(AppointmentApproved domainEvent, CancellationToken cancellationToken)
    {
        var appointmentRequest = await _context.AppointmentRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(ar => ar.Id == domainEvent.AppointmentRequestId, cancellationToken);

        if (appointmentRequest is null)
            return;

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == appointmentRequest.CustomerId, cancellationToken);

        if (customer is null)
            return;

        await _notify.SendEmailAsync(customer.Email!, cancellationToken);
    }
}
