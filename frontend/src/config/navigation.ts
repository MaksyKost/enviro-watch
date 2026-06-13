import { UserRole } from "../types";

export type PageLayout = "default" | "canvas";

export interface NavItem {
  to: string;
  label: string;
  icon: string;
  pageTitle?: string;
  layout?: PageLayout;
  public?: boolean;
  minRole?: UserRole;
  description?: string;
}

export const NAV_ITEMS: NavItem[] = [
  {
    to: "/",
    label: "Dashboard",
    icon: "dashboard",
    public: true,
    description: "Live metrics, charts, SignalR feed",
  },
  {
    to: "/dashboards",
    label: "Panels",
    icon: "grid_view",
    pageTitle: "Dashboard Builder",
    layout: "canvas",
    minRole: UserRole.Viewer,
    description: "Custom dashboard builder",
  },
  {
    to: "/alerts",
    label: "Alerts",
    icon: "warning",
    pageTitle: "Alerts Management",
    minRole: UserRole.Viewer,
    description: "Threshold alerts & history",
  },
  {
    to: "/observations",
    label: "Observations",
    icon: "visibility",
    pageTitle: "Manual Observations",
    minRole: UserRole.Viewer,
    description: "Manual field measurements",
  },
  {
    to: "/admin",
    label: "Admin",
    icon: "admin_panel_settings",
    pageTitle: "Admin Control Center",
    minRole: UserRole.Admin,
    description: "Users, stats, roles",
  },
];

export const PAGE_TITLES: Record<string, string | undefined> = Object.fromEntries(
  NAV_ITEMS.map((item) => [item.to, item.pageTitle]),
);

export const PAGE_LAYOUTS: Record<string, PageLayout> = Object.fromEntries(
  NAV_ITEMS.filter((item) => item.layout).map((item) => [item.to, item.layout!]),
);

export function canAccessNav(
  item: NavItem,
  role: UserRole | null,
  isLoggedIn: boolean,
): boolean {
  if (item.public) return true;
  if (!isLoggedIn || role === null) return false;
  if (item.minRole === UserRole.Admin) return role === UserRole.Admin;
  return role >= (item.minRole ?? UserRole.Viewer);
}

export function getInitials(name: string): string {
  return name
    .split(/[\s._-]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");
}
