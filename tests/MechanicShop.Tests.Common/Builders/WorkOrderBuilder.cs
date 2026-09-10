using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.workOrders.Enums;
using MechanicShop.Tests.Common.Constants;

namespace MechanicShop.Tests.Common.Builders;

public sealed class WorkOrderBuilder
{
    private Guid _id = TestConstants.WorkOrders.Id;
    private Guid _carId = TestConstants.Cars.Id;
    private DateTimeOffset _startAt = TestConstants.UtcNow;
    private DateTimeOffset _endAt = TestConstants.UtcNow.AddMinutes(30);
    private Guid _laborId = TestConstants.Employees.Id;
    private Spot _spot = Spot.A;
    private WorkOrderState _state = WorkOrderState.Scheduled;
    private List<RepairTask> _repairTasks = [new RepairTaskBuilder().Build()];

    public WorkOrderBuilder WithId(Guid id) { _id = id; return this; }
    public WorkOrderBuilder WithCarId(Guid carId) { _carId = carId; return this; }
    public WorkOrderBuilder WithStartAt(DateTimeOffset startAt) { _startAt = startAt; return this; }
    public WorkOrderBuilder WithEndAt(DateTimeOffset endAt) { _endAt = endAt; return this; }
    public WorkOrderBuilder WithLaborId(Guid laborId) { _laborId = laborId; return this; }
    public WorkOrderBuilder WithSpot(Spot spot) { _spot = spot; return this; }
    public WorkOrderBuilder WithState(WorkOrderState state) { _state = state; return this; }
    public WorkOrderBuilder WithRepairTasks(params RepairTask[] tasks) { _repairTasks = [.. tasks]; return this; }
    public WorkOrderBuilder WithNoRepairTasks() { _repairTasks = []; return this; }

    public WorkOrder Build()
    {
        var result = WorkOrder.Create(_id, _carId, _startAt, _endAt, _laborId, _spot, _state, _repairTasks);
        if (result.IsError)
            throw new InvalidOperationException($"WorkOrderBuilder produced an invalid WorkOrder: {result.TopError.Code}");
        return result.Value;
    }
}
