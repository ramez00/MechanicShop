# SPEC-10: AppointmentRequest Aggregate (Domain + Migration Only)

## Goal
Define the `AppointmentRequest` aggregate with its full state machine, domain events, EF configuration, and migration. No HTTP endpoints are added in this spec.

## State Machine
```
Pending → Approved
Pending → Rejected
Pending → Cancelled
Pending → Expired
Approved → Rescheduled (re-enters Pending)
```
All transitions from non-Pending states are errors.

## Files to Create

### Domain
- `src/MechanicShop.Domain/AppointmentRequests/Enums/AppointmentRequestStatus.cs` — `Pending, Approved, Rejected, Cancelled, Expired`
- `src/MechanicShop.Domain/AppointmentRequests/Events/AppointmentRequested.cs`
- `src/MechanicShop.Domain/AppointmentRequests/Events/AppointmentApproved.cs`
- `src/MechanicShop.Domain/AppointmentRequests/Events/AppointmentRejected.cs`
- `src/MechanicShop.Domain/AppointmentRequests/Events/AppointmentCancelled.cs`
- `src/MechanicShop.Domain/AppointmentRequests/Events/AppointmentExpired.cs`
- `src/MechanicShop.Domain/AppointmentRequests/Events/AppointmentRescheduled.cs`
- `src/MechanicShop.Domain/AppointmentRequests/AppointmentRequestErrors.cs`
- `src/MechanicShop.Domain/AppointmentRequests/AppointmentRequest.cs` — aggregate extending `AuditableEntity`; private ctor; static `Create(...)` factory returning `Result<AppointmentRequest>`; state-transition methods each guarded by `Status == Pending` check

### Infrastructure
- `src/MechanicShop.Infrastructure/Data/Configurations/AppointmentRequestConfiguration.cs`
  - Columns: `Id`, `CustomerId` (FK), `CarId` (FK), `Status` (string), `RequestedStartAtUtc`, `RequestedEndAtUtc`, `Notes` (max 500), `WorkOrderId` (nullable FK), `ReviewedBy` (nullable), `ReviewedAt` (nullable), `RowVersion` (concurrency token)
  - Join table: `AppointmentRequestRepairTasks (AppointmentRequestId, RepairTaskId)`
  - Index: `(Status, RequestedStartAtUtc)`

## Files to Modify
- `src/MechanicShop.Application/Common/Interfaces/IAppDbContext.cs` — add `DbSet<AppointmentRequest> AppointmentRequests`
- `src/MechanicShop.Infrastructure/Data/AppDbContext.cs` — add the DbSet

## Migration
```
dotnet ef migrations add AddAppointmentRequests \
  --project src/MechanicShop.infrastructure \
  --startup-project src/MechanicShop.Api
```

## Tests (`Domain.UnitTests`)
- `Create_validates_inputs` — empty tasks, end ≤ start, notes > 500 chars
- `State_transitions` theory — every valid and invalid cell of the transition table
- `Approve_sets_WorkOrderId_ReviewedBy_ReviewedAt`
