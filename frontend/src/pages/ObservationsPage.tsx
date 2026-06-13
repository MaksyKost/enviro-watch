import { useEffect, useState, type FormEvent } from "react";
import { observationsApi } from "../api";
import { HttpError } from "../api/client";
import { ErrorBanner, Icon, LoadingState } from "../components/ui";
import { useAuth } from "../context/AuthContext";
import type { Observation } from "../types";
import { DEFAULT_LAT, DEFAULT_LON, DEFAULT_REGION } from "../types";

export function ObservationsPage() {
  const { token, isAnalyst } = useAuth();
  const [observations, setObservations] = useState<Observation[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({
    region: DEFAULT_REGION,
    metric: "temperature",
    value: "",
    unit: "°C",
    lat: String(DEFAULT_LAT),
    lon: String(DEFAULT_LON),
    notes: "",
  });

  async function load() {
    if (!token) return;
    setLoading(true);
    try {
      const data = await observationsApi.list(token);
      setObservations(data);
      setError(null);
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Failed to load observations.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, [token]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!token || !isAnalyst) return;

    const value = Number(form.value);
    const lat = Number(form.lat);
    const lon = Number(form.lon);

    if (Number.isNaN(value)) {
      setError("Value must be a number.");
      return;
    }
    if (Number.isNaN(lat) || Number.isNaN(lon)) {
      setError("Coordinates must be numbers.");
      return;
    }

    try {
      await observationsApi.create(token, {
        region: form.region,
        metric: form.metric,
        value,
        unit: form.unit || undefined,
        lat,
        lon,
        notes: form.notes || undefined,
        observedAt: new Date().toISOString(),
      });
      setForm((f) => ({ ...f, value: "", notes: "" }));
      setShowForm(false);
      await load();
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Failed to submit observation.");
    }
  }

  if (loading) return <LoadingState />;

  return (
    <>
      <div className="flex justify-between items-center mb-lg">
        <div>
          <h1 className="font-headline-md text-headline-md text-on-surface">Manual Observations</h1>
          <p className="font-body-md text-body-md text-on-surface-variant mt-1">
            Submit field measurements that sync to the data store.
          </p>
        </div>
        {isAnalyst && (
          <button
            type="button"
            onClick={() => setShowForm((v) => !v)}
            className="btn-solid flex items-center gap-xs"
          >
            <Icon name="add" size={16} />
            New Observation
          </button>
        )}
      </div>

      {error && <ErrorBanner message={error} />}

      {!isAnalyst && (
        <p className="text-sm text-on-surface-variant mb-lg border border-outline-variant rounded p-sm">
          Observations are private to each account. As Viewer you only see your own entries.
        </p>
      )}

      {showForm && isAnalyst && (
        <form onSubmit={handleSubmit} className="surface-panel p-md mb-lg grid grid-cols-1 md:grid-cols-3 gap-md">
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
            <label className="font-label-sm text-label-sm text-on-surface-variant uppercase">Metric</label>
            <input
              className="input-dark mt-1"
              value={form.metric}
              onChange={(e) => setForm({ ...form, metric: e.target.value })}
              required
            />
          </div>
          <div>
            <label className="font-label-sm text-label-sm text-on-surface-variant uppercase">Value</label>
            <input
              type="number"
              step="any"
              className="input-dark mt-1"
              value={form.value}
              onChange={(e) => setForm({ ...form, value: e.target.value })}
              required
            />
          </div>
          <div>
            <label className="font-label-sm text-label-sm text-on-surface-variant uppercase">Unit</label>
            <input
              className="input-dark mt-1"
              value={form.unit}
              onChange={(e) => setForm({ ...form, unit: e.target.value })}
            />
          </div>
          <div>
            <label className="font-label-sm text-label-sm text-on-surface-variant uppercase">Latitude</label>
            <input
              type="number"
              step="any"
              className="input-dark mt-1"
              value={form.lat}
              onChange={(e) => setForm({ ...form, lat: e.target.value })}
            />
          </div>
          <div>
            <label className="font-label-sm text-label-sm text-on-surface-variant uppercase">Longitude</label>
            <input
              type="number"
              step="any"
              className="input-dark mt-1"
              value={form.lon}
              onChange={(e) => setForm({ ...form, lon: e.target.value })}
            />
          </div>
          <div className="md:col-span-3">
            <label className="font-label-sm text-label-sm text-on-surface-variant uppercase">Notes</label>
            <textarea
              className="input-dark mt-1 min-h-[80px]"
              value={form.notes}
              onChange={(e) => setForm({ ...form, notes: e.target.value })}
            />
          </div>
          <div className="md:col-span-3 flex gap-sm">
            <button type="button" onClick={() => setShowForm(false)} className="btn-ghost">
              Cancel
            </button>
            <button type="submit" className="btn-solid">
              Submit observation
            </button>
          </div>
        </form>
      )}

      <div className="surface-panel overflow-hidden">
        <table className="w-full text-left text-sm border-collapse">
          <thead className="border-b border-slate-border bg-slate-inset">
            <tr>
              <th className="p-md font-label-sm text-label-sm text-on-surface-variant uppercase">Region</th>
              <th className="p-md font-label-sm text-label-sm text-on-surface-variant uppercase">Metric</th>
              <th className="p-md font-label-sm text-label-sm text-on-surface-variant uppercase">Value</th>
              <th className="p-md font-label-sm text-label-sm text-on-surface-variant uppercase">Observed</th>
              <th className="p-md font-label-sm text-label-sm text-on-surface-variant uppercase">Notes</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-border">
            {observations.length === 0 ? (
              <tr>
                <td colSpan={5} className="p-lg text-center text-on-surface-variant">
                  No observations recorded.
                </td>
              </tr>
            ) : (
              observations.map((obs) => (
                <tr key={obs.id} className="hover:bg-slate-row transition-colors">
                  <td className="p-md">{obs.region}</td>
                  <td className="p-md font-mono">{obs.metric}</td>
                  <td className="p-md font-mono">
                    {obs.value} {obs.unit}
                  </td>
                  <td className="p-md font-mono text-xs">
                    {new Date(obs.observedAt).toLocaleString()}
                  </td>
                  <td className="p-md text-on-surface-variant">{obs.notes ?? "—"}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </>
  );
}
