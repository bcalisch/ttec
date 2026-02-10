# Backend API

## Local setup

1. Start SQL Server:

```bash
docker compose up -d
```

2. Restore packages (requires NuGet access):

```bash
DOTNET_CLI_HOME=/tmp/dotnet DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 dotnet restore
```

3. Install local tools:

```bash
dotnet tool restore
```

4. Create the initial migration (first time only):

```bash
dotnet ef migrations add InitialCreate
```

5. Apply migrations:

```bash
dotnet ef database update
```

## Run the API

```bash
dotnet run
```

## Notes

- The API uses SQL Server spatial types (`geography`) with SRID 4326.
- Connection string lives in `appsettings.json`.
