# SPEC-09: Availability with Capacity Holds

## Goal
Return a list of bookable 15-minute-aligned time slots for a given date and set of repair tasks, accounting for active work orders **and** pending appointment-request holds so the same slot cannot be double-booked.

## Endpoint
```
GET /api/v1/me/availability
  ?date=2026-10-21
  &repairTaskIds=<guid>&repairTaskIds=<guid>
  &timeZone=Europe/Istanbul
```

## New AppSettings Fields
| Field | Type | Default |
|-------|------|---------|
| `BookingLeadTimeMinutes` | int | 60 |
| `BookingHorizonDays` | int | 30 |

Also update `appsettings.json`:
- `OpeningTime`: `"08:00"`
- `ClosingTime`: `"18:00"`

## Algorithm
1. Load selected repair tasks; sum `EstimatedDurationInMins` → **D** minutes.
2. Validate: date not in past, date ≤ today + `BookingHorizonDays`, D ≤ window length.
3. Count total mechanics: `Employees.Count(e => e.Role == Mechanic)`.
4. Load day's **non-Cancelled** work orders (single DB query).
5. Load day's **Pending** appointment requests (single DB query).
6. For each 15-min candidate start **S** in `[OpeningTime, ClosingTime − D]`:
   - Skip if `S_utc < nowUtc + leadTime`.
   - For each 15-min sub-slot of `[S, S+D)`:
     - `busySpots` = distinct spots used by non-Cancelled WOs overlapping the sub-slot.
     - `pendingHolds` = pending requests overlapping the sub-slot.
     - `freeSpots = MaxSpots − busySpots − pendingHolds ≥ 1` ✓
     - `busyMechanics` = count of non-Cancelled WOs overlapping (each WO holds one mechanic).
     - `pendingMechanicHolds` = count of pending requests overlapping.
     - `freeMechanics = totalMechanics − busyMechanics − pendingMechanicHolds ≥ 1` ✓
   - If ALL sub-slots pass → add `{ StartAt, EndAt }` to result.

## Response
```json
{
  "slots": [
    { "startAt": "2026-10-21T08:00:00+03:00", "endAt": "2026-10-21T09:00:00+03:00" },
    ...
  ],
  "message": null
}
```
`message` is non-null when the list is empty (explains why: no mechanics, duration too long, etc.).

## Caching
- Cache key: `availability:{date}:tz={tzId}:{sorted repairTaskIds joined by ,}`
- Tags: `["work-order", "appointment-request"]`
- Expiration: **30 seconds**

## Validator Rules
- `RepairTaskIds` not empty
- `RepairTaskIds.Count ≤ 10`
- `Date ≥ today` (not in the past)
- `Date ≤ today + BookingHorizonDays`

## Files to Create
- `src/MechanicShop.Application/Common/Interfaces/IShopSettings.cs`
- `src/MechanicShop.Infrastructure/Settings/ShopSettingsAdapter.cs`
- `src/MechanicShop.Application/Features/Scheduling/Dtos/BookingSlotDto.cs` — `record { DateTimeOffset StartAt, DateTimeOffset EndAt }`
- `src/MechanicShop.Application/Features/Scheduling/Dtos/AvailabilityResponse.cs` — `record { IReadOnlyList<BookingSlotDto> Slots, string? Message }`
- `src/MechanicShop.Application/Features/Scheduling/Queries/GetAvailability/GetAvailabilityQuery.cs`
- `src/MechanicShop.Application/Features/Scheduling/Queries/GetAvailability/GetAvailabilityQueryValidator.cs`
- `src/MechanicShop.Application/Features/Scheduling/Queries/GetAvailability/GetAvailabilityQueryHandler.cs`
- `src/MechanicShop.Api/Controllers/MeController.cs`

## Files to Modify
- `src/MechanicShop.Infrastructure/Settings/AppSettings.cs` — add `BookingLeadTimeMinutes`, `BookingHorizonDays`
- `src/MechanicShop.Api/appsettings.json` — update times, add new fields
- `src/MechanicShop.Infrastructure/DependencyInjection.cs` — register `IShopSettings → ShopSettingsAdapter`

## Unit Tests (12 theory rows — `Application.UnitTests`)
Setup: MaxSpots=4, 2 mechanics, 08:00–18:00, lead=60 min, now=07:00, D=60 min

| # | Scenario | Expected |
|---|----------|----------|
| 1 | Empty day | 37 slots (08:00–17:00 every 15 min) |
| 2 | 4 WOs 10:00–11:00 | 10:00 not offered, 09:15 not offered, 11:00 offered |
| 3 | 2 WOs (both mechanics) 10:00–11:00 | 10:00 not offered (mechanic limit) |
| 4 | 1 WO + 1 Pending at 10:00 | 10:00 not offered (pending counts as hold) |
| 5 | 4 Cancelled WOs at 10:00 | 10:00 offered |
| 6 | Rejected/Expired request at 10:00 | 10:00 offered |
| 7 | D=90, empty day | Last slot 16:30 |
| 8 | now=14:20 | First slot ≥ 15:30 |
| 9 | D=11 h | Empty list + message |
| 10 | date = yesterday / today+31 | 400 Validation error |
| 11 | 0 mechanics | Empty list + message |
| 12 | DST date | No duplicate/missing local times |

## Integration Tests (`Api.IntegrationTests`)
- `Availability_response_contains_no_spot_labor_or_vehicle_fields`
- `Availability_runs_at_most_3_sql_queries`
- `New_request_disappears_from_availability_within_cache_ttl`
