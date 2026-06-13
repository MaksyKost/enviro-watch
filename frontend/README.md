# EnviroWatch Frontend

React + TypeScript + Vite client. Talks to the ASP.NET Core backend via REST and SignalR.

Full-stack setup (Docker vs local dev, API URLs, credentials): see the [root README](../README.md).

## Project layout

```
frontend/
├── design-reference/     # HTML mockups — visual reference only
├── src/
│   ├── api/              # REST client
│   ├── components/
│   │   ├── layout/       # AppLayout, ProtectedRoute
│   │   ├── RegionMap.tsx
│   │   └── ui.tsx
│   ├── config/           # Navigation
│   ├── context/          # Auth (JWT in localStorage)
│   ├── hooks/            # SignalR
│   ├── pages/
│   └── types/
├── Dockerfile            # Production build → nginx
└── nginx.conf            # Proxies /api, /hubs, /health to API container
```

## How the frontend reaches the API

This is the main difference between Docker and local dev:

| Mode | URL you open | `VITE_API_URL` | Request path |
|------|--------------|----------------|--------------|
| **Docker** | http://localhost:3000 | empty (set at Docker build) | Browser → nginx `:3000/api/...` → `api:8080` |
| **Local dev** | http://localhost:5173 | `http://localhost:5000` (default in `.env.example`) | Browser → API `:5000` directly |
| **Local dev (alt)** | http://localhost:5173 | empty | Browser → Vite `:5173/api/...` → proxy → `:5000` |

The API client (`src/api/client.ts`) and SignalR hook (`src/hooks/useSignalR.ts`) both use:

```typescript
import.meta.env.VITE_API_URL ?? ""
```

When empty, paths are relative to the page origin — same behaviour as production behind nginx.

## Development

Requires a running API at http://localhost:5000 (see root README).

```bash
cd frontend
cp .env.example .env
npm install
npm run dev
```

Open http://localhost:5173

## Production build (Docker)

Built from repo root — do not set `VITE_API_URL` for the Docker image:

```bash
docker compose up -d --build frontend
```

Open http://localhost:3000

## Pages

| Route | Description | Auth |
|-------|-------------|------|
| `/` | Live dashboard — metrics, chart, SignalR feed, map | Public |
| `/login` | Login & register | Public |
| `/dashboards` | Custom dashboard builder | JWT |
| `/alerts` | Threshold alerts + trigger history | JWT |
| `/observations` | Manual field measurements | JWT (submit: Analyst+) |
| `/admin` | Stats & user management | Admin |

Dev admin: `admin@envirowatch.local` / `Admin123!`

## Stack

- React 19, TypeScript, Vite
- React Router, Tailwind CSS
- Recharts, Leaflet
- `@microsoft/signalr`

Design tokens: `design-reference/enviro_watch/DESIGN.md`

## Build locally

```bash
npm run build    # output in dist/
npm run preview  # serve dist/ for smoke test
```
