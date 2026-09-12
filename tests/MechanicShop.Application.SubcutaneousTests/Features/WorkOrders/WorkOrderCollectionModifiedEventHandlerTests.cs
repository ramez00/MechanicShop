using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.EventHandlers;
using MechanicShop.Domain.workOrders.Events;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class WorkOrderCollectionModifiedEventHandlerTests
{
    [Fact]
    public async Task Handle_NotifiesWorkOrdersChanged()
    {
        var notifier = Substitute.For<IWorkOrderNotifier>();
        var sut = new WorkOrderCollectionModifiedEventHandler(notifier);

        await sut.Handle(new WorkOrderCollectionModified(), CancellationToken.None);

        await notifier.Received(1).NotifyWorkOrdersChangedAsync(Arg.Any<CancellationToken>());
    }
}
