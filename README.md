# EnviroWatch

Real-time environmental monitoring platform. ASP.NET Core backend aggregates weather, air quality, and air traffic data; React frontend provides live dashboards, alerts, custom panels, and admin tools.

## Project structure

```
enviro-watch/
├── backend/            # ASP.NET Core 8 API
├── frontend/           # React + Vite + TypeScript
├── docker-compose.yml  # Full stack (postgres + api + frontend)
└── .env.example        # Shared environment template
```

## Prerequisites

| Tool | Version | Needed for |
|------|---------|------------|
| Docker | recent | Full stack, or PostgreSQL only |
| .NET SDK | 8 | Running API outside Docker |
| Node.js | 20+ | Frontend dev server (`npm run dev`) |

---

## How to run

There are two common setups. **They use different API wiring on the frontend** — read the table below before debugging CORS or 404 errors.

### Option A — Full stack in Docker (simplest)

Everything runs in containers. The browser never talks to the API directly.

```bash
cp .env.example .env
docker compose up -d --build
```

| Service | URL | Notes |
|---------|-----|-------|
| Frontend | http://localhost:3000 | nginx serves SPA, proxies `/api`, `/hubs`, `/health` to the API container |
| API (direct) | http://localhost:5000 | Swagger, health checks, manual API testing |
| PostgreSQL | localhost:5432 | Credentials from `.env` |

**Frontend → API in Docker:** requests go to the same origin (`localhost:3000/api/...`). nginx forwards them to `api:8080` inside the Docker network. The frontend build sets `VITE_API_URL=` (empty) on purpose.

### Option B — Local development (mixed)

Typical day-to-day setup: PostgreSQL in Docker, API and frontend on the host.

```bash
# 1. Environment
cp .env.example .env

# 2. Database only
docker compose up -d postgres

# 3. Backend
cd backend
dotnet restore
dotnet run --project src/API

# 4. Frontend (separate terminal)
cd frontend
cp .env.example .env
npm install
npm run dev
```

| Service | URL | Notes |
|---------|-----|-------|
| Frontend (Vite) | http://localhost:5173 | Dev server with hot reload |
| API | http://localhost:5000 | `dotnet run`, reads `.env` from repo root |
| PostgreSQL | localhost:5432 | Docker container |

**Frontend → API locally:** two valid approaches:

| `frontend/.env` | How requests reach the API |
|-----------------|----------------------------|
| `VITE_API_URL=http://localhost:5000` | Browser calls API directly (CORS must allow `:5173`) |
| `VITE_API_URL=` (empty) | Browser calls `:5173/api/...`, Vite proxy forwards to `:5000` |

Default in `.env.example` is the direct URL. Both work; empty URL is closer to the Docker/production behaviour.

### Docker vs local — quick comparison

| | Docker stack | Local dev |
|---|-------------|-----------|
| Frontend URL | `:3000` | `:5173` |
| API URL (browser) | same origin via nginx proxy | `:5000` direct or via Vite proxy |
| API URL (server) | container `api:8080` | `localhost:5000` |
| DB host (from API) | `postgres` (Docker network) | `localhost` |
| Frontend env | `VITE_API_URL=` at build time | `frontend/.env` at dev time |
| Rebuild after frontend changes | `docker compose up -d --build frontend` | automatic (HMR) |

### Verify the stack

```bash
curl http://localhost:5000/health
```

Open Swagger: http://localhost:5000/swagger

---

## Default admin account

Seeded automatically on first API startup (Development):

| Field | Value |
|-------|-------|
| Email | `admin@envirowatch.local` |
| Password | `Admin123!` |

---

## Frontend

React app lives in [`frontend/`](frontend/). See [`frontend/README.md`](frontend/README.md) for page routes, stack, and frontend-specific setup.

| Route | Description | Auth |
|-------|-------------|------|
| `/` | Live dashboard — metrics, chart, SignalR feed, map | Public |
| `/login` | Login & register | Public |
| `/dashboards` | Custom dashboard builder | JWT |
| `/alerts` | Threshold alerts + trigger history | JWT |
| `/observations` | Manual field measurements | JWT (submit: Analyst+) |
| `/admin` | Stats & user management | Admin |

Design mockups (reference only): `frontend/design-reference/`

---

## Configuration

Copy `.env.example` to `.env` in the repo root:

```bash
cp .env.example .env
```

| Variable | Used by | Purpose |
|----------|---------|---------|
| `POSTGRES_*` | Docker postgres + local API | Database credentials |
| `ConnectionStrings__DefaultConnection` | Local API (`dotnet run`) | Points to `localhost:5432` |
| `JWT_SECRET` | API | JWT signing key (min 32 chars) |
| `OPENWEATHER_API_KEY` | API | Optional; without it OpenWeather source is skipped |

Data fetch regions are configured in `backend/src/API/appsettings.json`:

```json
"DataFetch": {
  "IntervalSeconds": 30,
  "Regions": [
    { "Name": "Wroclaw,PL", "Latitude": 51.1, "Longitude": 17.0 }
  ]
}
```

In Development, the API seeds ~147 sample snapshots (48h history) if the database is empty.

---

## Authentication

JWT bearer auth with three roles: **Admin**, **Analyst**, **Viewer**.

```http
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me              # requires token
```

Login example:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@envirowatch.local",
  "password": "Admin123!"
}
```

Use the token: `Authorization: Bearer <token>`

| Role | Access |
|------|--------|
| Viewer | Read dashboards, alerts, observations (own data) |
| Analyst | + create alerts, observations, dashboards |
| Admin | + user management, platform stats |

`GET /api/data/snapshots` and the SignalR hub remain **public** so the live dashboard works without login.

---

## REST API overview

Base URL: `http://localhost:5000` (always, whether API runs in Docker or locally).

### Data snapshots

```http
GET /api/data/snapshots?region=Wroclaw,PL&metric=temperature&from=2026-06-01&page=1&pageSize=50
GET /api/data/snapshots?source=openmeteo&metric=temperature
GET /api/data/snapshots?source=opensky&metric=aircraft_count
GET /api/data/snapshots?source=manual&metric=temperature
```

Response shape:

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

### External data sources

Fetched in parallel every 30 seconds:

| Source | Metrics | API key |
|--------|---------|---------|
| `openmeteo` | temperature, humidity, wind | No |
| `openweather` | temperature, humidity, wind | Yes (`OPENWEATHER_API_KEY`) |
| `openaq` | pm25, pm10, aqi | No |
| `opensky` | aircraft_count, avg_altitude | No |

> OpenWeather keys can take up to 2 hours to activate after creation at [openweathermap.org/api](https://openweathermap.org/api).

### SignalR live updates

**Hub:** `http://localhost:5000/hubs/dashboard` (or same-origin `/hubs/dashboard` through frontend proxy)

**Event:** `DataUpdate`

```json
{
  "type": "weather",
  "region": "Wroclaw,PL",
  "data": { "temperature": 18.4, "humidity": 65, "wind": 12 },
  "timestamp": "2026-06-07T12:00:30Z"
}
```

Hub methods: `SubscribeToRegion`, `UnsubscribeFromRegion` (optional region filter).

### Alerts

Background checker runs every 30s with a 5-minute cooldown per alert.

| Method | Endpoint | Role |
|--------|----------|------|
| POST | `/api/alerts` | Analyst+ |
| GET | `/api/alerts` | Authenticated (own alerts) |
| GET | `/api/alerts/{id}/logs` | Owner or Admin |
| DELETE | `/api/alerts/{id}` | Owner (Analyst+) or Admin |

```json
{
  "metric": "temperature",
  "region": "Wroclaw,PL",
  "threshold": 35.0,
  "condition": "Above",
  "notifyEmail": true
}
```

`condition`: `Above` or `Below`

### Manual observations

Analyst+ submit field measurements. Stored in `ManualObservations` and mirrored to snapshots with `source=manual`.

```http
POST /api/observations
GET  /api/observations
```

### Dashboards & widgets

| Method | Endpoint | Role |
|--------|----------|------|
| POST/GET/PUT/DELETE | `/api/dashboards` | Authenticated |
| POST/PUT/DELETE | `/api/dashboards/{id}/widgets` | Analyst+ |

Widget types: `LineChart`, `MetricCard`, `Map`

### Admin

| Method | Endpoint | Role |
|--------|----------|------|
| GET | `/api/admin/stats` | Admin |
| GET | `/api/admin/users` | Admin |
| PUT | `/api/admin/users/{id}/role` | Admin |

Snapshot cleanup (data older than 30 days) runs automatically as a background service — there is no manual trigger endpoint.

---

## Running tests

```bash
cd backend
dotnet test
```

---

## Troubleshooting

| Symptom | Likely cause |
|---------|--------------|
| Login works but `/api/auth/me` returns 401 | Stale token in browser — clear `localStorage` key `envirowatch_token` |
| Frontend 404 on `/api/...` in Docker | Rebuild frontend: `docker compose up -d --build frontend` |
| Frontend CORS error on `:5173` | Set `VITE_API_URL=` empty to use Vite proxy, or ensure API CORS allows `:5173` |
| Port 5000 already in use | Another process or old `envirowatch-api` container — `docker compose down` or change port |
| `relation "Dashboards" does not exist` | Run API once to apply EF migrations, or recreate DB volume |
| SignalR connected but no events | Check API logs for fetch/mapping errors; both openmeteo and openweather send `temperature` |
