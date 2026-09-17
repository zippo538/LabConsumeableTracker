// src/components/ui/ErrorAlert.tsx
interface ErrorAlertProps {
  title: string;
  message: string;
}

export const ErrorAlert = ({ title, message }: ErrorAlertProps) => (
  <div
    role="alert"
    className="flex items-start gap-3 rounded-lg border border-red-200 bg-red-50 px-4 py-3"
  >
    <span
      aria-hidden="true"
      className="mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-red-600 text-[11px] font-bold text-white"
    >
      !
    </span>
    <div>
      <p className="text-sm font-bold text-red-600">{title}</p>
      <p className="mt-0.5 text-xs text-red-600">{message}</p>
    </div>
  </div>
);