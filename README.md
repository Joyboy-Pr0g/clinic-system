# Clinic system (home nursing platform)

ASP.NET Core 9 MVC web app with ASP.NET Core Identity and SQL Server. The database schema is managed with Entity Framework Core migrations.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server** (e.g. [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) or LocalDB included with Visual Studio)
- Optional: [dotnet-ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) global tool if you want to run migration commands from the command line

## First-time setup

### 1. Clone and restore

```bash
git clone <your-repo-url>
cd clinic-system
dotnet restore
```

### 2. Database server and connection string

The app reads **`SqlCon` first**, then falls back to **`DefaultConnection`** in `appsettings.json` (see `Program.cs`).

1. Install and start SQL Server (or LocalDB).
2. Open `appsettings.json` and set **both** connection strings (or at least `SqlCon`) to point at **your** instance and database name.

**Examples:**

- **SQL Express on the same machine** (replace `YOUR_PC\INSTANCE`):

  `Server=YOUR_PC\SQLEXPRESS;Database=clinic-system;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;`

- **LocalDB** (common on dev machines with Visual Studio):

  `Server=(localdb)\\MSSQLLocalDB;Database=clinic-system;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;`

- **SQL authentication** (use a strong password and avoid committing secrets; prefer [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local dev):

  `Server=localhost,1433;Database=clinic-system;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true;`

The database **`clinic-system`** does not need to exist beforehand if your SQL login is allowed to create databases; otherwise create an empty database with that name in SQL Server Management Studio (or Azure Data Studio) first.

### 3. Migrations

Included migrations live under `Data/Migrations/`.

**Option A — let the app apply migrations (simplest for local runs)**  
On startup, `SeedData.EnsureSeedAsync` calls `Database.MigrateAsync()`, which applies pending migrations and then seeds roles, demo users, and sample data.

```bash
dotnet run
```

**Option B — apply migrations from the CLI before running**  
Use this if you prefer to migrate explicitly (or for automation).

Install the EF tool once (if needed):

```bash
dotnet tool install --global dotnet-ef
```

From the folder that contains `clinic-system.csproj`:

```bash
dotnet ef database update --project clinic-system.csproj --context ApplicationDbContext
```

Then start the app:

```bash
dotnet run
```

### 4. Google Maps (optional)

If you use map features, set a real key in `appsettings.json` under `GoogleMapsApiKey`, or override via configuration/environment variables for production.

## Run the site

```bash
dotnet run
```

Default URLs (from `Properties/launchSettings.json`):

- HTTPS: `https://localhost:7244`
- HTTP: `http://localhost:5224`

## Demo accounts (after seed runs)

Seeded in `Data/SeedData.cs` (only created when missing):

| Role | Email              | Password   |
|------------|--------------------|------------|
| Admin      | `admin@system.com` | `Admin@123` |
| Demo users | `nurse1@demo.com`, `clinic1@demo.com`, etc. | `Demo@123` |

## Project structure (high level)

- `Program.cs` — DI, Identity, EF SQL Server, pipeline
- `Data/` — `ApplicationDbContext`, migrations, seed data
- `Controllers/`, `Views/`, `wwwroot/` — MVC UI

## Troubleshooting

- **Cannot open database / login failed** — Check the server name, instance, Windows/SQL auth, and that SQL Server allows the connection (TCP, firewall if remote).
- **Migration errors** — Ensure the connection string is correct and the account can alter the database. Try `dotnet ef database update` to see detailed errors.
- **`dotnet ef` not found** — Run `dotnet tool install --global dotnet-ef` and ensure `%USERPROFILE%\.dotnet\tools` is on your PATH.
