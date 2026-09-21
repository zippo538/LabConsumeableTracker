import { type CSSProperties, type ReactNode } from "react";

type StateAction = {
  label: string;
  onClick: () => void;
};

export type LoadingStateProps = {
  variant?: "table" | "card" | "content";
  rows?: number;
  columns?: number;
  className?: string;
};

export function LoadingState({
  variant = "content",
  rows = 4,
  columns = 5,
  className = "",
}: LoadingStateProps) {
  if (variant === "table") {
    return (
      <div
        className={`overflow-hidden rounded-xl border border-slate-200 bg-white ${className}`}
        aria-busy="true"
        aria-label="Loading table"
      >
        <div className="h-12 border-b border-slate-100 bg-slate-50" />
        {Array.from({ length: rows }, (_, rowIndex) => (
          <div
            key={rowIndex}
            className="grid min-w-[720px] gap-8 border-b border-slate-100 px-6 py-[15px] last:border-b-0"
            style={
              {
                gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))`,
              } as CSSProperties
            }
          >
            {Array.from({ length: columns }, (_, columnIndex) => (
              <span
                key={columnIndex}
                className="h-3.5 animate-pulse rounded-full bg-slate-100"
                style={{
                  width: `${55 + ((rowIndex + columnIndex) % 3) * 15}%`,
                }}
              />
            ))}
          </div>
        ))}
      </div>
    );
  }

  if (variant === "card") {
    return (
      <div
        className={`rounded-xl border border-slate-200 bg-white p-5 ${className}`}
        aria-busy="true"
        aria-label="Loading summary"
      >
        <div className="mb-5 h-9 w-9 animate-pulse rounded-lg bg-slate-100" />
        <div className="mb-3 h-7 w-20 animate-pulse rounded bg-slate-100" />
        <div className="h-3.5 w-28 animate-pulse rounded-full bg-slate-100" />
      </div>
    );
  }

  return (
    <div
      className={`grid place-items-center gap-3 rounded-xl border border-slate-200 bg-white px-6 py-12 text-center ${className}`}
      aria-busy="true"
      aria-live="polite"
    >
      <span
        className="h-9 w-9 animate-spin rounded-full border-4 border-slate-200 border-t-teal-600"
        aria-hidden="true"
      />
      <p className="text-sm text-slate-500">Loading data…</p>
    </div>
  );
}

export type EmptyStateProps = {
  title: string;
  description: string;
  action?: StateAction;
  icon?: ReactNode;
  className?: string;
};

export function EmptyState({
  title,
  description,
  action,
  icon = "⌕",
  className = "",
}: EmptyStateProps) {
  return (
    <section
      className={`grid place-items-center px-6 py-14 text-center ${className}`}
    >
      <div
        className="mb-4 grid h-11 w-11 place-items-center rounded-xl bg-cyan-50 text-xl text-teal-700"
        aria-hidden="true"
      >
        {icon}
      </div>
      <h2 className="text-base font-semibold text-slate-900">{title}</h2>
      <p className="mt-2 max-w-sm text-sm leading-6 text-slate-500">
        {description}
      </p>
      {action && <StateButton action={action} className="mt-5" />}
    </section>
  );
}

export type ErrorStateProps = {
  title?: string;
  description?: string;
  onRetry?: () => void;
  retryLabel?: string;
  className?: string;
};

export function ErrorState({
  title = "Could not load data",
  description = "The server did not respond. Check your connection, then try again.",
  onRetry,
  retryLabel = "Try again",
  className = "",
}: ErrorStateProps) {
  return (
    <section
      className={`grid place-items-center px-6 py-14 text-center ${className}`}
      role="alert"
    >
      <div
        className="mb-4 grid h-11 w-11 place-items-center rounded-xl bg-red-50 text-xl font-bold text-red-600"
        aria-hidden="true"
      >
        !
      </div>
      <h2 className="text-base font-semibold text-slate-900">{title}</h2>
      <p className="mt-2 max-w-sm text-sm leading-6 text-slate-500">
        {description}
      </p>
      {onRetry && (
        <StateButton
          action={{ label: retryLabel, onClick: onRetry }}
          className="mt-5"
        />
      )}
    </section>
  );
}

type AccessStateProps = {
  title: string;
  description: string;
  action: StateAction;
  className?: string;
};

function AccessState({
  title,
  description,
  action,
  className = "",
}: AccessStateProps) {
  return (
    <main className={`grid min-h-[55vh] place-items-center px-6 ${className}`}>
      <section className="max-w-md text-center">
        <div
          className="mx-auto mb-5 grid h-12 w-12 place-items-center rounded-xl bg-slate-100 text-xl text-slate-700"
          aria-hidden="true"
        >
          ⌘
        </div>
        <h1 className="text-xl font-semibold text-slate-900">{title}</h1>
        <p className="mt-2 text-sm leading-6 text-slate-500">{description}</p>
        <StateButton action={action} className="mt-6" />
      </section>
    </main>
  );
}

export function UnauthorizedState({
  onSignIn,
  className,
}: {
  onSignIn: () => void;
  className?: string;
}) {
  return (
    <AccessState
      title="Your session has ended"
      description="Sign in again to continue using Consumable Tracker."
      action={{ label: "Sign in again", onClick: onSignIn }}
      className={className}
    />
  );
}

export function ForbiddenState({
  onBackToDashboard,
  className,
}: {
  onBackToDashboard: () => void;
  className?: string;
}) {
  return (
    <AccessState
      title="You do not have access to this page"
      description="This page is available only to users with the required role."
      action={{ label: "Back to Dashboard", onClick: onBackToDashboard }}
      className={className}
    />
  );
}

export type AlertTone = "info" | "success" | "warning" | "error";

export function InlineAlert({
  tone = "info",
  title,
  children,
  onDismiss,
  className = "",
}: {
  tone?: AlertTone;
  title?: string;
  children: ReactNode;
  onDismiss?: () => void;
  className?: string;
}) {
  const style = alertStyle[tone];

  return (
    <div
      className={`flex gap-3 rounded-xl border px-4 py-3 ${style.container} ${className}`}
      role={tone === "warning" || tone === "error" ? "alert" : "status"}
    >
      <span
        className={`grid h-6 w-6 shrink-0 place-items-center rounded-full text-xs font-bold ${style.icon}`}
        aria-hidden="true"
      >
        {style.symbol}
      </span>
      <div className="min-w-0 flex-1 text-sm leading-6">
        {title && <p className={`font-semibold ${style.title}`}>{title}</p>}
        <div className={style.text}>{children}</div>
      </div>
      {onDismiss && (
        <button
          type="button"
          onClick={onDismiss}
          aria-label="Dismiss alert"
          className={`h-7 w-7 shrink-0 rounded-md text-lg leading-none ${style.dismiss}`}
        >
          ×
        </button>
      )}
    </div>
  );
}

function StateButton({
  action,
  className = "",
}: {
  action: StateAction;
  className?: string;
}) {
  return (
    <button
      type="button"
      onClick={action.onClick}
      className={`h-10 rounded-lg bg-teal-600 px-4 text-sm font-semibold text-white hover:bg-teal-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-teal-500 focus-visible:ring-offset-2 ${className}`}
    >
      {action.label}
    </button>
  );
}

const alertStyle: Record<
  AlertTone,
  {
    container: string;
    icon: string;
    symbol: string;
    title: string;
    text: string;
    dismiss: string;
  }
> = {
  info: {
    container: "border-sky-200 bg-sky-50",
    icon: "bg-sky-100 text-sky-700",
    symbol: "i",
    title: "text-sky-900",
    text: "text-sky-800",
    dismiss: "text-sky-700 hover:bg-sky-100",
  },
  success: {
    container: "border-green-200 bg-green-50",
    icon: "bg-green-100 text-green-700",
    symbol: "✓",
    title: "text-green-900",
    text: "text-green-800",
    dismiss: "text-green-700 hover:bg-green-100",
  },
  warning: {
    container: "border-amber-200 bg-amber-50",
    icon: "bg-amber-100 text-amber-700",
    symbol: "!",
    title: "text-amber-900",
    text: "text-amber-800",
    dismiss: "text-amber-700 hover:bg-amber-100",
  },
  error: {
    container: "border-red-200 bg-red-50",
    icon: "bg-red-100 text-red-700",
    symbol: "!",
    title: "text-red-900",
    text: "text-red-800",
    dismiss: "text-red-700 hover:bg-red-100",
  },
};
