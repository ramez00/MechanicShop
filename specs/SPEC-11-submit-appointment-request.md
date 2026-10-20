# SPEC-11: Submit Appointment Request

## Goal
Allow an authenticated customer to submit a booking request for a set of repair tasks on a specific date/time. The request is stored as `Pending` and the customer receives a confirmation email. Availability is re-checked at submission time to prevent race conditions.

## Endpoint
```
POST /api/v1/me/appointments
Authorization: Bearer <customer token>
X-TimeZone: Europe/Istanbul
```

### Request body
```json
{
  "carId": "<guid>",
  "repairTaskIds": ["<guid>", "..."],
  "requestedStartAt": "2026-10-21T08:00:00+03:00",
  "notes": "Optional free-text (max 500 chars)"
}
```

### Response — 201 Created
```json
{ "appointmentRequestId": "<guid>" }
```

## Business Rules
1. `carId` must belong to the authenticated customer (CustomerId claim).
2. All `repairTaskIds` must exist.
3. The requested slot must still be available: re-run the same spot + mechanic capacity check from SPEC-09 for the exact `[requestedStartAt, requestedStartAt + D)` window.
4. `requestedStartAt` must satisfy lead-time (`>= now + BookingLeadTimeMinutes`) and horizon constraints.
5. `notes` max 500 characters.

## Side Effects
- Raises `AppointmentRequested` domain event.
- Queues "Request Received" email to customer via `IEmailSender`.
- Invalidates `"appointment-request"` cache tag (so availability reflects the new hold within 30 s).

## Files to Create
- `src/MechanicShop.Application/Features/Appointments/Commands/SubmitAppointmentRequest/SubmitAppointmentRequestCommand.cs` — `{ CustomerId, CarId, List<Guid> RepairTaskIds, DateTimeOffset RequestedStartAt, string? Notes }`
- `src/MechanicShop.Application/Features/Appointments/Commands/SubmitAppointmentRequest/SubmitAppointmentRequestCommandHandler.cs`
- `src/MechanicShop.Application/Features/Appointments/Commands/SubmitAppointmentRequest/SubmitAppointmentRequestCommandValidator.cs`
- `src/MechanicShop.Contracts/Requests/Appointments/SubmitAppointmentRequestRequest.cs`

## Files to Modify
- `src/MechanicShop.Api/Controllers/MeController.cs` — add `POST /me/appointments`
- `src/MechanicShop.Application/Common/Errors/ApplicationErrors.cs` — add `Appointment_SlotNoLongerAvailable`, `Appointment_CarNotOwned`

## Handler Logic
```
1. Load car → verify car.CustomerId == query.CustomerId → else 404/403
2. Load repair tasks → verify all exist
3. Compute duration D = sum(EstimatedDurationInMins)
4. Compute endAt = requestedStartAt + D
5. Re-check availability for [requestedStartAt, endAt]:
   - Count non-Cancelled WOs overlapping window → busySpots < MaxSpots
   - Count Pending AppointmentRequests overlapping window → combined holds < MaxSpots
   - Count busy mechanics → freeMechanics ≥ 1
6. AppointmentRequest.Create(id, customerId, carId, repairTaskIds, requestedStartAt, endAt, notes)
7. context.AppointmentRequests.Add(request)
8. context.SaveChangesAsync()
9. Raise AppointmentRequested event (handled by event handler that queues email)
10. cache.RemoveByTagAsync("appointment-request")
11. Return { appointmentRequestId }
```

## Tests (`Application.UnitTests`)
- `Submit_with_valid_data_creates_pending_request`
- `Submit_with_car_not_owned_by_customer_returns_404`
- `Submit_when_slot_full_returns_409_SlotNoLongerAvailable`
- `Submit_with_notes_over_500_chars_returns_400`
- `Submit_raises_AppointmentRequested_event`

## Tests (`Api.IntegrationTests`)
- `Submit_returns_201_with_appointmentRequestId`
- `Submit_queues_exactly_one_request_received_email`
- `Submit_invalidates_availability_cache`
- `Submit_with_another_customers_car_returns_404`
- `Anonymous_submit_returns_401`
- `Staff_token_on_me_appointments_returns_403`
