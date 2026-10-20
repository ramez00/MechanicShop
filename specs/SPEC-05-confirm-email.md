# SPEC-05: Confirm Email and Resend

## Goal
Customers must confirm their email before they can log in. Provide endpoints to confirm and to resend the confirmation link.

## Files to Create
- `src/MechanicShop.Application/Features/Identity/Commands/ConfirmEmail/ConfirmEmailCommand.cs` — `{ UserId, Token }`
- `src/MechanicShop.Application/Features/Identity/Commands/ConfirmEmail/ConfirmEmailCommandHandler.cs`
- `src/MechanicShop.Application/Features/Identity/Commands/ResendConfirmation/ResendConfirmationCommand.cs` — `{ Email }`
- `src/MechanicShop.Application/Features/Identity/Commands/ResendConfirmation/ResendConfirmationCommandHandler.cs` — always returns 202, only queues email if account is unconfirmed

## Files to Modify
- `src/MechanicShop.Application/Common/Interfaces/IIdentityService.cs` — add `ConfirmEmailAsync(userId, token)`, `ResendConfirmationAsync(email)`
- `src/MechanicShop.Infrastructure/Identity/IdentityService.cs` — implement; `AuthenticateAsync`: if user is Customer role AND `!EmailConfirmed` → return `Error.Forbidden("User_EmailNotConfirmed", ...)`
- `src/MechanicShop.Infrastructure/DependencyInjection.cs` — `TokenLifespan = TimeSpan.FromHours(24)`; `PersistKeysToDbContext<AppDbContext>`; `AddDefaultTokenProviders()`
- `src/MechanicShop.Api/Controllers/IdentityController.cs` — add `POST /identity/confirm-email` (200/400), `POST /identity/resend-confirmation` (202, rate-limited)

## Tests (`Api.IntegrationTests`)
- `Confirm_with_valid_token_sets_EmailConfirmed`
- `Confirm_twice_is_idempotent_200`
- `Confirm_with_tampered_token_returns_400`
- `Confirm_with_expired_token_returns_400`
- `Confirm_with_unknown_userId_returns_400_generic_message`
- `Unconfirmed_customer_login_returns_403_User_EmailNotConfirmed`
- `Resend_returns_202_for_unknown_confirmed_and_unconfirmed_email`
- `Seeded_staff_login_unaffected`
