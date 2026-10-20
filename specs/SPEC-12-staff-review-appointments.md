# SPEC-12: Staff Review Appointment Requests

## Goal
Give staff (Manager / Receptionist) a workflow to list, approve, reject, and expire pending appointment requests. Approving a request creates a `WorkOrder` and links it back to the request. Every state change queues a notification email to the customer.

## Endpoints
All under `[Authorize(Policy = "StaffOnly")]`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/v1/appointments` | Paginated list with filters |
| PUT | `/api/v1/appointments/{id}/approve` | Approve → create WorkOrder |
| PUT | `/api/v1/appointments/{id}/reject` | Reject with reason |
| PUT | `/api/v1/appointments/{id}/expire` | Mark as Expired |

---

## GET /appointments — List Requests

### Query parameters
| Param | Type | Description |
|-------|------|-------------|
| `status` | enum? | Filter by `Pending`, `Approved`, `Rejected`, `Cancelled`, `Expired` |
| `from` | DateOnly? | RequestedStartAtUtc ≥ from |
| `to` | DateOnly? | RequestedStartAtUtc ≤ to |
| `page` | int | Default from AppSettings |
| `pageSize` | int | Default from AppSettings, max 100 |

### Response
```json
{
  "items": [
    {
      "id": "<guid>",
      "customerName": "...",
      "vehicle": "Toyota | 34ABC123",
      "requestedStartAt": "2026-10-21T08:00:00+03:00",
      "requestedEndAt":   "2026-10-21T09:00:00+03:00",
      "status": "Pending",
      "services": ["Oil Change", "Tire Rotation"],
      "notes": "...",
      "reviewedBy": null,
      "reviewedAt": null
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 10
}
```

---

## PUT /appointments/{id}/approve

### Request body
```json
{
  "laborId": "<guid>",
  "spot": 1
}
```

### Rules
1. Request must be `Pending`.
2. Re-check spot + mechanic availability for `[requestedStartAt, requestedEndAt)` excluding Cancelled WOs — same logic as SPEC-09 prereq fix.
3. Create `WorkOrder` from the request data.
4. Call `AppointmentRequest.Approve(workOrderId, reviewedBy, reviewedAt)`.
5. Invalidate `"work-order"` and `"appointment-request"` cache tags.
6. Queue "Approved" email to customer.

### Response — 200 OK
```json
{ "workOrderId": "<guid>" }
```

---

## PUT /appointments/{id}/reject

### Request body
```json
{ "reason": "No mechanics available on this date." }
```

### Rules
1. Request must be `Pending`.
2. Call `AppointmentRequest.Reject(reviewedBy, reviewedAt)`.
3. Invalidate `"appointment-request"` cache tag.
4. Queue "Rejected" email to customer (include reason).

### Response — 204 No Content

---

## PUT /appointments/{id}/expire

### Rules
1. Request must be `Pending`.
2. Call `AppointmentRequest.Expire()`.
3. Invalidate `"appointment-request"` cache tag.
4. Queue "Expired" email to customer.

### Response — 204 No Content

---

## Background Job: Auto-Expire Overdue Requests
Extend `OverdueBookingCleanupService` (or add a new hosted service) to periodically query `AppointmentRequests` where `Status == Pending && RequestedEndAtUtc < utcNow` and call `Expire()` on each, then save + queue emails.

---

## Files to Create

### Application
- `src/MechanicShop.Application/Features/Appointments/Queries/GetAppointmentRequests/GetAppointmentRequestsQuery.cs` + handler + validator
- `src/MechanicShop.Application/Features/Appointments/Dtos/AppointmentRequestDto.cs`
- `src/MechanicShop.Application/Features/Appointments/Commands/ApproveAppointmentRequest/ApproveAppointmentRequestCommand.cs` + handler + validator
- `src/MechanicShop.Application/Features/Appointments/Commands/RejectAppointmentRequest/RejectAppointmentRequestCommand.cs` + handler + validator
- `src/MechanicShop.Application/Features/Appointments/Commands/ExpireAppointmentRequest/ExpireAppointmentRequestCommand.cs` + handler

### Domain Event Handlers
- `src/MechanicShop.Application/Features/Appointments/EventHandlers/AppointmentApprovedEmailHandler.cs`
- `src/MechanicShop.Application/Features/Appointments/EventHandlers/AppointmentRejectedEmailHandler.cs`
- `src/MechanicShop.Application/Features/Appointments/EventHandlers/AppointmentExpiredEmailHandler.cs`

### Api
- `src/MechanicShop.Api/Controllers/AppointmentsController.cs` — `[Route("api/v{version:apiVersion}/appointments")]`, `StaffOnly` policy

### Contracts
- `src/MechanicShop.Contracts/Requests/Appointments/ApproveAppointmentRequestRequest.cs`
- `src/MechanicShop.Contracts/Requests/Appointments/RejectAppointmentRequestRequest.cs`

## Files to Modify
- `src/MechanicShop.Application/Common/Errors/ApplicationErrors.cs` — add `Appointment_NotFound`, `Appointment_NotPending`, `Appointment_SlotTakenOnApproval`
- `src/MechanicShop.Infrastructure/BackgroundJobs/OverdueBookingCleanupService.cs` — add auto-expiry of pending requests past their end time

---

## Tests (`Application.UnitTests`)
- `Approve_pending_request_creates_work_order_and_sets_status_Approved`
- `Approve_non_pending_request_returns_409_NotPending`
- `Approve_when_slot_taken_returns_409_SlotTakenOnApproval`
- `Reject_pending_request_sets_status_Rejected`
- `Expire_pending_request_sets_status_Expired`
- `Background_job_expires_overdue_pending_requests`

## Tests (`Api.IntegrationTests`)
- `Get_appointments_returns_paginated_list`
- `Get_appointments_filtered_by_status_Pending`
- `Approve_returns_200_with_workOrderId`
- `Approve_queues_approved_email_to_customer`
- `Approve_invalidates_work_order_and_appointment_caches`
- `Reject_returns_204_and_queues_rejected_email`
- `Expire_returns_204_and_queues_expired_email`
- `Customer_token_on_appointments_returns_403`
- `Anonymous_gets_401`
