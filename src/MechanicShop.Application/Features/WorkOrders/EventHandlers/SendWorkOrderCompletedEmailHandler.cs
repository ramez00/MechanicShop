using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.workOrders.Events;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers;

public sealed class SendWorkOrderCompletedEmailHandler(
IAppDbContext context,
INotificationService notification
) : INotificationHandler<WorkOrderCompleted>
{
    private readonly IAppDbContext _context = context;
    private readonly INotificationService _notify = notification;
    public async Task Handle(WorkOrderCompleted notification, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
                                      .Include(w => w.Car)
                                      .ThenInclude(c => c.Customer)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync(w => w.Id == notification.WorkOrderId);

        if(workOrder is null)
            return;

        await _notify.SendEmailAsync(workOrder.Car!.Customer!.Email!,cancellationToken);

        await _notify.SendSmsAsync(workOrder.Car!.Customer!.PhoneNumber!,cancellationToken);                            
    }
}