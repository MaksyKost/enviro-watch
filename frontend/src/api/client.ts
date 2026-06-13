import type { ApiError } from "../types";

const API_BASE = import.meta.env.VITE_API_URL ?? "";

export class HttpError extends Error {
  status: number;
  body: ApiError | null;

  constructor(status: number, message: string, body: ApiError | null = null) {
    super(message);
    this.status = status;
    this.body = body;
  }
}

function buildUrl(path: string, params?: Record<string, string | number | undefined>): string {
  const url = new URL(`${API_BASE}${path}`, window.location.origin);
  if (params) {
    for (const [key, value] of Object.entries(params)) {
      if (value !== undefined && value !== "") {
        url.searchParams.set(key, String(value));
      }
    }
  }
  return url.toString();
}

export async function apiRequest<T>(
  path: string,
  options: RequestInit = {},
  params?: Record<string, string | number | undefined>,
): Promise<T> {
  const headers = new Headers(options.headers);
  if (options.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(buildUrl(path, params), {
    ...options,
    headers,
  });

  if (response.status === 204) {
    return undefined as T;
  }

  const text = await response.text();
  const body = text ? (JSON.parse(text) as ApiError | T) : null;

  if (!response.ok) {
    const errorBody = body as ApiError | null;
    const message =
      errorBody?.error ??
      errorBody?.title ??
      Object.values(errorBody?.errors ?? {})
        .flat()
        .join(", ") ??
      response.statusText;
    throw new HttpError(response.status, message, errorBody);
  }

  return body as T;
}

export function authHeaders(token: string | null): HeadersInit {
  return token ? { Authorization: `Bearer ${token}` } : {};
}
