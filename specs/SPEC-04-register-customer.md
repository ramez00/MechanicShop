# SPEC-04: Register Customer

## Goal
Allow a new user to self-register as a customer. Creates both an `AppUser` (Identity) and a `Customer` domain entity in a single transaction, then queues a confirmation email.

## Files to Create
- `src/MechanicShop.Application/Features/Identity/Commands/RegisterCustomer/RegisterCustomerCommand.cs` — `{ UserName, Password, ConfirmPassword, Email, FullName, PhoneNumber }`
- `src/MechanicShop.Application/Features/Identity/Commands/RegisterCustomer/RegisterCustomerCommandHandler.cs` — creates `AppUser` + `Customer` in one transaction, queues confirmation email via `IEmailQueue`
- `src/MechanicShop.Application/Features/Identity/Commands/RegisterCustomer/RegisterCustomerCommandValidator.cs`
- `src/MechanicShop.Contracts/Requests/Identity/RegisterCustomerRequest.cs`

## Validation Rules
- `UserName`: regex `[a-zA-Z0-9._-]`, no `@`, 3–30 chars
- `Password`: ≥8 chars, at least one uppercase, lowercase, digit
- `ConfirmPassword`: must match `Password`
- `Email`: valid email format
- `PhoneNumber`: E.164 format

## Files to Modify
- `src/MechanicShop.Application/Common/Interfaces/IIdentityService.cs` — add `CreateUserAsync(userName, email, password, role)` and `GenerateEmailConfirmationTokenAsync(userId)`
- `src/MechanicShop.Infrastructure/Identity/IdentityService.cs` — implement new methods
- `src/MechanicShop.Infrastructure/DependencyInjection.cs` — update `IdentityOptions.Password` (RequiredLength=8, RequireDigit=true, RequireUppercase=true, RequireLowercase=true); add rate limiter on `/identity/register`; set `RequireUniqueEmail = true`
- `src/MechanicShop.Api/Controllers/IdentityController.cs` — add `POST /identity/register` → 201 `{ userId }`
- `src/MechanicShop.Application/Common/Errors/ApplicationErrors.cs` — add `User_UserNameTaken`, `User_EmailTaken`

## Tests
- `Application.UnitTests` — `RegisterCustomerValidator` theory (bad usernames, weak passwords, confirm mismatch, bad email, bad phone); valid input passes
- `Api.IntegrationTests` — `Register_valid_returns_201_and_creates_user_and_customer`, `Register_queues_exactly_one_confirmation_email`, `Register_duplicate_username_returns_409`, `Register_duplicate_email_returns_409`, `Register_rolls_back_user_when_customer_insert_fails`, `Register_returns_429_after_rate_limit`, `Register_response_contains_no_tokens`
