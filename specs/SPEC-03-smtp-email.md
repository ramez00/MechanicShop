# SPEC-03: SMTP Email

## Goal
Add an async, queued email system backed by MailKit SMTP. Emails are enqueued in-process and delivered by a background service with 3-retry-with-backoff. HTML templates cover all customer-facing lifecycle events.

## Files to Create
- `src/MechanicShop.Application/Common/Interfaces/IEmailSender.cs` — `Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct)`
- `src/MechanicShop.Application/Common/Interfaces/IEmailQueue.cs` — `void Enqueue(string to, string subject, string htmlBody)`
- `src/MechanicShop.Infrastructure/Services/Email/EmailMessage.cs` — record `{ string To, string Subject, string HtmlBody }`
- `src/MechanicShop.Infrastructure/Services/Email/SmtpEmailSender.cs` — MailKit `SmtpClient`, reads `Smtp` config section
- `src/MechanicShop.Infrastructure/Services/Email/EmailBackgroundService.cs` — `BackgroundService`, `Channel<EmailMessage>`, 3-retry-with-backoff loop
- `src/MechanicShop.Infrastructure/Services/Email/EmailQueue.cs` — wraps `Channel<EmailMessage>`, implements `IEmailQueue`
- `src/MechanicShop.Infrastructure/Services/Email/EmailTemplates.cs` — static class: `ConfirmEmail(name, link)`, `RequestReceived(...)`, `Approved(...)`, `Rejected(...)`, `Expired(...)`, `Cancelled(...)`, `Rescheduled(...)`. HTML-encode all user values.
- `src/MechanicShop.Infrastructure/Settings/SmtpSettings.cs` — `Host`, `Port`, `UseStartTls`, `UserName`, `Password`, `FromAddress`, `FromName`

## Files to Modify
- `src/MechanicShop.Infrastructure/Settings/AppSettings.cs` — add `ClientBaseUrl`
- `src/MechanicShop.Infrastructure/DependencyInjection.cs` — register `SmtpEmailSender`, `EmailBackgroundService`, `EmailQueue`; bind `SmtpSettings`; fail-fast in Production if not configured
- `src/MechanicShop.Api/appsettings.json` — add `Smtp` section (no password)
- `docker-compose.yml` — add `smtp4dev` service (SMTP on 2525, web UI on 3000); point API at it in Development env

## NuGet
Add `MailKit` to `MechanicShop.infrastructure.csproj`

## Tests
- `Application.UnitTests` — template rendering + HTML-encoding tests for each template
- `Api.IntegrationTests` — `Queued_email_reaches_transport`, `Smtp_failure_does_not_fail_request`, `Production_without_Smtp_config_fails_at_startup`
