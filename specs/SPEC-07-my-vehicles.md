# SPEC-07: My Vehicles API

## Goal
Let authenticated customers manage their own vehicles via a `/me/vehicles` CRUD API. Staff tokens must be denied (403).

## Endpoint
`[Route("api/v{version:apiVersion}/me")]` — `CustomerOnly` policy

| Method | Route | Description |
|--------|-------|-------------|
| GET | /me/vehicles | List own vehicles |
| POST | /me/vehicles | Add a vehicle |
| PUT | /me/vehicles/{id} | Update a vehicle |
| DELETE | /me/vehicles/{id} | Delete a vehicle |

## Files to Create
- `src/MechanicShop.Application/Features/Customers/Queries/GetMyVehicles/GetMyVehiclesQuery.cs`
- `src/MechanicShop.Application/Features/Customers/Queries/GetMyVehicles/GetMyVehiclesQueryHandler.cs`
- `src/MechanicShop.Application/Features/Customers/Commands/AddMyVehicle/AddMyVehicleCommand.cs` + handler + validator
- `src/MechanicShop.Application/Features/Customers/Commands/UpdateMyVehicle/UpdateMyVehicleCommand.cs` + handler + validator
- `src/MechanicShop.Application/Features/Customers/Commands/DeleteMyVehicle/DeleteMyVehicleCommand.cs` + handler
- `src/MechanicShop.Api/Controllers/MeController.cs` — new controller, `CustomerOnly` policy

## Files to Modify
- `src/MechanicShop.Domain/Customers/Cars/Car.cs` — fix year rule: `year <= DateTimeOffset.UtcNow.Year + 1`; normalize plate (uppercase + remove spaces)
- `src/MechanicShop.Infrastructure/Data/Configurations/CarConfiguration.cs` — unique index on `LicensePlate`
- `src/MechanicShop.Application/Common/Errors/ApplicationErrors.cs` — add `Car_PlateTaken`, `Car_HasActiveBookings`

## Migration
```
dotnet ef migrations add AddCarLicensePlateUniqueIndex \
  --project src/MechanicShop.infrastructure \
  --startup-project src/MechanicShop.Api
```

## Tests
- `Domain.UnitTests` — `Car_Create_year_rules` theory, `Plate_is_normalized`
- `Api.IntegrationTests` — `Get_my_vehicles_returns_only_own_cars`, `Post_vehicle_ignores_customerId_in_body`, `Put_or_delete_other_customers_car_returns_404`, `Duplicate_plate_returns_409_Car_PlateTaken`, `Delete_car_with_active_booking_returns_409`, `Staff_token_on_me_vehicles_returns_403`
