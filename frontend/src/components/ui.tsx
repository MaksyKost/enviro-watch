import type { ReactNode } from "react";

interface IconProps {
  name: string;
  className?: string;
  filled?: boolean;
  title?: string;
  size?: number;
}

export function Icon({ name, className = "", filled = false, title, size }: IconProps) {
  return (
    <span
      className={`material-symbols-outlined ${className}`}
      style={{
        ...(filled ? { fontVariationSettings: "'FILL' 1" } : undefined),
        ...(size ? { fontSize: size } : undefined),
      }}
      title={title}
    >
      {name}
    </span>
  );
}

interface MetricCardProps {
  label: string;
  value: string | number;
  unit?: string;
  icon: string;
  accent?: "primary" | "secondary" | "tint";
}

const accentClass = {
  primary: "text-primary-container",
  secondary: "text-secondary",
  tint: "text-surface-tint",
};

export function MetricCard({ label, value, unit, icon, accent = "primary" }: MetricCardProps) {
  return (
    <div className="bg-surface-container border border-outline-variant rounded p-md flex flex-col justify-between h-32 hover:border-primary-container transition-colors group">
      <div className="flex justify-between items-start">
        <span className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">
          {label}
        </span>
        <Icon name={icon} className={`text-[20px] ${accentClass[accent]}`} />
      </div>
      <div className="flex items-baseline gap-xs">
        <span className="font-mono text-data-display text-3xl text-on-surface">{value}</span>
        {unit && <span className="font-mono text-data-label text-on-surface-variant">{unit}</span>}
      </div>
    </div>
  );
}

export function PageHeader({
  title,
  subtitle,
  action,
}: {
  title: string;
  subtitle?: string;
  action?: ReactNode;
}) {
  return (
    <div className="flex items-baseline justify-between mb-sm col-span-12">
      <div>
        <h1 className="font-headline-lg text-headline-lg text-on-surface">{title}</h1>
        {subtitle && (
          <p className="font-data-label text-data-label text-on-surface-variant mt-1">{subtitle}</p>
        )}
      </div>
      {action}
    </div>
  );
}

export function ErrorBanner({ message }: { message: string }) {
  return (
    <div className="mb-md p-sm border border-error/30 bg-error/10 text-error rounded text-sm col-span-12">
      {message}
    </div>
  );
}

export function LoadingState({ label = "Loading…" }: { label?: string }) {
  return <div className="text-on-surface-variant text-sm py-xl text-center">{label}</div>;
}

export function EmptyState({ message, action }: { message: string; action?: ReactNode }) {
  return (
    <div className="text-center py-xl text-on-surface-variant">
      <p className="mb-md font-body-md">{message}</p>
      {action}
    </div>
  );
}

export function StatusChip({
  label,
  variant = "emerald",
}: {
  label: string;
  variant?: "emerald" | "error" | "muted" | "triggered" | "amber";
}) {
  const cls =
    variant === "error" || variant === "triggered"
      ? "status-chip-error"
      : variant === "muted"
        ? "status-chip-muted"
        : variant === "amber"
          ? "status-chip-amber"
          : "status-chip-emerald";
  return <span className={cls}>{label}</span>;
}
