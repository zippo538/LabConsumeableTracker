import type { ReactNode } from "react";

type AlertVariant = "success" | "warning" | "error" | "info";

type AlertProps = {
  variant?: AlertVariant;
  title: string;
  description?: string;
  dismissible?: boolean;
  onDismiss?: () => void;
  className?: string;
};

const alertConfig: Record<
  AlertVariant,
  { dotColor: string; symbol: ReactNode }
> = {
  success: {
    dotColor: "bg-emerald-500",
    symbol: "✓",
  },
  warning: {
    dotColor: "bg-amber-500",
    symbol: "!",
  },
  error: {
    dotColor: "bg-red-500",
    symbol: "!",
  },
  info: {
    dotColor: "bg-blue-600",
    symbol: "i",
  },
};

export function Alert({
  variant = "info",
  title,
  description,
  dismissible = true,
  onDismiss,
  className = "",
}: AlertProps) {
  const config = alertConfig[variant];

  return (
    <div
      role="alert"
      className={`rounded-2xl border border-slate-200/80 bg-white p-5 shadow-2xs ${className}`}
    >
      <div className="flex items-start gap-3">
        {/* Dot indicator */}
        <span
          aria-hidden="true"
          className={`mt-1.5 h-2.5 w-2.5 shrink-0 rounded-xs ${config.dotColor}`}
        />

        {/* Content */}
        <div className="min-w-0 flex-1">
          <p className="text-sm font-semibold text-slate-900">{title}</p>
          {description && (
            <p className="mt-1 text-xs text-slate-500">{description}</p>
          )}
        </div>

        {/* Dismiss button */}
        {dismissible && (
          <button
            type="button"
            aria-label="Tutup notifikasi"
            onClick={onDismiss}
            className="-mt-1 shrink-0 rounded p-1 text-lg leading-none text-slate-400 hover:bg-slate-100 hover:text-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-600"
          >
            ×
          </button>
        )}
      </div>
    </div>
  );
}