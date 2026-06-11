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

### 2. Start stack (PostgreSQL + API)

```bash
docker compose up -d
```

Or PostgreSQL only:

```bash
docker compose up -d postgres
cd backend && dotnet run --project src/API
```

### 3. Run API locally

```bash
cd backend
dotnet restore
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
| 5 | Done | Alerts CRUD + background threshold checker |
| 6 | Done | OpenWeather, OpenAQ, OpenSky (parallel fetch) |
| 7 | Done | Manual observations |
| 8 | Done | Dashboards & widgets |
| 9 | Done | FluentValidation, cleanup job, Docker, admin stats |

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

## Alerts (Phase 5)

Threshold alerts with background checking every 30 seconds.

### Endpoints (require JWT)

| Method | Endpoint | Role |
|--------|----------|------|
| POST | `/api/alerts` | Analyst, Admin |
| GET | `/api/alerts` | Any authenticated |
| GET | `/api/alerts/{id}/logs` | Owner or Admin |
| DELETE | `/api/alerts/{id}` | Owner (Analyst+) or Admin |

### Create alert

```http
POST /api/alerts
Authorization: Bearer <token>
Content-Type: application/json

{
  "metric": "temperature",
  "region": "Wroclaw,PL",
  "threshold": 35.0,
  "condition": "Above",
  "notifyEmail": true
}
```

`condition`: `Above` or `Below`

### How checking works

1. `AlertProcessorBackgroundService` runs every 30s
2. For each active alert, loads latest snapshot for `region` + `metric`
3. If threshold exceeded → writes `AlertLog`
4. **Cooldown** (default 5 min) prevents duplicate logs for the same alert
5. `notifyEmail: true` → logs email stub (real SMTP later)

### Test flow

1. Login as admin
2. Create alert with low threshold (e.g. `temperature` above `0` for Wroclaw,PL)
3. Wait ~30s
4. `GET /api/alerts/{id}/logs` → should show triggered entries

## External APIs (Phase 6)

All sources are fetched **in parallel** every 30 seconds via `DataFetchService`.

| Source | Client | Metrics | API key |
|--------|--------|---------|---------|
| `openmeteo` | OpenMeteoClient | temperature, humidity, wind | No |
| `openweather` | OpenWeatherClient | temperature, humidity, wind | Yes |
| `openaq` | OpenAQClient | pm25, pm10, aqi | No |
| `opensky` | OpenSkyClient | aircraft_count, avg_altitude | No |

### OpenWeather setup

Add your key to `.env`:

```
OPENWEATHER_API_KEY=your-key-here
```

Or `OpenWeather:ApiKey` in `appsettings.json`. Without a key, OpenWeather is skipped.

### Query by source

```http
GET /api/data/snapshots?source=opensky&metric=aircraft_count
GET /api/data/snapshots?source=openaq&metric=pm25
GET /api/data/snapshots?source=openweather&metric=temperature
```

SignalR `DataUpdate` events still aggregate **weather** metrics (openmeteo + openweather).

> **OpenWeather note:** `.env` is loaded automatically on startup. If OpenWeather returns 401, generate a new key at [openweathermap.org/api](https://openweathermap.org/api) — new keys can take up to 2 hours to activate.

## Manual observations (Phase 7)

Analyst+ can submit field measurements. Data is stored in `ManualObservations` and mirrored to `DataSnapshots` with `source=manual`.

```http
POST /api/observations
Authorization: Bearer <token>
Content-Type: application/json

{
  "region": "Wroclaw,PL",
  "metric": "temperature",
  "value": 22.5,
  "unit": "°C",
  "lat": 51.1,
  "lon": 17.0,
  "notes": "Field reading",
  "observedAt": "2026-06-07T14:00:00Z"
}
```

```http
GET /api/observations
Authorization: Bearer <token>
```

Charts can include manual data via:
```http
GET /api/data/snapshots?source=manual&metric=temperature
```

## Dashboards & widgets (Phase 8)

User-configurable dashboards with widgets for charts, metric cards, and maps.

### Dashboard endpoints (JWT)

| Method | Endpoint | Role |
|--------|----------|------|
| POST | `/api/dashboards` | Analyst+ |
| GET | `/api/dashboards` | Authenticated |
| GET | `/api/dashboards/{id}` | Owner or Admin |
| PUT | `/api/dashboards/{id}` | Owner (Analyst+) or Admin |
| DELETE | `/api/dashboards/{id}` | Owner (Analyst+) or Admin |

### Widget endpoints

| Method | Endpoint | Role |
|--------|----------|------|
| POST | `/api/dashboards/{id}/widgets` | Analyst+ |
| PUT | `/api/dashboards/{id}/widgets/{widgetId}` | Analyst+ |
| DELETE | `/api/dashboards/{id}/widgets/{widgetId}` | Analyst+ |

### Widget types

`LineChart`, `MetricCard`, `Map`

### Example: create dashboard + widget

```http
POST /api/dashboards
Authorization: Bearer <token>

{ "name": "Wrocław Monitor", "description": "Live weather panel" }
```

```http
POST /api/dashboards/{dashboardId}/widgets
Authorization: Bearer <token>

{
  "title": "Temperature chart",
  "type": "LineChart",
  "metric": "temperature",
  "region": "Wroclaw,PL",
  "source": "openmeteo",
  "configJson": "{\"color\":\"#3b82f6\"}",
  "sortOrder": 0
}
```

Frontend loads widget config from dashboard API, then fetches chart data from `/api/data/snapshots` using `metric`, `region`, and `source` from each widget.

## Admin & maintenance (Phase 9)

- **FluentValidation** on register, login, alerts, observations, dashboards, widgets
- **Snapshot cleanup** — deletes data older than 30 days (configurable in `Cleanup` section)
- **Docker** — full stack via `docker compose up -d`
- **Admin stats** — `GET /api/admin/stats` (users, snapshots, alerts, dashboards)

```http
GET /api/admin/stats
Authorization: Bearer <admin-token>
```

---

# Partner frontend specification

Backend is **complete**. Below is what the React frontend should implement.

## Tech stack (recommended)

- React 18 + TypeScript + Vite
- `@microsoft/signalr` for real-time
- React Router for pages
- Chart library (Recharts, Chart.js, or similar)
- Leaflet or Mapbox for map view

## Base URLs

| Service | URL |
|---------|-----|
| REST API | `http://localhost:5000` |
| SignalR Hub | `http://localhost:5000/hubs/dashboard` |
| Swagger | `http://localhost:5000/swagger` |

## Pages to build

### 1. Login / Register (Phase 4)
- `POST /api/auth/login` → store `token` in memory/localStorage
- `POST /api/auth/register` → auto-login
- `GET /api/auth/me` → current user + role
- Send header: `Authorization: Bearer <token>`

Dev admin: `admin@envirowatch.local` / `Admin123!`

### 2. Live monitoring / Charts (Phases 1–3) — **start here**
- **History:** `GET /api/data/snapshots?region=PL&metric=temperature&from=&to=`
- **Live:** SignalR event `DataUpdate`:
  ```json
  { "type": "weather", "region": "Wroclaw,PL", "data": { "temperature": 18.4, "humidity": 65, "wind": 12 }, "timestamp": "..." }
  ```
- Line charts for temperature, humidity, wind
- Optional: filter by `source` (openmeteo, openaq, opensky, manual)

### 3. Map view (Phase 6)
- Marker on Wrocław (51.1, 17.0)
- Show latest metrics in popup from `/api/data/snapshots`
- Optional: aircraft count from `source=opensky&metric=aircraft_count`

### 4. Alert config UI (Phase 5) — Analyst+
- List: `GET /api/alerts`
- Create: `POST /api/alerts` with `{ metric, region, threshold, condition, notifyEmail }`
- History: `GET /api/alerts/{id}/logs`
- Delete: `DELETE /api/alerts/{id}`

### 5. Manual observation form (Phase 7) — Analyst+
- `POST /api/observations` — field measurement form
- `GET /api/observations` — list user's entries

### 6. Dashboard builder (Phase 8) — Analyst+
- `GET /api/dashboards` — list user dashboards
- `POST /api/dashboards` — create panel
- `POST /api/dashboards/{id}/widgets` — add widget
- Widget types: `LineChart`, `MetricCard`, `Map`
- Render each widget using its `metric`, `region`, `source` against `/api/data/snapshots` + SignalR

### 7. Admin panel (Phase 9) — Admin only
- `GET /api/admin/stats` — overview cards
- `GET /api/admin/users` — user table
- `PUT /api/admin/users/{id}/role` — change role

## Role-based UI

| Role | Can do |
|------|--------|
| Viewer | View charts, dashboards (read-only) |
| Analyst | + create alerts, observations, dashboards |
| Admin | + user management, admin stats |

## Suggested build order for partner

```
1. Vite + React + routing + API client with JWT
2. Live charts page (REST history + SignalR)     ← unblocks demo
3. Login / Register
4. Dashboard builder
5. Alerts UI
6. Map view
7. Manual observations form
8. Admin panel (if Admin user available)
```

## TypeScript types (copy-paste ready)

```typescript
interface DataSnapshot {
  source: string;
  metric: string;
  value: number;
  unit: string | null;
  region: string;
  lat: number | null;
  lon: number | null;
  timestamp: string;
}

interface DataUpdate {
  type: "weather";
  region: string;
  data: { temperature: number; humidity: number; wind: number };
  timestamp: string;
}

interface AuthResponse {
  token: string;
  expiresAt: string;
  user: { id: string; email: string; role: "Admin" | "Analyst" | "Viewer" };
}

interface Dashboard {
  id: string;
  name: string;
  description: string | null;
  widgets: Widget[];
}

interface Widget {
  id: string;
  title: string;
  type: "LineChart" | "MetricCard" | "Map";
  metric: string;
  region: string;
  source: string | null;
  configJson: string | null;
  sortOrder: number;
}
```
