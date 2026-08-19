using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.cars;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.workOrders.Billing;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Domain.WorkOrders;

public sealed class WorkOrder : AuditableEntity
{
    public Guid CarId { get; }
    public DateTimeOffset StartAtUtc { get; private set; }
    public DateTimeOffset EndAtUtc { get; private set; }
    public Guid LaborId { get; private set; }
    public Spot Spot { get; private set; }
    public WorkOrderState State { get; private set; }
    public Employee? Labor { get; set; }
    public Car? Car { get; set; }
    public Invoice? Invoice { get; set; }
    public decimal? Discount { get; private set; }
    public decimal? Tax { get; private set; }
    
    private readonly List<RepairTask> _repairTasks = [];
    public IEnumerable<RepairTask> RepairTasks => _repairTasks.AsReadOnly();

    public decimal? TotalPartsCost => _repairTasks.SelectMany(rt => rt.Parts).Sum(p => p.Price);
    public decimal? TotalLaborCost => _repairTasks.Sum(rt => rt.LaborCost);
    public decimal? Total => (TotalPartsCost ?? 0) + (TotalLaborCost ?? 0);

    private WorkOrder() { } // For EF Core

    private WorkOrder(Guid id, Guid carId, DateTimeOffset startAt, DateTimeOffset endAt, Guid laborId,
         Spot spot, WorkOrderState state, List<RepairTask> repairTasks) : base(id)
    {
        CarId = carId;
        StartAtUtc = startAt;
        EndAtUtc = endAt;
        LaborId = laborId;
        Spot = spot;
        State = state;
        _repairTasks = repairTasks;
    }

    public static Result<WorkOrder> Create(Guid id, Guid carId, DateTimeOffset startAt, DateTimeOffset endAt, 
    Guid laborId, Spot spot, WorkOrderState state, List<RepairTask> repairTasks)
    {
        if(id == Guid.Empty)
            return WorkOrderErrors.WorkOrderIdRequired;

        if (carId == Guid.Empty)
            return WorkOrderErrors.CarIdRequired;

        if(repairTasks == null || !repairTasks.Any())
            return WorkOrderErrors.RepairTasksRequired;

        if(laborId == Guid.Empty)
            return WorkOrderErrors.LaborIdRequired;

        if (endAt <= startAt)
            return WorkOrderErrors.InvalidTiming;

        if(!Enum.IsDefined(spot))
            return WorkOrderErrors.SpotInvalid;

        return new WorkOrder(id, carId, startAt, endAt, laborId, spot, state, repairTasks);
    }

    public bool IsEditable 
        => State is not (WorkOrderState.Completed or WorkOrderState.Cancelled or WorkOrderState.InProgress);

    public Result<Updated> AddRepairTask(RepairTask repairTask)
    {
        if(!IsEditable)
            return WorkOrderErrors.Readonly;

        if (_repairTasks.Any(rt => rt.Id == repairTask.Id))
            return WorkOrderErrors.RepairTaskAlreadyAdded;

        _repairTasks.Add(repairTask);

        return Result.updated;
    }

    public Result<Updated> UpdateTiming(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if(!IsEditable)
            return WorkOrderErrors.Readonly;

        if (endAt <= startAt)
            return WorkOrderErrors.InvalidTiming;

        StartAtUtc = startAt;
        EndAtUtc = endAt;

        return Result.updated;
    }

    public Result<Updated> UpdateLabor(Guid laborId)
    {
        if(!IsEditable)
            return WorkOrderErrors.Readonly;

        if (laborId == Guid.Empty)
            return WorkOrderErrors.LaborIdEmpty(Id.ToString());

        LaborId = laborId;

        return Result.updated;
    }

    public bool CanTransitionTo(WorkOrderState newStatus)
    {
        return (State, newStatus) switch
        {
            (WorkOrderState.Scheduled, WorkOrderState.InProgress) => true,
            (WorkOrderState.InProgress, WorkOrderState.Completed) => true,
            (_, WorkOrderState.Cancelled) when State != WorkOrderState.Completed => true,
            _ => false
        };
    }
    
    public Result<Updated> UpdateState(WorkOrderState newState)
    {
        if(!IsEditable)
            return WorkOrderErrors.Readonly;

        if (!CanTransitionTo(newState))
            return WorkOrderErrors.InvalidStateTransition(State, newState);

        State = newState;

        return Result.updated;
    }

    public Result<Updated> Cancel()
    {
        if(!CanTransitionTo(WorkOrderState.Cancelled))
            return WorkOrderErrors.InvalidStateTransition(State, WorkOrderState.Cancelled);

        State = WorkOrderState.Cancelled;

        return Result.updated;
    }

    public Result<Updated> ClearRepairTasks(decimal? tax)
    {
        if(!IsEditable)
            return WorkOrderErrors.Readonly;

        _repairTasks.Clear();

        return Result.updated;
    }

    public Result<Updated> UpdateSpot(Spot NewSpot)
    {
        if(!IsEditable)
            return WorkOrderErrors.Readonly;

        if(!Enum.IsDefined(NewSpot))
            return WorkOrderErrors.SpotInvalid;

        Spot = NewSpot;

        return Result.updated;
    }
    
    public Result<Updated> UpdateDiscount(decimal? discount)
    {
        if(!IsEditable)
            return WorkOrderErrors.Readonly;

        Discount = discount;

        return Result.updated;
    }

    public Result<Updated> UpdateTax(decimal? tax)
    {
        if(!IsEditable)
            return WorkOrderErrors.Readonly;

        Tax = tax;

        return Result.updated;
    }
}