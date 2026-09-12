using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.EventHandlers;
using MechanicShop.Domain.workOrders.Enums;
using MechanicShop.Domain.workOrders.Events;

namespace MechanicShop.Application.UnitTests.Features.WorkOrders;

public class SendWorkOrderCompletedEmailHandlerTests : HandlerTestBase
{
    private readonly INotificationService _notification = Substitute.For<INotificationService>();

    private SendWorkOrderCompletedEmailHandler CreateSut() => new(Context, _notification);

    [Fact]
    public async Task Handle_WhenWorkOrderExists_SendsEmailAndSms()
    {
        var customer = new CustomerBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Completed).Build();
        await SeedAsync(customer, labor, workOrder);
        var sut = CreateSut();

        await sut.Handle(new WorkOrderCompleted { WorkOrderId = workOrder.Id }, CancellationToken.None);

        await _notification.Received(1).SendEmailAsync(TestConstants.Customers.Email, Arg.Any<CancellationToken>());
        await _notification.Received(1).SendSmsAsync(TestConstants.Customers.Phone, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWorkOrderMissing_DoesNothing()
    {
        var sut = CreateSut();

        await sut.Handle(new WorkOrderCompleted { WorkOrderId = Guid.NewGuid() }, CancellationToken.None);

        await _notification.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _notification.DidNotReceive().SendSmsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
