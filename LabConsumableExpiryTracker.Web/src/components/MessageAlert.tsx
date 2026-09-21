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

const alertStyles: Record<
  AlertVariant,
  { wrapper: string; icon: string; symbol: ReactNode }
> = {
  success: {
    wrapper: "border-[#7ada96] bg-[#e3faeb] text-[#14873b]",
    icon: "bg-[#14873b] text-white",
    symbol: "✓",
  },
  warning: {
    wrapper: "border-[#f5c95e] bg-[#fff5db] text-[#8c5908]",
    icon: "bg-[#8c5908] text-white",
    symbol: "!",
  },
  error: {
    wrapper: "border-[#f5adad] bg-[#ffeeee] text-[#bd1f1f]",
    icon: "bg-[#bd1f1f] text-white",
    symbol: "!",
  },
  info: {
    wrapper: "border-[#85bff0] bg-[#e6f5ff] text-[#0a599e]",
    icon: "bg-[#0a599e] text-white",
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
  const style = alertStyles[variant];

  return (
    <div
      role="alert"
      className={`flex items-start gap-3 rounded-xl border px-4 py-3.5 ${style.wrapper} ${className}`}
    >
      <span
        aria-hidden="true"
        className={`flex size-7 shrink-0 items-center justify-center rounded-full text-sm font-bold ${style.icon}`}
      >
        {style.symbol}
      </span>

      <div className="min-w-0 flex-1">
        <p className="text-base font-semibold leading-[22px]">{title}</p>

        {description && (
          <p className="mt-0.5 text-xs leading-[18px] opacity-90">
            {description}
          </p>
        )}
      </div>

      {dismissible && (
        <button
          type="button"
          aria-label="Tutup notifikasi"
          onClick={onDismiss}
          className="ml-1 -mt-1 shrink-0 rounded p-1 text-lg leading-none hover:bg-black/5 focus:outline-none focus:ring-2 focus:ring-current"
        >
          ×
        </button>
      )}
    </div>
  );
}
