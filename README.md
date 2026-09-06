# 🐾 Pufzi Backend

Backend API for the Pufzi platform.

## Requirements

Before running the project, install:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 18](https://www.postgresql.org/download/windows/)
- pgAdmin 4 — included with the PostgreSQL installer
- Visual Studio 2022 with ASP.NET and web development workload

## Run the Backend

Open:

```text
Pufzi.Backend.slnx
```

in Visual Studio.

Make sure `Pufzi.Api` is selected as the startup project.

Run the project using the **HTTPS** profile.

The API will be available at:

```text
https://localhost:7154
```

## Swagger

After starting the backend, open:

```text
https://localhost:7154/swagger
```

Swagger can be used to view and test all available API endpoints.

## Database

The project uses:

```text
PostgreSQL 18
```

Local development configuration:

```text
Host: localhost
Port: 5432
Database: pufzi
Username: postgres
```

The PostgreSQL password is configured locally by each developer.

### View the Database

Open:

```text
pgAdmin 4
```

Then navigate to:

```text
Servers
└── PostgreSQL 18
    └── Databases
        └── pufzi
            └── Schemas
                └── public
                    └── Tables
```

To view the data inside a table:

```text
Right Click Table
→ View/Edit Data
→ All Rows
```

## Useful Commands

Restore dependencies:

```bash
dotnet restore
```

Build the backend:

```bash
dotnet build
```

Run tests:

```bash
dotnet test
```

Run the API:

```bash
dotnet run --project Pufzi.Api
```

## Project Structure

```text
Pufzi.Api
Pufzi.Services
Pufzi.Data
Pufzi.Contracts
Pufzi.Infrastructure
Pufzi.Tests
```
