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
| 1 | Done | DataSnapshot REST API with filtering |
| 2 | Done | Open-Meteo + background fetcher (every 30s) |
| 3 | Done | SignalR live updates |
| 4 | Done | JWT auth + roles (Admin / Analyst / Viewer) |
| 5 | Next | Alerts |
| 6+ | | External APIs, dashboards, admin |

## API base URL

```
http://localhost:5000
```

Frontend CORS is configured for `http://localhost:5173` (Vite) and `http://localhost:3000`.

## API examples (Phase 1)

```http
GET /api/data/snapshots?region=PL&metric=temperature&from=2026-06-01&page=1&pageSize=50
```

Response:

```json
{
  "items": [
    {
      "source": "openmeteo",
      "metric": "temperature",
      "value": 18.4,
      "unit": "°C",
      "region": "Wroclaw,PL",
      "lat": 51.1,
      "lon": 17.0,
      "timestamp": "2026-06-07T12:00:00Z"
    }
  ],
  "total": 49,
  "page": 1,
  "pageSize": 50
}
```

In Development mode the API seeds ~147 sample snapshots (48h, every 30 min) on first startup if the database is empty.

## Background data fetch (Phase 2)

Open-Meteo weather data is fetched automatically every 30 seconds for configured regions.

Configure regions in `backend/src/API/appsettings.json`:

```json
"DataFetch": {
  "IntervalSeconds": 30,
  "Regions": [
    { "Name": "Wroclaw,PL", "Latitude": 51.1, "Longitude": 17.0 }
  ]
}
```

After startup, check logs for `Persisted N weather snapshots from Open-Meteo`, then query:

```http
GET /api/data/snapshots?source=openmeteo&metric=temperature
```

## SignalR live updates (Phase 3)

**Hub URL:** `http://localhost:5000/hubs/dashboard`

**Event:** `DataUpdate`

Payload (one event per region after each fetch cycle):

```json
{
  "type": "weather",
  "region": "Wroclaw,PL",
  "data": {
    "temperature": 18.4,
    "humidity": 65,
    "wind": 12
  },
  "timestamp": "2026-06-07T12:00:30Z"
}
```

### React / TypeScript example

```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5000/hubs/dashboard")
  .withAutomaticReconnect()
  .build();

connection.on("DataUpdate", (update) => {
  console.log(update.region, update.data.temperature);
});

await connection.start();

// Optional: receive only one region
await connection.invoke("SubscribeToRegion", "Wroclaw,PL");
```

All connected clients receive every `DataUpdate` via broadcast. `SubscribeToRegion` / `UnsubscribeFromRegion` are available on the hub for future region-filtered delivery.

## Authentication (Phase 4)

JWT bearer auth with roles: **Admin**, **Analyst**, **Viewer**.

### Dev admin (seeded on first startup)

| Field | Value |
|-------|-------|
| Email | `admin@envirowatch.local` |
| Password | `Admin123!` |

### Endpoints

```http
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me          # requires token
GET  /api/admin/users      # Admin only
PUT  /api/admin/users/{id}/role
```

### Login example

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@envirowatch.local",
  "password": "Admin123!"
}
```

Response:

```json
{
  "token": "eyJ...",
  "expiresAt": "2026-06-07T13:00:00Z",
  "user": {
    "id": "...",
    "email": "admin@envirowatch.local",
    "role": "Admin"
  }
}
```

Use the token in requests: `Authorization: Bearer <token>`

In Swagger click **Authorize** and enter `Bearer <token>`.

### Role rules (for upcoming protected endpoints)

| Role | Access |
|------|--------|
| Viewer | Read-only |
| Analyst | Read + create/update (alerts, observations) |
| Admin | Full access + user management |

`GET /api/data/snapshots` and SignalR remain **public** so the frontend can build charts without login.
