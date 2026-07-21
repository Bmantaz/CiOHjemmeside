# Local Development Setup

This project connects to Azure resources (PostgreSQL) in production. To test locally
without touching the Azure deployment, follow these steps.

## 1. Start a local PostgreSQL instance

A `docker-compose.yml` is provided in this folder:

```powershell
cd CiOHjemmeside
docker compose up -d
```

This starts a local Postgres 16 container on `localhost:5432` with database `cio_dev`.

## 2. Configure local secrets

Secrets are never stored in `appsettings.json` or `appsettings.Development.json` (both are
committed to source control). Instead, use the .NET User Secrets manager, which stores
values outside the repo and is only loaded when `ASPNETCORE_ENVIRONMENT=Development`
(the default when running from Visual Studio / `dotnet run`).

```powershell
cd CiOHjemmeside\CiOHjemmeside
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=cio_dev;Username=postgres;Password=DevOnlyPassword123!"
dotnet user-secrets set "Security:EpkAccessCode" "dev-epk-code"
dotnet user-secrets set "Security:SeedUsers:DefaultPassword" "Dev123!Password"
```

These values must match the credentials used in `docker-compose.yml`.

## 3. Seed the database

Run the app (F5 in Visual Studio, or `dotnet run`) and navigate to `/seed-db`
(`Components/Pages/SeedDb.razor`) to populate the local database with test data.

## 4. Run the app

The `http`/`https` launch profiles already target `localhost` and set
`ASPNETCORE_ENVIRONMENT=Development`, so no further changes are needed. The app will use
the local Postgres instance and never touch the Azure production database.

## Notes on security

- Never commit real connection strings, passwords, or access codes to `appsettings.json`.
- User Secrets are stored per-machine outside the repository (`%APPDATA%\Microsoft\UserSecrets`
  on Windows) and are excluded from source control by design.
- The password used above is for local development only and is not the production secret.
