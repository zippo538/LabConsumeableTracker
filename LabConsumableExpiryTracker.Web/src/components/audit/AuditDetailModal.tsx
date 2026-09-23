import { useEffect } from "react";
import { ModalShell } from "../Modal";
import type { AuditEvent } from "../../types/audit.types";

interface AuditDetailModalProps {
  isOpen: boolean;
  onClose: () => void;
  event: AuditEvent | null;
  loading?: boolean;
}

function formatDateTime(isoString: string): string {
  const date = new Date(isoString);
  return (
    date.toLocaleString("en-GB", {
      day: "2-digit",
      month: "short",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
      timeZone: "UTC",
    }) + " UTC"
  );
}

export function AuditDetailModal({
  isOpen,
  onClose,
  event,
  loading = false,
}: AuditDetailModalProps) {
  // Tutup modal saat Escape ditekan
  useEffect(() => {
    if (!isOpen) return;
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    document.addEventListener("keydown", handleEscape);
    return () => document.removeEventListener("keydown", handleEscape);
  }, [isOpen, onClose]);

  if (!event) return null;

  const isConsumption = event.eventType === "Consumption";

  return (
    <ModalShell
      isOpen={isOpen}
      onClose={onClose}
      title={`${event.eventType} Detail`}
      description={`Event ID: ${event.eventId}`}
      size="md"
      busy={loading}
    >
      <div className="space-y-4">
        {/* Item & Lot Info */}
        <div className="rounded-lg bg-slate-50 p-4">
          <h4 className="text-sm font-semibold text-slate-700 mb-2">
            Item / Lot
          </h4>
          <dl className="grid grid-cols-2 gap-3 text-sm">
            <div>
              <dt className="text-slate-500">Item Name</dt>
              <dd className="font-medium text-slate-900">{event.itemName}</dd>
            </div>
            <div>
              <dt className="text-slate-500">Item Code</dt>
              <dd className="font-medium text-slate-900">{event.itemCode}</dd>
            </div>
            <div>
              <dt className="text-slate-500">Lot Number</dt>
              <dd className="font-medium text-slate-900">{event.lotNumber}</dd>
            </div>
            <div>
              <dt className="text-slate-500">Quantity</dt>
              <dd className="font-medium text-slate-900">
                {event.quantity} {event.baseUnit}
              </dd>
            </div>
          </dl>
        </div>

        {/* Event-Specific Info */}
        <div className="rounded-lg bg-slate-50 p-4">
          <h4 className="text-sm font-semibold text-slate-700 mb-2">
            {isConsumption ? "Job Information" : "Disposal Reason"}
          </h4>
          {isConsumption ? (
            <dl className="text-sm">
              <dt className="text-slate-500">Job Number</dt>
              <dd className="font-medium text-slate-900">
                {event.jobNumber || "—"}
              </dd>
            </dl>
          ) : (
            <dl className="text-sm">
              <dt className="text-slate-500">Reason</dt>
              <dd className="font-medium text-slate-900">
                {event.reason || "—"}
              </dd>
            </dl>
          )}
        </div>

        {/* Actor & Timestamp */}
        <div className="rounded-lg bg-slate-50 p-4">
          <h4 className="text-sm font-semibold text-slate-700 mb-2">
            Audit Trail
          </h4>
          <dl className="grid grid-cols-2 gap-3 text-sm">
            <div>
              <dt className="text-slate-500">Occurred At</dt>
              <dd className="font-medium text-slate-900">
                {formatDateTime(event.occurredAt)}
              </dd>
            </div>
              {isConsumption ? (<div>
              <dt className="text-slate-500">Performed By</dt>
              <dd className="font-medium text-slate-900">{event.fullName}</dd>
            </div>): (null)} 
            
          </dl>
        </div>

        {/* Read-only notice */}
        <div className="rounded-lg border border-sky-200 bg-sky-50 p-3 text-xs text-sky-700">
          <strong>Read-only audit trail.</strong> Records cannot be edited or
          deleted.
        </div>
      </div>
    </ModalShell>
  );
}
