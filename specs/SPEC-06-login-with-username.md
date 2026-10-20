# SPEC-06: Login With Username

## Goal
Switch authentication from email-based to username-based login. Add account lockout after 5 failed attempts. Embed a `customer_id` claim in tokens issued to Customer-role users.

## Files to Modify
- `src/MechanicShop.Application/Features/Identity/Queries/GenerateTokens/GenerateTokenQuery.cs` — rename `Email` → `UserName`
- `src/MechanicShop.Application/Features/Identity/Queries/GenerateTokens/GenerateTokenQueryValidator.cs` — validate `UserName` not empty (remove email format check)
- `src/MechanicShop.Application/Common/Interfaces/IIdentityService.cs` — change `AuthenticateAsync(string email, ...)` → `AuthenticateAsync(string userName, ...)`
- `src/MechanicShop.Infrastructure/Identity/IdentityService.cs` — use `FindByNameAsync` instead of `FindByEmailAsync`; if user is in Customer role, query `Customers.UserId == user.Id` and add `new Claim("customer_id", customer.Id.ToString())`
- `src/MechanicShop.Infrastructure/DependencyInjection.cs` — enable lockout: `MaxFailedAccessAttempts = 5`, `DefaultLockoutTimeSpan = 15min`; pass `lockoutOnFailure: true`
- `src/MechanicShop.Infrastructure/Identity/CurrentUserService.cs` — extract `customer_id` claim; expose as `CustomerId`
- `src/MechanicShop.Contracts/Requests/Identity/` — update login request if it exists

## Tests
- `Application.UnitTests` — `GenerateTokenQueryValidator` (empty username/password fails)
- `Api.IntegrationTests` — `Customer_login_by_username_returns_tokens_with_role_and_customer_id`, `Seeded_manager_login_with_email_as_username_still_works`, `Wrong_password_returns_401_generic`, `Five_failures_lock_account`, `Refresh_token_keeps_customer_id_claim`, `Staff_token_has_no_customer_id_claim`
