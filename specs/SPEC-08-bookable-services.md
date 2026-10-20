# SPEC-08: Bookable Services Catalog

## Goal
Expose a read-only, cached catalog of all repair tasks so customers can pick services when requesting an appointment. Response must not include part details.

## Endpoint
`GET /me/services` — authenticated (any role)

## Response DTO
```
BookableServiceDto {
  Id: Guid
  Name: string
  EstimatedDurationInMins: int
  LaborCost: decimal
  EstimatedTotal: decimal   // LaborCost + sum(part.Price * part.Quantity)
}
```

## Files to Create
- `src/MechanicShop.Application/Features/RepairTasks/Queries/GetBookableServices/GetBookableServicesQuery.cs` — `ICachedQuery`, tag `"repair-task"`, 10 min expiration
- `src/MechanicShop.Application/Features/RepairTasks/Queries/GetBookableServices/GetBookableServicesQueryHandler.cs`
- `src/MechanicShop.Application/Features/RepairTasks/Dtos/BookableServiceDto.cs`

## Files to Modify
- `src/MechanicShop.Api/Controllers/MeController.cs` — add `GET /me/services`
- `src/MechanicShop.Application/Features/RepairTasks/Commands/Update*/UpdateRepairTaskCommandHandler.cs` — add `RemoveByTagAsync("repair-task")`

## Tests
- `Application.UnitTests` — `Services_query_maps_duration_and_price`
- `Api.IntegrationTests` — `Customer_gets_all_repair_tasks`, `Response_has_no_part_details`, `Manager_update_invalidates_services_cache`, `Anonymous_gets_401`
