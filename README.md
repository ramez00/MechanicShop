# MechanicShop

A management system for an auto repair shop, built with .NET 10 and Clean Architecture.
It covers customers and their vehicles, repair tasks, work orders scheduled into time slots
("spots"), labor assignment, invoicing, and a dashboard.

## Features

- **Daily schedule** — book work orders into shop spots, relocate them, reassign labor, and move them through their states.
- **Work orders** — `Scheduled → InProgress → Completed`, with cancellation allowed until completion.
- **Customers and vehicles** — manage customers and the cars they own.
- **Repair tasks** — a catalog of tasks with parts, labor cost, and estimated duration.
- **Billing** — issue an invoice for a work order, record payment, and download it as a PDF.
- **Dashboard** — today's work order statistics.
- **Real-time updates** — work order changes are pushed to clients over SignalR.
- **Roles** — managers have full access; labor users can only update work orders assigned to them.

## Tech stack

| Area | Technology |
| --- | --- |
| Backend | ASP.NET Core (.NET 10), MediatR (CQRS), FluentValidation |
| Frontend | Blazor WebAssembly, hosted by the API project |
| Data | EF Core with SQL Server, HybridCache |
| Auth | ASP.NET Core Identity, JWT tokens |
| Real-time | SignalR |
| PDF | QuestPDF |
| Observability | Serilog, OpenTelemetry, Seq, Prometheus, Grafana |
| Tests | xUnit |

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (a local default instance), or Docker Desktop for the Docker option

### Run locally

```bash
dotnet run --project src/MechanicShop.Api
```

Then open http://localhost:5263. The API project serves the Blazor client, so this one command
starts the whole app. In Development it creates the `MechanicShopDb` database and seeds sample
data on startup.

To use HTTPS (https://localhost:7142):

```bash
dotnet run --project src/MechanicShop.Api --launch-profile https
```

The default connection string targets `Server=.` with Windows authentication. If your SQL Server
is a different instance, change `ConnectionStrings:DefaultConnection` in
`src/MechanicShop.Api/appsettings.json`.

### Run with Docker Compose

```bash
docker compose up --build
```

| Service | URL |
| --- | --- |
| App | http://localhost:5001 |
| Seq (logs and traces) | http://localhost:8081 |
| Prometheus | http://localhost:9090 |
| Grafana | http://localhost:3000 |

### Seeded accounts

Each seeded user's password is the same as their email address.

| Role | Email |
| --- | --- |
| Manager | `pm@localhost` |
| Labor | `john.labor@localhost` |
| Labor | `peter.labor@localhost` |
| Labor | `kevin.labor@localhost` |
| Labor | `suzan.labor@localhost` |

## Configuration

Shop rules live in the `AppSettings` section of `src/MechanicShop.Api/appsettings.json`:

| Setting | Purpose |
| --- | --- |
| `OpeningTime` / `ClosingTime` | Shop operating hours |
| `MaxSpots` | Number of work spots that can be booked in parallel |
| `MinimumAppointmentDurationInMinutes` | Shortest bookable work order |
| `BookingCancellationThresholdMinutes` | How long an unstarted booking is kept before it counts as overdue |
| `OverdueBookingCleanupFrequencyMinutes` | How often the overdue-booking cleanup job runs |
| `AllowedOrigins` | CORS origins |

`JwtSettings:Secret` in `appsettings.json` is a placeholder — replace it before deploying anywhere.

## Project structure

```
src/
  MechanicShop.Domain          Aggregates, business rules, domain events, Result/Error types
  MechanicShop.Application     CQRS commands/queries, validators, pipeline behaviors
  MechanicShop.infrastructure  EF Core, Identity, caching, SignalR, PDF generation, background jobs
  MechanicShop.Api             Controllers, API versioning, exception handling, hosts the client
  MechanicShop.Contracts       Request/response DTOs shared by the API and the client
  MechanicShop.Client          Blazor WebAssembly UI
tests/
  MechanicShop.Domain.UnitTests
  MechanicShop.Application.UnitTests
  MechanicShop.Application.SubcutaneousTests
  MechanicShop.Api.IntegrationTests
  MechanicShop.Tests.Common
```

Dependencies flow inward: `Api → Application → Domain`, and `Infrastructure → Application/Domain`.
`Domain` depends on nothing.

Expected business failures are returned as `Result<T>` values carrying an `Error`, not thrown as
exceptions. Controllers translate them into HTTP responses (404, 409, 400, 403).

## API

Versioned endpoints live under `/api/v1/`:

| Resource | Route |
| --- | --- |
| Work orders and schedule | `/api/v1/workorders` |
| Customers | `/api/v1/customers` |
| Repair tasks | `/api/v1/repair-tasks` |
| Labors | `/api/v1/labors` |
| Invoices | `/api/v1/invoices` |
| Dashboard | `/api/v1/dashboard` |
| Tokens and current user | `/identity` |

In Development the OpenAPI document is served at `/openapi/v1.json`, with Swagger UI and Scalar
enabled for browsing it. SignalR clients connect to `/hubs/workorders`.

## Development

```bash
# Build
dotnet build MechanicShop.slnx

# Run all tests
dotnet test MechanicShop.slnx

# Run one test project, or tests matching a name
dotnet test tests/MechanicShop.Domain.UnitTests
dotnet test --filter "FullyQualifiedName~WorkOrderTests"
```

### Database migrations

The `DbContext` lives in the infrastructure project and the API is the startup project, so both
must be specified:

```bash
dotnet ef migrations add <Name> \
  --project src/MechanicShop.infrastructure \
  --startup-project src/MechanicShop.Api

dotnet ef database update \
  --project src/MechanicShop.infrastructure \
  --startup-project src/MechanicShop.Api
```
