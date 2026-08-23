using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.workOrders.Events;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers;

public sealed class WorkOrderCollectionModifiedEventHandler(
    IWorkOrderNotifier notifier
) 
: INotificationHandler<WorkOrderCollectionModified>
{
    private readonly IWorkOrderNotifier _notifier = notifier;
    public Task Handle(WorkOrderCollectionModified notification, CancellationToken cancellationToken)
      =>  _notifier.NotifyWorkOrdersChangedAsync(cancellationToken);
    
}