# SPEC-02: Customer Role and Account Link

## Goal
Introduce a `Customer` Identity role and link each `Customer` domain entity to an ASP.NET Identity `AppUser` via a nullable `UserId` field with a unique filtered index.

## Files to Create
- `src/MechanicShop.Domain/Identity/RoleNames.cs` — `public static class RoleNames { public const string Customer = "Customer"; /* Staff constants too */ }`

## Files to Modify
- `src/MechanicShop.Domain/Customers/Customer.cs` — add `public string? UserId { get; private set; }`; add `CreateSelfRegistered(id, userId, name, email, phone)` factory that reuses existing validation + sets UserId
- `src/MechanicShop.Application/Common/Interfaces/IUser.cs` — add `string? CustomerId { get; }` property
- `src/MechanicShop.Infrastructure/Identity/CurrentUserService.cs` — extract `customer_id` claim (added in SPEC-06); expose as `CustomerId`
- `src/MechanicShop.Infrastructure/Data/ApplicationDbContextInitialiser.cs` — seed `"Customer"` role using `RoleNames.Customer`
- `src/MechanicShop.Infrastructure/Data/Configurations/CustomerConfiguration.cs` — map `UserId nvarchar(450) NULL`, add unique filtered index `WHERE UserId IS NOT NULL`, no cascade

## Migration
```
dotnet ef migrations add AddCustomerUserId \
  --project src/MechanicShop.infrastructure \
  --startup-project src/MechanicShop.Api
```

## Tests
- `Domain.UnitTests` — `CreateSelfRegistered_with_valid_data_succeeds`, `CreateSelfRegistered_with_empty_userId_fails`, `CreateSelfRegistered_with_invalid_email_fails`
- `Api.IntegrationTests` — `Two_customers_with_same_UserId_violate_unique_index`, `Many_customers_with_null_UserId_are_allowed`
- `Application.UnitTests` — `CurrentUser_returns_CustomerId_from_claim`, `returns_null_when_claim_missing`
