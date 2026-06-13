import { apiRequest, authHeaders } from "./client";
import type {
  AdminStats,
  Alert,
  AlertLog,
  AuthResponse,
  Dashboard,
  DataSnapshotListResponse,
  Observation,
  User,
  Widget,
} from "../types";
import { AlertCondition, WidgetType } from "../types";

export const authApi = {
  login(email: string, password: string) {
    return apiRequest<AuthResponse>("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
  },

  register(email: string, password: string) {
    return apiRequest<AuthResponse>("/api/auth/register", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
  },

  me(token: string) {
    return apiRequest<User>("/api/auth/me", {
      headers: authHeaders(token),
    });
  },
};

export const dataApi = {
  snapshots(params: {
    region?: string;
    metric?: string;
    source?: string;
    from?: string;
    to?: string;
    page?: number;
    pageSize?: number;
  }) {
    return apiRequest<DataSnapshotListResponse>("/api/data/snapshots", {}, params);
  },
};

export const alertsApi = {
  list(token: string) {
    return apiRequest<Alert[]>("/api/alerts", { headers: authHeaders(token) });
  },

  create(
    token: string,
    payload: {
      metric: string;
      region: string;
      threshold: number;
      condition: AlertCondition;
      notifyEmail: boolean;
    },
  ) {
    return apiRequest<Alert>("/api/alerts", {
      method: "POST",
      headers: authHeaders(token),
      body: JSON.stringify(payload),
    });
  },

  logs(token: string, alertId: string) {
    return apiRequest<AlertLog[]>(`/api/alerts/${alertId}/logs`, {
      headers: authHeaders(token),
    });
  },

  remove(token: string, alertId: string) {
    return apiRequest<void>(`/api/alerts/${alertId}`, {
      method: "DELETE",
      headers: authHeaders(token),
    });
  },
};

export const observationsApi = {
  list(token: string) {
    return apiRequest<Observation[]>("/api/observations", {
      headers: authHeaders(token),
    });
  },

  create(
    token: string,
    payload: {
      region: string;
      metric: string;
      value: number;
      unit?: string;
      lat?: number;
      lon?: number;
      notes?: string;
      observedAt?: string;
    },
  ) {
    return apiRequest<Observation>("/api/observations", {
      method: "POST",
      headers: authHeaders(token),
      body: JSON.stringify(payload),
    });
  },
};

export const dashboardsApi = {
  list(token: string) {
    return apiRequest<Dashboard[]>("/api/dashboards", {
      headers: authHeaders(token),
    });
  },

  get(token: string, id: string) {
    return apiRequest<Dashboard>(`/api/dashboards/${id}`, {
      headers: authHeaders(token),
    });
  },

  create(token: string, name: string, description?: string) {
    return apiRequest<Dashboard>("/api/dashboards", {
      method: "POST",
      headers: authHeaders(token),
      body: JSON.stringify({ name, description }),
    });
  },

  update(token: string, id: string, name: string, description?: string) {
    return apiRequest<Dashboard>(`/api/dashboards/${id}`, {
      method: "PUT",
      headers: authHeaders(token),
      body: JSON.stringify({ name, description }),
    });
  },

  remove(token: string, id: string) {
    return apiRequest<void>(`/api/dashboards/${id}`, {
      method: "DELETE",
      headers: authHeaders(token),
    });
  },

  addWidget(
    token: string,
    dashboardId: string,
    payload: {
      title: string;
      type: WidgetType;
      metric: string;
      region: string;
      source?: string;
      configJson?: string;
      sortOrder?: number;
    },
  ) {
    return apiRequest<Widget>(`/api/dashboards/${dashboardId}/widgets`, {
      method: "POST",
      headers: authHeaders(token),
      body: JSON.stringify(payload),
    });
  },

  removeWidget(token: string, dashboardId: string, widgetId: string) {
    return apiRequest<void>(`/api/dashboards/${dashboardId}/widgets/${widgetId}`, {
      method: "DELETE",
      headers: authHeaders(token),
    });
  },
};

export const adminApi = {
  stats(token: string) {
    return apiRequest<AdminStats>("/api/admin/stats", {
      headers: authHeaders(token),
    });
  },

  users(token: string) {
    return apiRequest<User[]>("/api/admin/users", {
      headers: authHeaders(token),
    });
  },

  updateRole(token: string, userId: string, role: number) {
    return apiRequest<void>(`/api/admin/users/${userId}/role`, {
      method: "PUT",
      headers: authHeaders(token),
      body: JSON.stringify({ role }),
    });
  },
};

export const healthApi = {
  check() {
    return apiRequest<{ status: string; timestamp: string }>("/health");
  },
};
