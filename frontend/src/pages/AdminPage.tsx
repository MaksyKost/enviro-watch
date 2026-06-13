import { useEffect, useState } from "react";
import { adminApi } from "../api";
import { HttpError } from "../api/client";
import { ErrorBanner, Icon, LoadingState, StatusChip } from "../components/ui";
import { getInitials } from "../config/navigation";
import { useAuth } from "../context/AuthContext";
import type { AdminStats, User } from "../types";
import { ROLE_LABELS, UserRole } from "../types";

export function AdminPage() {
  const { token } = useAuth();
  const [stats, setStats] = useState<AdminStats | null>(null);
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState("");

  async function load() {
    if (!token) return;
    setLoading(true);
    try {
      const [statsData, usersData] = await Promise.all([
        adminApi.stats(token),
        adminApi.users(token),
      ]);
      setStats(statsData);
      setUsers(usersData);
      setError(null);
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Failed to load admin data.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, [token]);

  async function changeRole(userId: string, role: UserRole) {
    if (!token) return;
    try {
      await adminApi.updateRole(token, userId, role);
      await load();
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Failed to update role.");
    }
  }

  const filteredUsers = users.filter((u) =>
    u.email.toLowerCase().includes(filter.toLowerCase()),
  );

  if (loading) return <LoadingState />;

  return (
    <>
      <div className="mb-lg">
        <h1 className="font-headline-lg text-headline-lg text-on-surface mb-unit">
          System Health & Management
        </h1>
        <p className="font-body-md text-body-md text-on-surface-variant">
          Real-time overview of system performance and user access control.
        </p>
      </div>

      {error && <ErrorBanner message={error} />}

      {stats && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-md mb-lg">
          <div className="surface-panel p-md flex flex-col gap-sm">
            <div className="flex justify-between items-start">
              <span className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-widest">
                Total Snapshots
              </span>
              <Icon name="photo_library" className="text-primary" />
            </div>
            <div className="font-data-display text-data-display text-on-surface text-3xl">
              {stats.snapshots >= 1000
                ? `${(stats.snapshots / 1000).toFixed(1)}k`
                : stats.snapshots.toLocaleString()}
            </div>
            <div className="flex items-center gap-unit mt-auto">
              <StatusChip label="↑ Live" variant="emerald" />
              <span className="font-data-label text-data-label text-on-surface-variant">
                {stats.dashboards} dashboards
              </span>
            </div>
          </div>

          <div className="surface-panel p-md flex flex-col gap-sm">
            <div className="flex justify-between items-start">
              <span className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-widest">
                Active Users
              </span>
              <Icon name="group" className="text-secondary" />
            </div>
            <div className="font-data-display text-data-display text-on-surface text-3xl">{stats.users}</div>
            <span className="font-data-label text-data-label text-on-surface-variant mt-auto">
              Registered accounts
            </span>
          </div>

          <div className="surface-panel p-md flex flex-col gap-sm">
            <div className="flex justify-between items-start">
              <span className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-widest">
                Active Alerts
              </span>
              <Icon name="warning" className="text-error" />
            </div>
            <div className="font-data-display text-data-display text-on-surface text-3xl">
              {stats.activeAlerts}
            </div>
            <div className="flex items-center gap-unit mt-auto">
              {stats.activeAlerts > 0 ? (
                <StatusChip label={`${stats.activeAlerts} Active`} variant="amber" />
              ) : (
                <StatusChip label="All clear" variant="emerald" />
              )}
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-md">
        <div className="surface-panel lg:col-span-2 flex flex-col">
          <div className="p-md border-b border-slate-border flex justify-between items-center gap-md flex-wrap">
            <h2 className="font-headline-sm text-headline-sm text-on-surface">User Management</h2>
            <div className="relative">
              <Icon
                name="search"
                className="absolute left-sm top-1/2 -translate-y-1/2 text-on-surface-variant text-[18px]"
              />
              <input
                className="input-dark pl-xl pr-sm py-unit w-48 text-sm"
                placeholder="Filter users..."
                value={filter}
                onChange={(e) => setFilter(e.target.value)}
              />
            </div>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="border-b border-slate-border bg-slate-row">
                  <th className="p-sm font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider w-1/3">
                    User
                  </th>
                  <th className="p-sm font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider w-1/4">
                    Status
                  </th>
                  <th className="p-sm font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider w-1/4">
                    Role
                  </th>
                </tr>
              </thead>
              <tbody className="font-body-md text-body-md text-on-surface divide-y divide-slate-border">
                {filteredUsers.map((user) => {
                  const displayName = user.email.split("@")[0].replace(/[._]/g, " ");
                  return (
                    <tr key={user.id} className="hover:bg-slate-row transition-colors">
                      <td className="p-sm">
                        <div className="flex items-center gap-sm">
                          <div className="w-8 h-8 rounded-full bg-surface flex items-center justify-center border border-outline-variant shrink-0">
                            <span className="font-data-label text-data-label text-primary text-[10px]">
                              {getInitials(displayName)}
                            </span>
                          </div>
                          <div>
                            <div className="font-medium capitalize">{displayName}</div>
                            <div className="font-data-label text-data-label text-on-surface-variant">
                              {user.email}
                            </div>
                          </div>
                        </div>
                      </td>
                      <td className="p-sm">
                        <StatusChip label="Active" variant="emerald" />
                      </td>
                      <td className="p-sm">
                        <select
                          className="input-dark text-xs py-1 w-full"
                          value={user.role}
                          onChange={(e) => void changeRole(user.id, Number(e.target.value) as UserRole)}
                        >
                          {Object.entries(ROLE_LABELS).map(([value, label]) => (
                            <option key={value} value={value}>
                              {label}
                            </option>
                          ))}
                        </select>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>

        <div className="flex flex-col gap-md">
          <div className="surface-panel p-md flex flex-col">
            <div className="flex justify-between items-center mb-md">
              <h2 className="font-headline-sm text-headline-sm text-on-surface">Platform Overview</h2>
              <Icon name="storage" className="text-on-surface-variant text-[20px]" />
            </div>
            {stats && (
              <dl className="space-y-sm font-body-md text-body-md">
                <div className="flex justify-between border-b border-slate-border pb-sm">
                  <dt className="text-on-surface-variant">Snapshots stored</dt>
                  <dd className="font-data-label text-data-label">{stats.snapshots.toLocaleString()}</dd>
                </div>
                <div className="flex justify-between border-b border-slate-border pb-sm">
                  <dt className="text-on-surface-variant">Dashboards</dt>
                  <dd className="font-data-label text-data-label">{stats.dashboards}</dd>
                </div>
                <div className="flex justify-between">
                  <dt className="text-on-surface-variant">Active alerts</dt>
                  <dd className="font-data-label text-data-label">{stats.activeAlerts}</dd>
                </div>
              </dl>
            )}
            <p className="font-body-md text-body-md text-on-surface-variant mt-md pt-md border-t border-outline-variant border-dashed">
              Snapshot retention cleanup runs automatically on the server schedule.
            </p>
          </div>
        </div>
      </div>
    </>
  );
}
