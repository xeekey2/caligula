# AGENTS.md

## Cursor Cloud specific instructions

### Product overview

**Caligula** is a .NET 8 / Aspire app: Blazor Server UI (`Caligula.Web`), internal BFF (`Caligula.ApiService` → SC2Pulse API), and EF Core + SQL Server (`Caligula.Api`). The main feature is ladder match comparison at `/Matchhistory`.

### Prerequisites (Linux / Cloud Agent VM)

| Component | Notes |
|-----------|--------|
| .NET 8 SDK | `dotnet-sdk-8.0` (use `/usr/bin/dotnet`, not `~/.dotnet` alone) |
| .NET 6 runtime | Required for `Caligula.Api` (`net6.0`) and `dotnet-ef`; install to `/usr/lib/dotnet` if missing |
| Aspire workload | `dotnet workload install aspire` — must match `Aspire.Hosting.AppHost` package version in `Caligula.AppHost.csproj` (currently **8.2.2**) |
| Docker | SQL Server runs in Docker; daemon needs `fuse-overlayfs` storage driver in nested VMs (see Docker install notes in Cursor Cloud docs) |

### SQL Server (Linux)

The app defaults to `(localdb)\MSSQLLocalDB` on Windows. On Linux, start SQL Server and point the app at it:

```bash
sudo service docker start   # if needed
docker compose up -d        # from repo root; see docker-compose.yml
```

```bash
export ConnectionStrings__DefaultConnection='Server=localhost,1433;Database=Caligula;User Id=sa;Password=Caligula_Dev123!;TrustServerCertificate=True;'
```

Apply migrations once (or after schema changes):

```bash
export PATH="/usr/bin:/home/ubuntu/.dotnet/tools:$PATH"
dotnet ef database update --project Caligula.Api/Caligula.Service.csproj
```

`Caligula.Web` reads `ConnectionStrings:DefaultConnection` from configuration (environment variable above or user secrets).

### Run the stack

```bash
export PATH="/usr/bin:/home/ubuntu/.dotnet/tools:$PATH"
export ConnectionStrings__DefaultConnection='Server=localhost,1433;Database=Caligula;User Id=sa;Password=Caligula_Dev123!;TrustServerCertificate=True;'
export ASPIRE_ALLOW_UNSECURED_TRANSPORT=true

dotnet run --project Caligula.AppHost/Caligula.AppHost.csproj --launch-profile http
```

- **Aspire dashboard**: `http://localhost:15210` (token printed in console)
- **Web UI**: `http://localhost:5120` (port may vary; check dashboard **Resources** if 5120 is not up)
- **ApiService** (SC2Pulse proxy): typically `http://localhost:5483` — e.g. `GET /playerid/{name}`

Match search reads the **local database only** (empty DB → “No matches found”). Ingestion uses ApiService + SC2Pulse via **Run Data Collector** on the Match History page.

### Build / test

| Task | Command |
|------|---------|
| Restore | `dotnet restore Caligula.sln` |
| Build | `dotnet build Caligula.sln` |
| Run (Aspire) | See above |
| EF migrations | `dotnet ef database update --project Caligula.Api/Caligula.Service.csproj` |

There is no separate linter or test project in this repository; `dotnet build` is the primary quality gate.

### Gotchas

- **Aspire `Projects.*` build errors**: Align `dotnet workload` aspire version with `Aspire.Hosting.AppHost` NuGet version; clean `Caligula.AppHost/obj` after changes.
- **`ASPIRE_ALLOW_UNSECURED_TRANSPORT`**: Required when using the `http` launch profile without HTTPS endpoints.
- **`Caligula.Api` library reference in AppHost**: Aspire warns ASPIRE004; optional cleanup: `IsAspireProjectResource="false"` on that `ProjectReference`.
- **PATH**: If `dotnet` reports no SDK, ensure `/usr/bin` precedes `~/.dotnet` on `PATH`.
