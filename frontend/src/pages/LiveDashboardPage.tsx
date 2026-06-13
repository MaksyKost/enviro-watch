import { useCallback, useEffect, useMemo, useState } from "react";
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { dataApi } from "../api";
import { RegionMap } from "../components/RegionMap";
import { ErrorBanner, Icon, MetricCard } from "../components/ui";
import { useSignalR } from "../hooks/useSignalR";
import type { DataSnapshot, DataUpdate } from "../types";
import { DEFAULT_REGION } from "../types";

function latestValue(snapshots: DataSnapshot[], metric: string, source?: string): DataSnapshot | undefined {
  return snapshots
    .filter((s) => s.metric === metric && (!source || s.source === source))
    .sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime())[0];
}

function downloadCsv(rows: { timestamp: string; value: number }[], filename: string) {
  const header = "timestamp,value\n";
  const body = rows.map((r) => `${r.timestamp},${r.value}`).join("\n");
  const blob = new Blob([header + body], { type: "text/csv" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = filename;
  link.click();
  URL.revokeObjectURL(url);
}

export function LiveDashboardPage() {
  const [history, setHistory] = useState<DataSnapshot[]>([]);
  const [latest, setLatest] = useState<DataSnapshot[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [liveMetrics, setLiveMetrics] = useState<DataUpdate["data"] | null>(null);

  const onUpdate = useCallback((update: DataUpdate) => {
    if (update.region === DEFAULT_REGION) {
      setLiveMetrics(update.data);
    }
  }, []);

  const { connected, logs } = useSignalR(onUpdate);

  useEffect(() => {
    const from = new Date(Date.now() - 48 * 60 * 60 * 1000).toISOString();
    Promise.all([
      dataApi.snapshots({
        region: DEFAULT_REGION,
        metric: "temperature",
        from,
        pageSize: 200,
      }),
      dataApi.snapshots({ region: DEFAULT_REGION, pageSize: 100 }),
    ])
      .then(([historyRes, latestRes]) => {
        setHistory(historyRes.items);
        setLatest(latestRes.items);
      })
      .catch((err: Error) => setError(err.message));
  }, []);

  const chartData = useMemo(
    () =>
      [...history]
        .sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime())
        .map((item) => ({
          time: new Date(item.timestamp).toLocaleTimeString("en-GB", {
            hour: "2-digit",
            minute: "2-digit",
          }),
          value: item.value,
          timestamp: item.timestamp,
        })),
    [history],
  );

  const temp = liveMetrics?.temperature ?? latestValue(latest, "temperature")?.value ?? "—";
  const pm25 = latestValue(latest, "pm25")?.value ?? "—";
  const aircraft = latestValue(latest, "aircraft_count", "opensky")?.value ?? "—";
  const humidity = liveMetrics?.humidity ?? latestValue(latest, "humidity")?.value ?? "—";

  const regionLabel = DEFAULT_REGION.replace(",", ", ");

  return (
    <div className="grid grid-cols-12 gap-md md:gap-lg">
      <div className="col-span-12 flex items-baseline justify-between mb-sm">
        <h1 className="font-headline-lg text-headline-lg text-on-surface">{regionLabel}</h1>
        <span className="font-data-label text-data-label text-on-surface-variant">
          Lat: 51.1079° N, Lon: 17.0385° E
        </span>
      </div>

      {error && (
        <div className="col-span-12">
          <ErrorBanner message={error} />
        </div>
      )}

      <div className="col-span-12 sm:col-span-4">
        <MetricCard label="Temperature" value={temp} unit="°C" icon="thermostat" />
      </div>
      <div className="col-span-12 sm:col-span-4">
        <MetricCard label="PM2.5 Density" value={pm25} unit="µg/m³" icon="air" accent="secondary" />
      </div>
      <div className="col-span-12 sm:col-span-4">
        <MetricCard label="Aircraft In Sector" value={aircraft} unit="Active" icon="flight" accent="tint" />
      </div>

      <div className="col-span-12 surface-card p-md flex flex-col h-80">
        <div className="flex justify-between items-center mb-md">
          <h2 className="font-headline-sm text-headline-sm text-on-surface">Temperature — 48h History</h2>
          <button
            type="button"
            onClick={() =>
              downloadCsv(
                chartData.map((d) => ({ timestamp: d.timestamp, value: d.value })),
                "temperature-48h.csv",
              )
            }
            className="text-on-surface-variant hover:text-primary transition-colors flex items-center gap-xs text-xs font-label-sm border border-outline-variant px-2 py-1 rounded-sm"
          >
            <Icon name="download" className="text-[14px]" />
            CSV
          </button>
        </div>
        <div className="flex-1 min-h-0">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#414751" opacity={0.5} />
              <XAxis dataKey="time" stroke="#8b919d" tick={{ fontSize: 10 }} />
              <YAxis stroke="#8b919d" tick={{ fontSize: 10 }} unit="°" />
              <Tooltip
                contentStyle={{
                  background: "#1d2025",
                  border: "1px solid #414751",
                  borderRadius: 4,
                }}
              />
              <Line
                type="monotone"
                dataKey="value"
                stroke="#60a5fa"
                strokeWidth={2}
                dot={false}
                activeDot={{ r: 4, fill: "#101419", stroke: "#60a5fa", strokeWidth: 2 }}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </div>

      <div className="col-span-12 md:col-span-8 surface-card flex flex-col h-72">
        <div className="px-md py-sm border-b border-outline-variant flex justify-between items-center bg-surface-variant/50">
          <h2 className="font-headline-sm text-headline-sm text-on-surface flex items-center gap-sm">
            <Icon name="terminal" className="text-[18px] text-on-surface-variant" />
            SignalR Telemetry Log
          </h2>
          <span
            className={`font-data-label text-data-label px-2 py-0.5 rounded-sm border ${
              connected
                ? "text-primary bg-primary/10 border-primary/20"
                : "text-error bg-error/10 border-error/20"
            }`}
          >
            {connected ? "Connected" : "Disconnected"}
          </span>
        </div>
        <div className="flex-1 overflow-y-auto p-sm space-y-1 font-data-label text-data-label">
          {logs.length === 0 ? (
            <div className="text-on-surface-variant px-sm py-md">Waiting for live updates…</div>
          ) : (
            logs.map((entry) => (
              <div
                key={entry.id}
                className="flex gap-md py-1 border-b border-outline-variant/30 hover:bg-surface-variant/30 px-sm transition-colors"
              >
                <span className="text-on-surface-variant w-24 shrink-0">{entry.time}</span>
                <span
                  className={`w-16 shrink-0 ${
                    entry.level === "WARN" ? "text-error" : entry.level === "DEBUG" ? "text-surface-tint" : "text-secondary"
                  }`}
                >
                  [{entry.level}]
                </span>
                <span className="text-on-surface truncate">{entry.message}</span>
              </div>
            ))
          )}
        </div>
      </div>

      <div className="col-span-12 md:col-span-4 surface-card flex flex-col h-72 overflow-hidden">
        <RegionMap
          overlayTitle
          metrics={[
            { label: "Temp", value: `${temp}°C` },
            { label: "Humidity", value: `${humidity}%` },
          ]}
        />
      </div>
    </div>
  );
}
