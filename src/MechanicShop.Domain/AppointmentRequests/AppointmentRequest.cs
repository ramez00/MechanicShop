using MechanicShop.Domain.AppointmentRequests.Enums;
using MechanicShop.Domain.AppointmentRequests.Events;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;

namespace MechanicShop.Domain.AppointmentRequests;

public sealed class AppointmentRequest : AuditableEntity
{
    public Guid CustomerId { get; private set; }
    public Guid CarId { get; private set; }
    public AppointmentRequestStatus Status { get; private set; }
    public DateTimeOffset RequestedStartAtUtc { get; private set; }
    public DateTimeOffset RequestedEndAtUtc { get; private set; }
    public string? Notes { get; private set; }
    public Guid? WorkOrderId { get; private set; }
    public string? ReviewedBy { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private readonly List<RepairTask> _repairTasks = [];
    public IEnumerable<RepairTask> RepairTasks => _repairTasks.AsReadOnly();

    private AppointmentRequest() { } // For EF Core

    private AppointmentRequest(
        Guid id,
        Guid customerId,
        Guid carId,
        DateTimeOffset requestedStartAtUtc,
        DateTimeOffset requestedEndAtUtc,
        string? notes,
        List<RepairTask> repairTasks) : base(id)
    {
        CustomerId = customerId;
        CarId = carId;
        Status = AppointmentRequestStatus.Pending;
        RequestedStartAtUtc = requestedStartAtUtc;
        RequestedEndAtUtc = requestedEndAtUtc;
        Notes = notes;
        _repairTasks = repairTasks;
    }

    public static Result<AppointmentRequest> Create(
        Guid id,
        Guid customerId,
        Guid carId,
        DateTimeOffset requestedStartAtUtc,
        DateTimeOffset requestedEndAtUtc,
        List<RepairTask> repairTasks,
        string? notes = null)
    {
        if (customerId == Guid.Empty)
            return AppointmentRequestErrors.CustomerIdRequired;

        if (carId == Guid.Empty)
            return AppointmentRequestErrors.CarIdRequired;

        if (repairTasks == null || repairTasks.Count == 0)
            return AppointmentRequestErrors.RepairTasksRequired;

        if (requestedEndAtUtc <= requestedStartAtUtc)
            return AppointmentRequestErrors.InvalidTiming;

        if (notes is not null && notes.Length > 500)
            return AppointmentRequestErrors.NotesTooLong;

        var request = new AppointmentRequest(id, customerId, carId, requestedStartAtUtc, requestedEndAtUtc, notes, repairTasks);

        request.AddDomainEvent(new AppointmentRequested { AppointmentRequestId = id });

        return request;
    }

    public Result<Updated> Approve(Guid workOrderId, string reviewedBy, DateTimeOffset reviewedAt)
    {
        if (Status != AppointmentRequestStatus.Pending)
            return AppointmentRequestErrors.NotPending;

        Status = AppointmentRequestStatus.Approved;
        WorkOrderId = workOrderId;
        ReviewedBy = reviewedBy;
        ReviewedAt = reviewedAt;

        AddDomainEvent(new AppointmentApproved { AppointmentRequestId = Id, WorkOrderId = workOrderId });

        return Result.updated;
    }

    public Result<Updated> Reject(string reviewedBy, DateTimeOffset reviewedAt)
    {
        if (Status != AppointmentRequestStatus.Pending)
            return AppointmentRequestErrors.NotPending;

        Status = AppointmentRequestStatus.Rejected;
        ReviewedBy = reviewedBy;
        ReviewedAt = reviewedAt;

        AddDomainEvent(new AppointmentRejected { AppointmentRequestId = Id });

        return Result.updated;
    }

    public Result<Updated> Cancel()
    {
        if (Status != AppointmentRequestStatus.Pending)
            return AppointmentRequestErrors.NotPending;

        Status = AppointmentRequestStatus.Cancelled;

        AddDomainEvent(new AppointmentCancelled { AppointmentRequestId = Id });

        return Result.updated;
    }

    public Result<Updated> Expire()
    {
        if (Status != AppointmentRequestStatus.Pending)
            return AppointmentRequestErrors.NotPending;

        Status = AppointmentRequestStatus.Expired;

        AddDomainEvent(new AppointmentExpired { AppointmentRequestId = Id });

        return Result.updated;
    }

    public Result<Updated> Reschedule(DateTimeOffset newStartAtUtc, DateTimeOffset newEndAtUtc)
    {
        if (Status != AppointmentRequestStatus.Approved)
            return AppointmentRequestErrors.NotApproved;

        if (newEndAtUtc <= newStartAtUtc)
            return AppointmentRequestErrors.InvalidTiming;

        RequestedStartAtUtc = newStartAtUtc;
        RequestedEndAtUtc = newEndAtUtc;
        Status = AppointmentRequestStatus.Pending;
        WorkOrderId = null;
        ReviewedBy = null;
        ReviewedAt = null;

        AddDomainEvent(new AppointmentRescheduled { AppointmentRequestId = Id });

        return Result.updated;
    }
}
