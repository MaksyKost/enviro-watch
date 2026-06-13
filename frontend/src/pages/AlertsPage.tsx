import { useEffect, useState, type FormEvent } from "react";
import { alertsApi } from "../api";
import { HttpError } from "../api/client";
import { ErrorBanner, Icon, LoadingState, StatusChip } from "../components/ui";
import { useAuth } from "../context/AuthContext";
import type { Alert, AlertLog } from "../types";
import { AlertCondition, CONDITION_LABELS, DEFAULT_REGION } from "../types";

const METRIC_ICONS: Record<string, { icon: string; color: string }> = {
  temperature: { icon: "water_drop", color: "text-secondary" },
  humidity: { icon: "water_drop", color: "text-secondary" },
  pm25: { icon: "co2", color: "text-error" },
  wind: { icon: "air", color: "text-outline" },
  aircraft_count: { icon: "flight", color: "text-surface-tint" },
};

function alertStatus(alert: Alert): { label: string; variant: "emerald" | "error" | "muted" | "triggered" } {
  if (!alert.isActive) return { label: "Muted", variant: "muted" };
  if (alert.lastTriggeredAt) {
    const hoursSince = (Date.now() - new Date(alert.lastTriggeredAt).getTime()) / 3_600_000;
    if (hoursSince < 24) return { label: "Triggered", variant: "triggered" };
  }
  return { label: "Active", variant: "emerald" };
}

function formatTime(iso: string): string {
  const date = new Date(iso);
  const now = new Date();
  if (now.toDateString() === date.toDateString()) {
    return date.toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" });
  }
  const yesterday = new Date(now);
  yesterday.setDate(yesterday.getDate() - 1);
  if (yesterday.toDateString() === date.toDateString()) return "Yesterday";
  return date.toLocaleDateString("en-GB", { day: "numeric", month: "short" });
}

export function AlertsPage() {
  const { token, isAnalyst } = useAuth();
  const [alerts, setAlerts] = useState<Alert[]>([]);
  const [history, setHistory] = useState<AlertLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({
    metric: "temperature",
    region: DEFAULT_REGION,
    threshold: "35",
    condition: AlertCondition.Above,
    notifyEmail: false,
  });

  async function loadAlerts() {
    if (!token) return;
    setLoading(true);
    try {
      const data = await alertsApi.list(token);
      setAlerts(data);
      setError(null);

      const logSets = await Promise.all(
        data.map((alert) => alertsApi.logs(token, alert.id).catch(() => [] as AlertLog[])),
      );
      const merged = logSets.flat().sort(
        (a, b) => new Date(b.triggeredAt).getTime() - new Date(a.triggeredAt).getTime(),
      );
      setHistory(merged.slice(0, 20));
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Failed to load alerts.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadAlerts();
  }, [token]);

  async function handleCreate(e: FormEvent) {
    e.preventDefault();
    if (!token || !isAnalyst) return;

    const threshold = Number(form.threshold);
    if (Number.isNaN(threshold)) {
      setError("Threshold must be a number.");
      return;
    }

    try {
      await alertsApi.create(token, {
        metric: form.metric,
        region: form.region,
        threshold,
        condition: form.condition,
        notifyEmail: form.notifyEmail,
      });
      setForm((f) => ({ ...f, threshold: "35" }));
      setShowForm(false);
      await loadAlerts();
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Failed to create alert.");
    }
  }

  async function handleDelete(alertId: string) {
    if (!token || !isAnalyst) return;
    if (!confirm("Delete this alert?")) return;
    try {
      await alertsApi.remove(token, alertId);
      await loadAlerts();
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Failed to delete alert.");
    }
  }

  const historyPreview = history;

  if (loading) return <LoadingState />;

  return (
    <div className="flex flex-col lg:flex-row gap-lg">
      <div className="flex-1 flex flex-col gap-md min-w-0">
        <div className="flex justify-between items-center mb-sm">
          <h2 className="font-headline-md text-headline-md text-on-surface">Alert Configurations</h2>
          {isAnalyst && (
            <button
              type="button"
              onClick={() => setShowForm((v) => !v)}
              className="bg-primary-container text-on-primary-container px-md py-sm rounded hover:opacity-90 transition-opacity font-label-sm text-label-sm flex items-center gap-xs"
            >
              <Icon name="add" size={16} />
              Create New Alert
            </button>
          )}
        </div>

        {error && <ErrorBanner message={error} />}

        {!isAnalyst && (
          <p className="text-sm text-on-surface-variant border border-outline-variant rounded p-sm">
            Alerts are private to each account. As Viewer you only see alerts you created yourself.
          </p>
        )}

        {showForm && isAnalyst && (
          <form onSubmit={handleCreate} className="surface-panel p-md grid grid-cols-1 md:grid-cols-3 gap-md items-end">
            <div>
              <label className="font-label-sm text-label-sm text-on-surface-variant uppercase">Metric</label>
              <select
                className="input-dark mt-1"
                value={form.metric}
                onChange={(e) => setForm({ ...form, metric: e.target.value })}
              >
                <option value="temperature">temperature</option>
                <option value="humidity">humidity</option>
                <option value="wind">wind</option>
                <option value="pm25">pm25</option>
                <option value="aircraft_count">aircraft_count</option>
              </select>
            </div>
            <div>
              <label className="font-label-sm text-label-sm text-on-surface-variant uppercase">Region</label>
              <input
                className="input-dark mt-1"
                value={form.region}
                onChange={(e) => setForm({ ...form, region: e.target.value })}
                required
              />
            </div>
            <div>
              <label className="font-label-sm text-label-sm text-on-surface-variant uppercase">Threshold</label>
              <input
                type="number"
                step="any"
                className="input-dark mt-1"
                value={form.threshold}
                onChange={(e) => setForm({ ...form, threshold: e.target.value })}
                required
              />
            </div>
            <div>
              <label className="font-label-sm text-label-sm text-on-surface-variant uppercase">Condition</label>
              <select
                className="input-dark mt-1"
                value={form.condition}
                onChange={(e) => setForm({ ...form, condition: Number(e.target.value) as AlertCondition })}
              >
                <option value={AlertCondition.Above}>{CONDITION_LABELS[AlertCondition.Above]}</option>
                <option value={AlertCondition.Below}>{CONDITION_LABELS[AlertCondition.Below]}</option>
              </select>
            </div>
            <label className="flex items-center gap-sm text-sm">
              <input
                type="checkbox"
                checked={form.notifyEmail}
                onChange={(e) => setForm({ ...form, notifyEmail: e.target.checked })}
              />
              Email notify
            </label>
            <button type="submit" className="btn-solid">Create alert</button>
          </form>
        )}

        <div className="surface-panel overflow-hidden flex flex-col">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead className="bg-slate-inset border-b border-slate-border">
                <tr>
                  <th className="p-md font-label-sm text-label-sm text-on-surface-variant uppercase">Metric</th>
                  <th className="p-md font-label-sm text-label-sm text-on-surface-variant uppercase">Region</th>
                  <th className="p-md font-label-sm text-label-sm text-on-surface-variant uppercase">Condition</th>
                  <th className="p-md font-label-sm text-label-sm text-on-surface-variant uppercase">Threshold</th>
                  <th className="p-md font-label-sm text-label-sm text-on-surface-variant uppercase">Status</th>
                  <th className="p-md font-label-sm text-label-sm text-on-surface-variant uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-border">
                {alerts.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="p-lg text-center text-on-surface-variant">
                      No alerts configured yet.
                    </td>
                  </tr>
                ) : (
                  alerts.map((alert) => {
                    const status = alertStatus(alert);
                    const meta = METRIC_ICONS[alert.metric] ?? { icon: "monitoring", color: "text-outline" };
                    return (
                      <tr key={alert.id} className="hover:bg-surface-variant transition-colors group">
                        <td className="p-md font-data-label text-data-label text-on-surface">
                          <span className="flex items-center gap-sm">
                            <Icon name={meta.icon} className={meta.color} size={16} />
                            {alert.metric}
                          </span>
                        </td>
                        <td className="p-md font-body-md text-body-md text-on-surface-variant">{alert.region}</td>
                        <td className="p-md font-data-label text-data-label text-on-surface-variant">
                          {CONDITION_LABELS[alert.condition]}
                        </td>
                        <td className="p-md font-data-display text-data-label text-on-surface">{alert.threshold}</td>
                        <td className="p-md">
                          <StatusChip label={status.label.toUpperCase()} variant={status.variant} />
                        </td>
                        <td className="p-md">
                          {isAnalyst ? (
                            <button
                              type="button"
                              className="text-outline hover:text-error transition-colors"
                              onClick={() => void handleDelete(alert.id)}
                              title="Delete alert"
                            >
                              <Icon name="delete" size={18} />
                            </button>
                          ) : (
                            <span className="text-on-surface-variant text-xs">—</span>
                          )}
                        </td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
          <div className="p-sm bg-slate-inset border-t border-slate-border flex justify-end">
            <span className="font-label-sm text-label-sm text-on-surface-variant">
              Showing {alerts.length} configuration{alerts.length === 1 ? "" : "s"}
            </span>
          </div>
        </div>
      </div>

      <div className="w-full lg:w-80 flex flex-col gap-md shrink-0">
        <h3 className="font-headline-sm text-headline-sm text-on-surface mb-sm">Trigger History</h3>
        <div className="surface-panel p-md flex flex-col gap-sm overflow-hidden min-h-[500px]">
          <div className="flex-1 overflow-y-auto pr-sm space-y-sm">
            {historyPreview.length === 0 ? (
              <p className="text-on-surface-variant text-sm">No triggers recorded yet.</p>
            ) : (
              historyPreview.map((log) => {
                const meta = METRIC_ICONS[log.metric] ?? { icon: "monitoring", color: "text-tertiary" };
                const isCritical = log.value > log.threshold;
                return (
                  <div
                    key={log.id}
                    className={`p-sm rounded border flex flex-col gap-xs ${
                      isCritical
                        ? "border-error/30 bg-error/5"
                        : "border-tertiary/30 bg-tertiary/5"
                    }`}
                  >
                    <div className="flex justify-between items-start">
                      <div className={`flex items-center gap-xs ${isCritical ? "text-error" : "text-tertiary"}`}>
                        <Icon name={meta.icon} size={14} />
                        <span className="font-label-sm text-label-sm">{log.metric}</span>
                      </div>
                      <span className="font-data-label text-[10px] text-on-surface-variant">
                        {formatTime(log.triggeredAt)}
                      </span>
                    </div>
                    <p className="font-body-md text-[13px] text-on-surface">
                      {log.region}: value {log.value} ({CONDITION_LABELS[log.condition]} {log.threshold})
                    </p>
                    <span className={`font-data-label text-[11px] ${isCritical ? "text-error" : "text-tertiary"}`}>
                      Threshold: {log.threshold}
                    </span>
                  </div>
                );
              })
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
