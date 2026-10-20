# SPEC-09 Prerequisite: Exclude Cancelled Work Orders From Overlap Checks

## Goal
Cancelled work orders must not block spots, labor, or vehicles. Fix all overlap predicates to add `State != Cancelled` before implementing the availability endpoint.

## Files to Modify

### `src/MechanicShop.Infrastructure/Services/WorkOrderPolicy.cs`
- `CheckSpotAvailabilityAsync` — add `&& a.State != WorkOrderState.Cancelled`
- `IsLaborOccupied` — same filter
- `IsVehicleAlreadyScheduled` — same filter (also remove the redundant `.ToListAsync()` query)

### `src/MechanicShop.Application/Features/WorkOrders/Commands/CreateWorkOrder/CreateWorkOrderCommandHandler.cs`
- Inline vehicle-conflict check — add `&& a.State != WorkOrderState.Cancelled`
- Inline labor-occupied check — same

## Tests (`Application.UnitTests`)
- `Cancelled_work_order_does_not_block_spot`
- `Cancelled_work_order_does_not_block_labor`
- `Cancelled_work_order_does_not_block_vehicle`

## Tests (`Api.IntegrationTests`)
- `Staff_can_create_work_order_over_a_cancelled_one`
