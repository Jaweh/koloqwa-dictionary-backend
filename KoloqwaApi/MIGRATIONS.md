# Migration Setup

## Add the first migration (run from repo root):

```bash
dotnet ef migrations add InitialCreate \
  --project src/Koloqwa.Infrastructure \
  --startup-project src/Koloqwa.API \
  --output-dir Persistence/Migrations
```

## Apply migrations:

```bash
dotnet ef database update \
  --project src/Koloqwa.Infrastructure \
  --startup-project src/Koloqwa.API
```

## Generate migration SQL script (for production deployments):

```bash
dotnet ef migrations script \
  --project src/Koloqwa.Infrastructure \
  --startup-project src/Koloqwa.API \
  --output migration.sql \
  --idempotent
```

## Notes

- The `DatabaseSeeder` in Infrastructure auto-runs `MigrateAsync()` on startup,
  so dev environments self-migrate on first run.
- In production, prefer the SQL script approach above and run migrations
  as part of your CI/CD pipeline before deploying the new API version.
- The `SubmissionQueueConfiguration` uses a shared FK (`EntryId`) for both
  Word and Phrase entries. EF handles this via soft navigation; no DB-level
  polymorphic FK constraint is used — the `EntryType` column disambiguates.
