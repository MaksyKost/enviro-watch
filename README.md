# enviro-watch

Real-time environmental monitoring platform built with ASP.NET Core, SignalR, and React. Aggregates live data from weather, air quality, and air traffic APIs with user-configurable dashboards, threshold alerts, and historical data persistence.

## Project structure

```
enviro-watch/
├── backend/          # ASP.NET Core 8 API (this repo's backend)
├── frontend/         # React app (partner)
└── docker-compose.yml
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (for PostgreSQL)

## Quick start (backend)

### 1. Environment

```bash
cp .env.example .env
```

### 2. Start PostgreSQL

```bash
docker compose up -d postgres
```

### 3. Run API

```bash
cd backend
dotnet restore
dotnet ef database update --project src/Infrastructure --startup-project src/API
dotnet run --project src/API
```

### 4. Verify

- Health: http://localhost:5000/health
- Swagger: http://localhost:5000/swagger

## Development phases

| Phase | Status | Description |
|-------|--------|-------------|
| 0 | Done | Solution scaffold, Docker, health endpoint |
| 1 | Next | DataSnapshot REST API with filtering |
| 2 | | Open-Meteo + background fetcher |
| 3 | | SignalR live updates |
| 4+ | | Auth, alerts, external APIs, dashboards |

## API base URL

```
http://localhost:5000
```

Frontend CORS is configured for `http://localhost:5173` (Vite) and `http://localhost:3000`.
