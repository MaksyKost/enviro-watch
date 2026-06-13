import { useEffect, useState, type FormEvent } from "react";
import { dashboardsApi, dataApi } from "../api";
import { HttpError } from "../api/client";
import { RegionMap } from "../components/RegionMap";
import { ErrorBanner, Icon, LoadingState } from "../components/ui";
import { useAuth } from "../context/AuthContext";
import type { Dashboard, DataSnapshot, Widget } from "../types";
import { DEFAULT_REGION, WidgetType, WIDGET_TYPE_LABELS } from "../types";
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

const CHART_HEIGHT = 192;

function WidgetRenderer({ widget, editing }: { widget: Widget; editing?: boolean }) {
  const [snapshots, setSnapshots] = useState<DataSnapshot[]>([]);

  useEffect(() => {
    const from = new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString();
    dataApi
      .snapshots({
        region: widget.region,
        metric: widget.metric,
        source: widget.source ?? undefined,
        from,
        pageSize: 100,
      })
      .then((res) => setSnapshots(res.items))
      .catch(() => setSnapshots([]));
  }, [widget.id, widget.region, widget.metric, widget.source]);

  const latest = snapshots.sort(
    (a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime(),
  )[0];

  const shell = editing
    ? "surface-panel border-2 border-primary-container shadow-[0_0_15px_rgba(96,165,250,0.15)] relative"
    : "surface-panel";

  if (widget.type === WidgetType.MetricCard) {
    return (
      <div className={`${shell} p-md flex flex-col h-[200px]`}>
        {editing && (
          <div className="absolute -top-3 -right-3 bg-primary-container text-on-primary-container px-2 py-1 font-label-sm text-[9px] uppercase border border-slate-inset">
            Editing
          </div>
        )}
        <div className="flex justify-between items-center mb-md shrink-0">
          <span className="font-label-sm text-label-sm text-on-surface-variant uppercase truncate pr-2">
            {widget.title}
          </span>
          <Icon name="edit" className="text-primary-container text-[16px]" />
        </div>
        <div className="flex-1 flex flex-col justify-center items-center border border-dashed border-slate-border bg-slate-inset/50">
          <span className="font-data-display text-[48px] text-primary font-medium mb-1">
            {latest?.value ?? "—"}
          </span>
          <span className="font-data-label text-data-label text-on-surface-variant">
            {latest?.unit ?? widget.metric}
          </span>
        </div>
      </div>
    );
  }

  if (widget.type === WidgetType.Map) {
    return (
      <div className={`${shell} p-md flex flex-col h-[240px]`}>
        <div className="flex justify-between items-center mb-md shrink-0">
          <span className="font-label-sm text-label-sm text-on-surface-variant uppercase">{widget.title}</span>
        </div>
        <div className="h-[180px] shrink-0">
          <RegionMap
            region={widget.region}
            lat={latest?.lat ?? undefined}
            lon={latest?.lon ?? undefined}
            metrics={latest ? [{ label: widget.metric, value: String(latest.value) }] : []}
          />
        </div>
      </div>
    );
  }

  const chartData = [...snapshots]
    .sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime())
    .map((s) => ({
      time: new Date(s.timestamp).toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" }),
      value: s.value,
    }));

  return (
    <div className={`${shell} p-md flex flex-col h-[260px]`}>
      <div className="flex justify-between items-center mb-md shrink-0">
        <span className="font-label-sm text-label-sm text-on-surface-variant uppercase">{widget.title}</span>
      </div>
      <div className="shrink-0 border border-outline-variant/30" style={{ height: CHART_HEIGHT }}>
        <ResponsiveContainer width="100%" height={CHART_HEIGHT}>
          <LineChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" stroke="#414751" />
            <XAxis dataKey="time" stroke="#8b919d" tick={{ fontSize: 10 }} />
            <YAxis stroke="#8b919d" tick={{ fontSize: 10 }} />
            <Tooltip contentStyle={{ background: "#1d2025", border: "1px solid #414751" }} />
            <Line type="monotone" dataKey="value" stroke="#60a5fa" strokeWidth={2} dot={false} />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}

export function DashboardsPage() {
  const { token, isAnalyst } = useAuth();
  const [dashboards, setDashboards] = useState<Dashboard[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [newName, setNewName] = useState("");
  const [widgetForm, setWidgetForm] = useState({
    title: "Temperature chart",
    type: WidgetType.LineChart,
    metric: "temperature",
    region: DEFAULT_REGION,
    source: "openmeteo",
  });

  const selected = dashboards.find((d) => d.id === selectedId) ?? dashboards[0];
  const sortedWidgets = selected?.widgets.slice().sort((a, b) => a.sortOrder - b.sortOrder) ?? [];

  async function load(silent = false) {
    if (!token) return;
    if (!silent) setLoading(true);
    try {
      const data = await dashboardsApi.list(token);
      setDashboards(data);
      setSelectedId((current) => current ?? data[0]?.id ?? null);
      setError(null);
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Failed to load dashboards.");
    } finally {
      if (!silent) setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, [token]);

  async function createDashboard(e: FormEvent) {
    e.preventDefault();
    if (!token || !isAnalyst || !newName.trim()) return;
    try {
      const created = await dashboardsApi.create(token, newName.trim());
      setNewName("");
      setSelectedId(created.id);
      await load(true);
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Failed to create dashboard.");
    }
  }

  async function addWidget(e: FormEvent) {
    e.preventDefault();
    if (!token || !isAnalyst || !selected) return;
    try {
      await dashboardsApi.addWidget(token, selected.id, widgetForm);
      await load(true);
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Failed to add widget.");
    }
  }

  async function removeWidget(widgetId: string) {
    if (!token || !isAnalyst || !selected) return;
    try {
      await dashboardsApi.removeWidget(token, selected.id, widgetId);
      await load(true);
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Failed to remove widget.");
    }
  }

  if (loading) return <LoadingState label="Loading panels…" />;

  return (
    <div className="flex flex-1 min-h-0 overflow-hidden">
      <div className="flex-1 grid-bg overflow-y-auto p-lg min-h-0">
        <div className="max-w-5xl mx-auto">
          <div className="flex flex-wrap items-center justify-between gap-md mb-lg">
            <h2 className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">
              Live Preview Canvas
            </h2>
            {dashboards.length > 0 && (
              <select
                className="input-dark w-auto text-sm"
                value={selected?.id ?? ""}
                onChange={(e) => setSelectedId(e.target.value)}
              >
                {dashboards.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.name}
                  </option>
                ))}
              </select>
            )}
          </div>

          {error && (
            <div className="mb-md">
              <ErrorBanner message={error} />
            </div>
          )}

          {!selected ? (
            <div className="surface-panel p-xl text-center text-on-surface-variant">
              {isAnalyst ? "Create a panel using the configuration sidebar." : "No panels available."}
            </div>
          ) : (
            <div className="grid grid-cols-12 gap-md auto-rows-max">
              {sortedWidgets.length === 0 ? (
                <div className="col-span-12 surface-panel p-lg text-center text-on-surface-variant">
                  No widgets yet. Configure one in the sidebar and click Apply.
                </div>
              ) : (
                sortedWidgets.map((widget, index) => (
                  <div
                    key={widget.id}
                    className={
                      widget.type === WidgetType.LineChart
                        ? "col-span-12 md:col-span-8"
                        : "col-span-12 md:col-span-4"
                    }
                  >
                    <WidgetRenderer widget={widget} editing={index === sortedWidgets.length - 1} />
                    {isAnalyst && (
                      <button
                        type="button"
                        className="mt-1 text-error text-xs hover:underline"
                        onClick={() => void removeWidget(widget.id)}
                      >
                        Remove widget
                      </button>
                    )}
                  </div>
                ))
              )}
            </div>
          )}
        </div>
      </div>

      <aside className="w-full lg:w-80 surface-panel border-l border-slate-border flex flex-col min-h-0 overflow-y-auto shrink-0">
        <div className="p-md border-b border-slate-border sticky top-0 bg-slate-card z-10">
          <span className="font-headline-sm text-headline-sm text-on-surface">Configure Widget</span>
        </div>

        <div className="p-md space-y-lg flex-1">
          {isAnalyst && (
            <form onSubmit={createDashboard} className="space-y-sm pb-md border-b border-slate-border">
              <label className="block font-label-sm text-label-sm text-on-surface-variant uppercase">New Panel</label>
              <input
                className="input-dark"
                placeholder="Panel name"
                value={newName}
                onChange={(e) => setNewName(e.target.value)}
              />
              <button type="submit" className="btn-solid w-full text-[11px]">Create panel</button>
            </form>
          )}

          <form id="widget-form" onSubmit={addWidget} className="space-y-lg">
            <section className="space-y-sm">
              <label className="block font-label-sm text-label-sm text-on-surface-variant uppercase">Widget Title</label>
              <input
                className="input-dark"
                value={widgetForm.title}
                onChange={(e) => setWidgetForm({ ...widgetForm, title: e.target.value })}
                required
                disabled={!isAnalyst || !selected}
              />
            </section>

            <section className="space-y-sm">
              <label className="block font-label-sm text-label-sm text-on-surface-variant uppercase flex justify-between">
                Visualization Type
                <span className="text-primary-container">{WIDGET_TYPE_LABELS[widgetForm.type]}</span>
              </label>
              <div className="grid grid-cols-3 gap-xs">
                {[
                  { type: WidgetType.LineChart, icon: "show_chart", label: "Line" },
                  { type: WidgetType.MetricCard, icon: "looks_one", label: "Metric" },
                  { type: WidgetType.Map, icon: "map", label: "Map" },
                ].map(({ type, icon, label }) => (
                  <button
                    key={type}
                    type="button"
                    disabled={!isAnalyst}
                    onClick={() => setWidgetForm({ ...widgetForm, type })}
                    className={`p-2 flex flex-col items-center gap-1 transition-colors ${
                      widgetForm.type === type
                        ? "bg-slate-card border-2 border-primary-container text-primary-container"
                        : "bg-slate-inset border border-slate-border text-on-surface-variant hover:border-outline"
                    }`}
                  >
                    <Icon name={icon} className="text-[20px]" />
                    <span className="text-[9px] uppercase font-label-sm">{label}</span>
                  </button>
                ))}
              </div>
            </section>

            <div className="h-px bg-slate-border w-full" />

            <section className="space-y-md">
              <h3 className="font-label-sm text-label-sm text-secondary uppercase flex items-center gap-2">
                <Icon name="database" size={16} />
                Data Binding
              </h3>
              <div className="space-y-xs">
                <label className="block font-label-sm text-[10px] text-on-surface-variant uppercase">Provider</label>
                <select
                  className="input-dark"
                  value={widgetForm.source}
                  onChange={(e) => setWidgetForm({ ...widgetForm, source: e.target.value })}
                  disabled={!isAnalyst}
                >
                  <option value="openmeteo">OpenMeteo API</option>
                  <option value="openweather">OpenWeather</option>
                  <option value="opensky">OpenSky Network</option>
                </select>
              </div>
              <div className="space-y-xs">
                <label className="block font-label-sm text-[10px] text-on-surface-variant uppercase">Metric</label>
                <input
                  className="input-dark"
                  value={widgetForm.metric}
                  onChange={(e) => setWidgetForm({ ...widgetForm, metric: e.target.value })}
                  required
                  disabled={!isAnalyst}
                />
              </div>
              <div className="space-y-xs">
                <label className="block font-label-sm text-[10px] text-on-surface-variant uppercase">Region</label>
                <input
                  className="input-dark"
                  value={widgetForm.region}
                  onChange={(e) => setWidgetForm({ ...widgetForm, region: e.target.value })}
                  required
                  disabled={!isAnalyst}
                />
              </div>
            </section>
          </form>
        </div>

        {isAnalyst && (
          <div className="p-md border-t border-slate-border bg-slate-card">
            <div className="flex gap-sm">
              <button
                type="button"
                onClick={() =>
                  setWidgetForm({
                    title: "Temperature chart",
                    type: WidgetType.LineChart,
                    metric: "temperature",
                    region: DEFAULT_REGION,
                    source: "openmeteo",
                  })
                }
                className="flex-1 py-2 border border-error text-error font-label-sm text-label-sm uppercase hover:bg-error/10 transition-colors"
              >
                Discard
              </button>
              <button
                type="submit"
                form="widget-form"
                disabled={!selected || !isAnalyst}
                className="flex-1 py-2 btn-solid font-bold text-[11px]"
              >
                Apply
              </button>
            </div>
          </div>
        )}
      </aside>
    </div>
  );
}
