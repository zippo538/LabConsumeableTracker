import {
  type ReactNode,
  useEffect,
  useId,
  useRef,
} from "react";

export type ModalSize = "sm" | "md" | "lg";
export type ModalTone = "default" | "danger";

type ModalShellProps = {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  description?: string;
  children: ReactNode;
  footer?: ReactNode;
  size?: ModalSize;
  tone?: ModalTone;
  busy?: boolean;
  closeOnOverlayClick?: boolean;
};

export function ModalShell({
  isOpen,
  onClose,
  title,
  description,
  children,
  footer,
  size = "md",
  tone = "default",
  busy = false,
  closeOnOverlayClick = true,
}: ModalShellProps) {
  const titleId = useId();
  const descriptionId = useId();
  const closeButtonRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (!isOpen) return;

    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    closeButtonRef.current?.focus();

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape" && !busy) onClose();
    }

    window.addEventListener("keydown", handleKeyDown);
    return () => {
      document.body.style.overflow = previousOverflow;
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [isOpen]);

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/45 p-4 sm:p-6"
      onMouseDown={(event) => {
        if (
          event.target === event.currentTarget &&
          closeOnOverlayClick &&
          !busy
        )
          onClose();
      }}
    >
      <section
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        aria-describedby={description ? descriptionId : undefined}
        className={`flex max-h-[calc(100dvh-2rem)] w-full flex-col overflow-hidden rounded-2xl bg-white shadow-2xl ${sizeClass[size]}`}
      >
        <header className="flex items-start justify-between gap-5 border-b border-slate-100 px-6 py-5 sm:px-8">
          <div>
            <p
              className={`mb-1 text-xs font-bold uppercase tracking-wider ${tone === "danger" ? "text-red-600" : "text-teal-700"}`}
            >
              {tone === "danger" ? "Confirmation required" : "Form"}
            </p>
            <h2 id={titleId} className="text-lg font-semibold text-slate-900">
              {title}
            </h2>
            {description && (
              <p
                id={descriptionId}
                className="mt-1 text-sm leading-6 text-slate-500"
              >
                {description}
              </p>
            )}
          </div>
          <button
            ref={closeButtonRef}
            type="button"
            onClick={onClose}
            disabled={busy}
            aria-label="Close modal"
            className="grid h-10 w-10 shrink-0 place-items-center rounded-lg border border-slate-200 text-xl text-slate-500 hover:border-slate-300 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50 focus:outline-none focus-visible:ring-2 focus-visible:ring-teal-500 focus-visible:ring-offset-2"
          >
            ×
          </button>
        </header>

        <div className="overflow-y-auto px-6 py-6 sm:px-8">{children}</div>

        {footer && (
          <footer className="flex flex-col-reverse gap-3 border-t border-slate-100 px-6 py-4 sm:flex-row sm:justify-end sm:px-8">
            {footer}
          </footer>
        )}
      </section>
    </div>
  );
}

type FormModalProps = Omit<ModalShellProps, "children" | "footer" | "tone"> & {
  mode: "create" | "update";
  children: ReactNode;
  submitLabel: string;
  onSubmit: (event: React.SyntheticEvent<HTMLFormElement>) => void;
  cancelLabel?: string;
  identity?: ReactNode;
  guard?: ReactNode;
  error?: ReactNode;
};

export function FormModal({
  mode,
  children,
  submitLabel,
  onSubmit,
  onClose,
  cancelLabel = "Cancel",
  identity,
  guard,
  error,
  busy,
  ...shellProps
}: FormModalProps) {
  const formId = useId();

  return (
    <ModalShell
      {...shellProps}
      onClose={onClose}
      busy={busy}
      description={shellProps.description}
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            disabled={busy}
            className="h-11 rounded-lg border border-slate-200 px-5 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {cancelLabel}
          </button>
          <button
            form={formId}
            type="submit"
            disabled={busy}
            className="h-11 rounded-lg bg-teal-600 px-5 text-sm font-semibold text-white hover:bg-teal-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {busy ? "Saving…" : submitLabel}
          </button>
        </>
      }
    >
      <form id={formId} onSubmit={onSubmit} className="space-y-5">
        {identity && (
          <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
            {identity}
          </div>
        )}
        {error && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {error}
          </div>
        )}
        <input type="hidden" name="mode" value={mode} />
        {children}
        {guard && (
          <div className="rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-900">
            {guard}
          </div>
        )}
      </form>
    </ModalShell>
  );
}

type ConfirmDialogProps = Omit<
  ModalShellProps,
  "children" | "footer" | "tone"
> & {
  confirmLabel: string;
  onConfirm: () => void;
  impact?: ReactNode;
  cancelLabel?: string;
};

export function ConfirmDialog({
  onClose,
  onConfirm,
  confirmLabel,
  impact,
  cancelLabel = "Cancel",
  busy,
  ...shellProps
}: ConfirmDialogProps) {
  return (
    <ModalShell
      {...shellProps}
      onClose={onClose}
      busy={busy}
      tone="danger"
      size="sm"
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            disabled={busy}
            className="h-11 rounded-lg border border-slate-200 px-5 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {cancelLabel}
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={busy}
            className="h-11 rounded-lg bg-red-600 px-5 text-sm font-semibold text-white hover:bg-red-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {busy ? "Deleting…" : confirmLabel}
          </button>
        </>
      }
    >
      <div className="flex gap-4">
        <div className="grid h-10 w-10 shrink-0 place-items-center rounded-full bg-red-100 text-lg font-bold text-red-600">
          !
        </div>
        <div className="space-y-2 text-sm leading-6 text-slate-600">
          <p>This action cannot be undone.</p>
          {impact && <p className="text-slate-500">{impact}</p>}
        </div>
      </div>
    </ModalShell>
  );
}

export function ModalField({
  label,
  htmlFor,
  hint,
  required,
  children,
}: {
  label: string;
  htmlFor?: string;
  hint?: string;
  required?: boolean;
  children: ReactNode;
}) {
  return (
    <label
      htmlFor={htmlFor}
      className="grid gap-2 text-sm font-medium text-slate-700"
    >
      <span>
        {label}
        {required && <span className="ml-1 text-red-600">*</span>}
      </span>
      {children}
      {hint && (
        <span className="text-xs font-normal text-slate-500">{hint}</span>
      )}
    </label>
  );
}

const sizeClass: Record<ModalSize, string> = {
  sm: "max-w-[484px]",
  md: "max-w-[470px]",
  lg: "max-w-[564px]",
};
