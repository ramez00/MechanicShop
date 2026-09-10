using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Domain.UnitTests.WorkOrders;

public class WorkOrderTests
{
    private static readonly Guid CarId = TestConstants.Cars.Id;
    private static readonly Guid LaborId = TestConstants.Employees.Id;
    private static readonly DateTimeOffset Start = TestConstants.UtcNow;
    private static readonly DateTimeOffset End = TestConstants.UtcNow.AddMinutes(30);

    private static List<RepairTask> Tasks() => [new RepairTaskBuilder().Build()];

    // ---------- Create ----------

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = WorkOrder.Create(
            TestConstants.WorkOrders.Id, CarId, Start, End, LaborId,
            Spot.A, WorkOrderState.Scheduled, Tasks());

        var workOrder = result.ShouldBeSuccess();
        Assert.Equal(CarId, workOrder.CarId);
        Assert.Equal(LaborId, workOrder.LaborId);
        Assert.Equal(Spot.A, workOrder.Spot);
        Assert.Equal(WorkOrderState.Scheduled, workOrder.State);
        Assert.Single(workOrder.RepairTasks);
    }

    [Fact]
    public void Create_WithEmptyId_ReturnsWorkOrderIdRequired()
    {
        var result = WorkOrder.Create(
            Guid.Empty, CarId, Start, End, LaborId, Spot.A, WorkOrderState.Scheduled, Tasks());

        result.ShouldBeError(WorkOrderErrors.WorkOrderIdRequired);
    }

    [Fact]
    public void Create_WithEmptyCarId_ReturnsCarIdRequired()
    {
        var result = WorkOrder.Create(
            TestConstants.WorkOrders.Id, Guid.Empty, Start, End, LaborId, Spot.A, WorkOrderState.Scheduled, Tasks());

        result.ShouldBeError(WorkOrderErrors.CarIdRequired);
    }

    [Fact]
    public void Create_WithNoRepairTasks_ReturnsRepairTasksRequired()
    {
        var result = WorkOrder.Create(
            TestConstants.WorkOrders.Id, CarId, Start, End, LaborId, Spot.A, WorkOrderState.Scheduled, []);

        result.ShouldBeError(WorkOrderErrors.RepairTasksRequired);
    }

    [Fact]
    public void Create_WithEmptyLaborId_ReturnsLaborIdRequired()
    {
        var result = WorkOrder.Create(
            TestConstants.WorkOrders.Id, CarId, Start, End, Guid.Empty, Spot.A, WorkOrderState.Scheduled, Tasks());

        result.ShouldBeError(WorkOrderErrors.LaborIdRequired);
    }

    [Fact]
    public void Create_WhenEndNotAfterStart_ReturnsInvalidTiming()
    {
        var result = WorkOrder.Create(
            TestConstants.WorkOrders.Id, CarId, Start, Start, LaborId, Spot.A, WorkOrderState.Scheduled, Tasks());

        result.ShouldBeError(WorkOrderErrors.InvalidTiming);
    }

    [Fact]
    public void Create_WithUndefinedSpot_ReturnsSpotInvalid()
    {
        var result = WorkOrder.Create(
            TestConstants.WorkOrders.Id, CarId, Start, End, LaborId, (Spot)99, WorkOrderState.Scheduled, Tasks());

        result.ShouldBeError(WorkOrderErrors.SpotInvalid);
    }

    // ---------- IsEditable ----------

    [Theory]
    [InlineData(WorkOrderState.Scheduled, true)]
    [InlineData(WorkOrderState.InProgress, false)]
    [InlineData(WorkOrderState.Completed, false)]
    [InlineData(WorkOrderState.Cancelled, false)]
    public void IsEditable_ReflectsState(WorkOrderState state, bool expected)
    {
        var workOrder = new WorkOrderBuilder().WithState(state).Build();

        Assert.Equal(expected, workOrder.IsEditable);
    }

    // ---------- AddRepairTask ----------

    [Fact]
    public void AddRepairTask_WhenEditable_AddsTask()
    {
        var workOrder = new WorkOrderBuilder().Build();
        var newTask = new RepairTaskBuilder().WithId(Guid.NewGuid()).Build();

        var result = workOrder.AddRepairTask(newTask);

        result.ShouldBeSuccess();
        Assert.Equal(2, workOrder.RepairTasks.Count());
    }

    [Fact]
    public void AddRepairTask_WhenNotEditable_ReturnsReadonly()
    {
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Completed).Build();
        var newTask = new RepairTaskBuilder().WithId(Guid.NewGuid()).Build();

        var result = workOrder.AddRepairTask(newTask);

        result.ShouldBeError(WorkOrderErrors.Readonly);
    }

    [Fact]
    public void AddRepairTask_WhenDuplicate_ReturnsRepairTaskAlreadyAdded()
    {
        var task = new RepairTaskBuilder().WithId(Guid.NewGuid()).Build();
        var workOrder = new WorkOrderBuilder().WithRepairTasks(task).Build();

        var result = workOrder.AddRepairTask(task);

        result.ShouldBeError(WorkOrderErrors.RepairTaskAlreadyAdded);
    }

    // ---------- UpdateTiming ----------

    [Fact]
    public void UpdateTiming_WhenEditable_UpdatesTimes()
    {
        var workOrder = new WorkOrderBuilder().Build();
        var newStart = Start.AddHours(1);
        var newEnd = End.AddHours(2);

        var result = workOrder.UpdateTiming(newStart, newEnd);

        result.ShouldBeSuccess();
        Assert.Equal(newStart, workOrder.StartAtUtc);
        Assert.Equal(newEnd, workOrder.EndAtUtc);
    }

    [Fact]
    public void UpdateTiming_WhenNotEditable_ReturnsReadonly()
    {
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.InProgress).Build();

        var result = workOrder.UpdateTiming(Start, End);

        result.ShouldBeError(WorkOrderErrors.Readonly);
    }

    [Fact]
    public void UpdateTiming_WhenEndNotAfterStart_ReturnsInvalidTiming()
    {
        var workOrder = new WorkOrderBuilder().Build();

        var result = workOrder.UpdateTiming(End, Start);

        result.ShouldBeError(WorkOrderErrors.InvalidTiming);
    }

    // ---------- UpdateLabor ----------

    [Fact]
    public void UpdateLabor_WhenEditable_UpdatesLabor()
    {
        var workOrder = new WorkOrderBuilder().Build();
        var newLabor = Guid.NewGuid();

        var result = workOrder.UpdateLabor(newLabor);

        result.ShouldBeSuccess();
        Assert.Equal(newLabor, workOrder.LaborId);
    }

    [Fact]
    public void UpdateLabor_WhenNotEditable_ReturnsReadonly()
    {
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Completed).Build();

        var result = workOrder.UpdateLabor(Guid.NewGuid());

        result.ShouldBeError(WorkOrderErrors.Readonly);
    }

    [Fact]
    public void UpdateLabor_WithEmptyLaborId_ReturnsLaborIdEmpty()
    {
        var workOrder = new WorkOrderBuilder().Build();

        var result = workOrder.UpdateLabor(Guid.Empty);

        result.ShouldBeErrorWithCode("WorkOrderErrors.LaborIdEmpty");
    }

    // ---------- CanTransitionTo ----------

    [Theory]
    [InlineData(WorkOrderState.Scheduled, WorkOrderState.InProgress, true)]
    [InlineData(WorkOrderState.InProgress, WorkOrderState.Completed, true)]
    [InlineData(WorkOrderState.Scheduled, WorkOrderState.Cancelled, true)]
    [InlineData(WorkOrderState.InProgress, WorkOrderState.Cancelled, true)]
    [InlineData(WorkOrderState.Completed, WorkOrderState.Cancelled, false)]
    [InlineData(WorkOrderState.Scheduled, WorkOrderState.Completed, false)]
    [InlineData(WorkOrderState.Scheduled, WorkOrderState.Scheduled, false)]
    [InlineData(WorkOrderState.InProgress, WorkOrderState.Scheduled, false)]
    public void CanTransitionTo_ReturnsExpected(WorkOrderState from, WorkOrderState to, bool expected)
    {
        var workOrder = new WorkOrderBuilder().WithState(from).Build();

        Assert.Equal(expected, workOrder.CanTransitionTo(to));
    }

    // ---------- UpdateState ----------

    [Fact]
    public void UpdateState_WithValidTransition_UpdatesState()
    {
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Scheduled).Build();

        var result = workOrder.UpdateState(WorkOrderState.InProgress);

        result.ShouldBeSuccess();
        Assert.Equal(WorkOrderState.InProgress, workOrder.State);
    }

    [Fact]
    public void UpdateState_WhenNotEditable_ReturnsReadonly()
    {
        // InProgress is not editable, so the readonly guard fires before the transition check.
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.InProgress).Build();

        var result = workOrder.UpdateState(WorkOrderState.Completed);

        result.ShouldBeError(WorkOrderErrors.Readonly);
    }

    [Fact]
    public void UpdateState_WithInvalidTransition_ReturnsInvalidStateTransition()
    {
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Scheduled).Build();

        var result = workOrder.UpdateState(WorkOrderState.Completed);

        result.ShouldBeErrorWithCode("WorkOrderErrors.InvalidStateTransition");
    }

    // ---------- Cancel ----------

    [Fact]
    public void Cancel_FromScheduled_Succeeds()
    {
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Scheduled).Build();

        var result = workOrder.Cancel();

        result.ShouldBeSuccess();
        Assert.Equal(WorkOrderState.Cancelled, workOrder.State);
    }

    [Fact]
    public void Cancel_FromCompleted_ReturnsInvalidStateTransition()
    {
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Completed).Build();

        var result = workOrder.Cancel();

        result.ShouldBeErrorWithCode("WorkOrderErrors.InvalidStateTransition");
    }

    // ---------- ClearRepairTasks ----------

    [Fact]
    public void ClearRepairTasks_WhenEditable_EmptiesList()
    {
        var workOrder = new WorkOrderBuilder().Build();

        var result = workOrder.ClearRepairTasks(tax: null);

        result.ShouldBeSuccess();
        Assert.Empty(workOrder.RepairTasks);
    }

    [Fact]
    public void ClearRepairTasks_WhenNotEditable_ReturnsReadonly()
    {
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Completed).Build();

        var result = workOrder.ClearRepairTasks(tax: null);

        result.ShouldBeError(WorkOrderErrors.Readonly);
    }

    // ---------- UpdateSpot ----------

    [Fact]
    public void UpdateSpot_WhenEditable_UpdatesSpot()
    {
        var workOrder = new WorkOrderBuilder().WithSpot(Spot.A).Build();

        var result = workOrder.UpdateSpot(Spot.C);

        result.ShouldBeSuccess();
        Assert.Equal(Spot.C, workOrder.Spot);
    }

    [Fact]
    public void UpdateSpot_WithUndefinedSpot_ReturnsSpotInvalid()
    {
        var workOrder = new WorkOrderBuilder().Build();

        var result = workOrder.UpdateSpot((Spot)99);

        result.ShouldBeError(WorkOrderErrors.SpotInvalid);
    }

    [Fact]
    public void UpdateSpot_WhenNotEditable_ReturnsReadonly()
    {
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Completed).Build();

        var result = workOrder.UpdateSpot(Spot.B);

        result.ShouldBeError(WorkOrderErrors.Readonly);
    }

    // ---------- UpdateDiscount / UpdateTax ----------

    [Fact]
    public void UpdateDiscount_WhenEditable_SetsDiscount()
    {
        var workOrder = new WorkOrderBuilder().Build();

        var result = workOrder.UpdateDiscount(15m);

        result.ShouldBeSuccess();
        Assert.Equal(15m, workOrder.Discount);
    }

    [Fact]
    public void UpdateDiscount_WhenNotEditable_ReturnsReadonly()
    {
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Completed).Build();

        var result = workOrder.UpdateDiscount(15m);

        result.ShouldBeError(WorkOrderErrors.Readonly);
    }

    [Fact]
    public void UpdateTax_WhenEditable_SetsTax()
    {
        var workOrder = new WorkOrderBuilder().Build();

        var result = workOrder.UpdateTax(8m);

        result.ShouldBeSuccess();
        Assert.Equal(8m, workOrder.Tax);
    }

    [Fact]
    public void UpdateTax_WhenNotEditable_ReturnsReadonly()
    {
        var workOrder = new WorkOrderBuilder().WithState(WorkOrderState.Cancelled).Build();

        var result = workOrder.UpdateTax(8m);

        result.ShouldBeError(WorkOrderErrors.Readonly);
    }

    // ---------- Computed totals ----------

    [Fact]
    public void Totals_AggregatePartsAndLaborAcrossRepairTasks()
    {
        var part1 = new PartBuilder().WithId(Guid.NewGuid()).WithPrice(50m).WithQuantity(1).Build();
        var part2 = new PartBuilder().WithId(Guid.NewGuid()).WithPrice(30m).WithQuantity(1).Build();
        var task1 = new RepairTaskBuilder().WithId(Guid.NewGuid()).WithLaborCost(100m).WithParts(part1).Build();
        var task2 = new RepairTaskBuilder().WithId(Guid.NewGuid()).WithLaborCost(200m).WithParts(part2).Build();

        var workOrder = new WorkOrderBuilder().WithRepairTasks(task1, task2).Build();

        // TotalPartsCost sums Part.Price (not qty * price).
        Assert.Equal(80m, workOrder.TotalPartsCost);
        Assert.Equal(300m, workOrder.TotalLaborCost);
        Assert.Equal(380m, workOrder.Total);
    }
}
