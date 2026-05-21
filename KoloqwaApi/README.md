# Koloqwa Dictionary API

Backend for The Koloqwa Dictionary — a platform for Liberian local language words and phrase meanings.

## Stack

- **.NET 9** Web API
- **Entity Framework Core 9** + **PostgreSQL** (Npgsql)
- **Clean Architecture** (Domain → Application → Infrastructure → API)
- **MediatR** (CQRS pattern)
- **AutoMapper**, **FluentValidation**
- **JWT** Bearer authentication
- **Serilog** structured logging

## Quick start

### 1. Prerequisites

- .NET 9 SDK
- PostgreSQL 15+

### 2. Configure

Edit `src/Koloqwa.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=koloqwa_dev;Username=postgres;Password=YOUR_PG_PASSWORD"
  }
}
```

Set a strong JWT secret in `appsettings.json` (`Jwt:Secret` — min 32 chars).

### 3. Run migrations

```bash
cd src/Koloqwa.API
dotnet ef database update --project ../Koloqwa.Infrastructure
```

### 4. Run the API

```bash
dotnet run --project src/Koloqwa.API
```

Swagger UI: http://localhost:5000 (dev mode)

### 5. Seed credentials

SuperAdmin seeded automatically on first run:
- Email: `admin@koloqwa.lr`
- Password: `Admin@123!`

> ⚠️ Change this immediately in production.

## Creating migrations

```bash
cd src/Koloqwa.API
dotnet ef migrations add InitialCreate \
  --project ../Koloqwa.Infrastructure \
  --startup-project . \
  --output-dir Persistence/Migrations
```

## Architecture

```
KoloqwaApi/
└── src/
    ├── Koloqwa.Domain          # Entities, Enums, Exceptions — zero dependencies
    ├── Koloqwa.Application     # Use cases (Commands/Queries), DTOs, Interfaces
    ├── Koloqwa.Infrastructure  # EF Core, JWT, Services — implements Application interfaces
    └── Koloqwa.API             # Controllers, Middleware, Program.cs
```

## API Modules

| Module | Base path | Auth |
|---|---|---|
| Auth | `/api/v1/auth` | Public |
| Words | `/api/v1/words` | GET: Public · POST: Auth |
| Phrases | `/api/v1/phrases` | GET: Public · POST: Auth |
| Admin | `/api/v1/admin` | Admin / SuperAdmin |

## Roles

`User` → `Admin` → `SuperAdmin`
