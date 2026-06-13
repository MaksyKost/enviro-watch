export enum UserRole {
  Viewer = 0,
  Analyst = 1,
  Admin = 2,
}

export enum AlertCondition {
  Above = 0,
  Below = 1,
}

export enum WidgetType {
  LineChart = 0,
  MetricCard = 1,
  Map = 2,
}

export interface User {
  id: string;
  email: string;
  role: UserRole;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: User;
}

export interface DataSnapshot {
  source: string;
  metric: string;
  value: number;
  unit: string | null;
  region: string;
  lat: number | null;
  lon: number | null;
  timestamp: string;
}

export interface DataSnapshotListResponse {
  items: DataSnapshot[];
  total: number;
  page: number;
  pageSize: number;
}

export interface DataUpdate {
  type: string;
  region: string;
  data: {
    temperature: number;
    humidity: number;
    wind: number;
  };
  timestamp: string;
}

export interface Alert {
  id: string;
  metric: string;
  region: string;
  threshold: number;
  condition: AlertCondition;
  notifyEmail: boolean;
  isActive: boolean;
  lastTriggeredAt: string | null;
  createdAt: string;
}

export interface AlertLog {
  id: string;
  alertId: string;
  metric: string;
  region: string;
  value: number;
  threshold: number;
  condition: AlertCondition;
  triggeredAt: string;
  emailSent: boolean;
}

export interface Observation {
  id: string;
  region: string;
  metric: string;
  value: number;
  unit: string | null;
  lat: number | null;
  lon: number | null;
  notes: string | null;
  observedAt: string;
  createdAt: string;
}

export interface Widget {
  id: string;
  dashboardId: string;
  title: string;
  type: WidgetType;
  metric: string;
  region: string;
  source: string | null;
  configJson: string | null;
  sortOrder: number;
  createdAt: string;
}

export interface Dashboard {
  id: string;
  name: string;
  description: string | null;
  createdAt: string;
  updatedAt: string;
  widgets: Widget[];
}

export interface AdminStats {
  users: number;
  snapshots: number;
  activeAlerts: number;
  dashboards: number;
  generatedAt: string;
}

export interface ApiError {
  error?: string;
  title?: string;
  errors?: Record<string, string[]>;
}

export const DEFAULT_REGION = "Wroclaw,PL";
export const DEFAULT_LAT = 51.1;
export const DEFAULT_LON = 17.0;

export const ROLE_LABELS: Record<UserRole, string> = {
  [UserRole.Viewer]: "Viewer",
  [UserRole.Analyst]: "Analyst",
  [UserRole.Admin]: "Admin",
};

export const CONDITION_LABELS: Record<AlertCondition, string> = {
  [AlertCondition.Above]: "Above",
  [AlertCondition.Below]: "Below",
};

export const WIDGET_TYPE_LABELS: Record<WidgetType, string> = {
  [WidgetType.LineChart]: "Line Chart",
  [WidgetType.MetricCard]: "Metric Card",
  [WidgetType.Map]: "Map",
};

export function canManage(role: UserRole): boolean {
  return role >= UserRole.Analyst;
}

export function isAdmin(role: UserRole): boolean {
  return role === UserRole.Admin;
}
