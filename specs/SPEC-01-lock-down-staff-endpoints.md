# SPEC-01: Lock Down Staff Endpoints

## Goal
Restrict all staff-facing endpoints so only authenticated employees (Manager, Mechanic, Receptionist) can access them. Customers and anonymous users must be denied.

## Policy
Add a `StaffOnly` authorization policy that requires any of the three staff roles.

## Files to Modify
- `src/MechanicShop.Infrastructure/DependencyInjection.cs` — add `StaffOnly` policy (roles: Manager, Mechanic, Receptionist)
- `src/MechanicShop.Api/Controllers/CustomerController.cs` — add `[Authorize(Policy = "StaffOnly")]` at controller level
- `src/MechanicShop.Api/Controllers/WorkOrdersController.cs` — same
- `src/MechanicShop.Api/Controllers/LaborsController.cs` — same
- `src/MechanicShop.Api/Controllers/RepairTasksController.cs` — same
- `src/MechanicShop.Api/Controllers/DashboardController.cs` — same
- `src/MechanicShop.Api/Controllers/InvoicesController.cs` — same
- `src/MechanicShop.Infrastructure/RealTime/WorkOrderHub.cs` — add `[Authorize(Policy = "StaffOnly")]`

## Tests (`Api.IntegrationTests`)
- `Every_staff_controller_action_requires_StaffOnly` — reflect over all non-identity controllers
- `Customer_token_gets_403_on_staff_endpoints` — theory over 6 endpoints
- `Manager_token_still_gets_200_on_staff_endpoints`
- `Anonymous_gets_401_on_staff_endpoints`
