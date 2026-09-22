import { useState } from "react";
import {
  type Lot,
  type UpdatableLotAdministrativeStatus,
} from "../../types/lot.types";
import { Alert } from "../MessageAlert";
import { FormModal, ModalField } from "../Modal";
import { useCreateDisposal } from "../../hooks/useDisposals";
import { useUpdateLot } from "../../hooks/useLot";

type DetailLotProps = {
  lot: Lot;
  // Prop onUpdateAdministrativeStatus dihapus karena ditangani oleh useUpdateLot
};

type Notice = {
  variant: "success" | "warning" | "error" | "info";
  title: string;
  description: string;
};

const statusClass: Record<Lot["administrativeStatus"], string> = {
  Active: "border-emerald-200 bg-emerald-50 text-emerald-700",
  Quarantined: "border-amber-200 bg-amber-50 text-amber-800",
  ManuallyBlocked: "border-red-200 bg-red-50 text-red-700",
  Disposed: "border-slate-200 bg-slate-100 text-slate-600",
};

const statusLabel: Record<Lot["administrativeStatus"], string> = {
  Active: "Active",
  Quarantined: "Quarantined",
  ManuallyBlocked: "Manually blocked",
  Disposed: "Disposed",
};

export function DetailLot({ lot }: DetailLotProps) {
  const [administrativeStatus, setAdministrativeStatus] = useState(
    lot.administrativeStatus,
  );

  const [isDisposalOpen, setIsDisposalOpen] = useState(false);
  const [disposalFormError, setDisposalFormError] = useState<string>();

  const [isStatusFormOpen, setIsStatusFormOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string>();
  const [notice, setNotice] = useState<Notice>(() => getInitialNotice(lot));

  // Inisialisasi hook
  const createDisposal = useCreateDisposal();
  const updateLot = useUpdateLot();

  const closeDisposal = () => {
    if (!isSubmitting && !createDisposal.isPending) {
      setIsDisposalOpen(false);
      setDisposalFormError(undefined);
    }
  };

  const closeStatusForm = () => {
    if (!isSubmitting && !updateLot.isPending) {
      setFormError(undefined);
      setIsStatusFormOpen(false);
    }
  };

  const saveDisposal = async (event: React.SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const reason = String(formData.get("reason") ?? "").trim();
    const quantity = Number(formData.get("quantity"));

    if (!reason) {
      setDisposalFormError("Reason is required for disposal.");
      return;
    }
    if (isNaN(quantity) || quantity <= 0 || quantity > lot.remainingQuantity) {
      setDisposalFormError(
        `Quantity must be between 0 and ${lot.remainingQuantity}.`,
      );
      return;
    }

    setIsSubmitting(true);
    setDisposalFormError(undefined);

    try {
      await createDisposal.mutateAsync({
        lotId: lot.id,
        payload: {
          quantity,
          reason,
          expectedRowVersion: lot.rowVersion,
        },
      });

      setIsDisposalOpen(false);
      setNotice({
        variant: "success",
        title: "Disposal recorded",
        description: `${formatQuantity(quantity, lot.baseUnit)} has been recorded in lot history.`,
      });
    } catch (err: any) {
      setDisposalFormError(
        err?.message || "The disposal could not be saved. Please try again.",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const saveAdministrativeStatus = async (
    event: React.SyntheticEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const status = formData.get(
      "administrativeStatus",
    ) as UpdatableLotAdministrativeStatus;

    setIsSubmitting(true);
    setFormError(undefined);

    try {
      // Menyusun payload lengkap karena backend (UpdateLotDto) mewajibkan field-field ini
      await updateLot.mutateAsync({
        id: lot.id,
        payload: {
          lotNumber: lot.lotNumber,
          administrativeStatus: status,
          rowVersion: lot.rowVersion,
          suppliername: lot.suppliername ?? "",
          supplierLotNumber: lot.supplierLotNumber ?? "", // Handle optional field
          expiryDate: lot.expiryDate,
          storageLocation: lot.storageLocation,
        },
      });

      setAdministrativeStatus(status);
      setIsStatusFormOpen(false);
      setNotice({
        variant: "success",
        title: "Administrative status updated",
        description: `${lot.lotNumber} is now ${statusLabel[status].toLowerCase()}.`,
      });
    } catch (err: any) {
      setFormError(
        err?.message || "The status could not be saved. Please try again.",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const isDisposed = administrativeStatus === "Disposed";
  const isBusy =
    isSubmitting || createDisposal.isPending || updateLot.isPending;

  return (
    <div className="space-y-5 text-slate-800">
      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
        <div className="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <div className="flex flex-wrap items-center gap-3">
              <p className="text-sm font-semibold text-slate-500">Lot detail</p>
              <span
                className={`rounded-full border px-3 py-1 text-xs font-semibold ${statusClass[administrativeStatus]}`}
              >
                {statusLabel[administrativeStatus]}
              </span>
            </div>
            <h2 className="mt-2 text-xl font-bold tracking-tight sm:text-2xl">
              {lot.itemName}
            </h2>
            <p className="mt-1 text-sm text-slate-500">Lot ID: {lot.id}</p>
          </div>
          <div className="flex flex-wrap gap-3">
            <button
              type="button"
              disabled={isDisposed}
              onClick={() => setIsDisposalOpen(true)}
              className="rounded-lg border border-red-200 bg-white px-4 py-2.5 text-sm font-semibold text-red-700 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Disposal
            </button>
            <button
              type="button"
              disabled={isDisposed}
              onClick={() => setIsStatusFormOpen(true)}
              className="rounded-lg bg-teal-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-teal-700 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Manage status
            </button>
          </div>
        </div>
        <div className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          <MetricCard
            label="Value at registration"
            value={formatQuantity(lot.initialQuantity, lot.baseUnit)}
            dotClass="bg-teal-500"
          />
          <MetricCard
            label="Current quantity"
            value={formatQuantity(lot.remainingQuantity, lot.baseUnit)}
            dotClass="bg-emerald-600"
          />
          <MetricCard
            label={
              lot.expiryCondition === "ExpiringSoon"
                ? "Expiry approaching"
                : "Expiry date"
            }
            value={formatDate(lot.expiryDate)}
            dotClass={
              lot.expiryCondition === "Expired" ? "bg-red-600" : "bg-amber-600"
            }
            warning={lot.expiryCondition !== "Valid"}
          />
          <MetricCard
            label="Storage location"
            value={lot.storageLocation}
            dotClass="bg-teal-500"
          />
        </div>
      </section>

      <section className="grid gap-5 lg:grid-cols-3">
        <div className="space-y-5 lg:col-span-2">
          <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
            <h3 className="text-base font-bold">Lot information</h3>
            <dl className="mt-5 grid gap-y-5 sm:grid-cols-2">
              <DetailRow label="Lot number" value={lot.lotNumber} />
              <DetailRow label="Item code" value={lot.itemCode} />
              <DetailRow
                label="Received at"
                value={formatDate(lot.receivedAt)}
              />
              <DetailRow
                label="Expiry date"
                value={formatDate(lot.expiryDate)}
              />
              <DetailRow
                label="Supplier"
                value={lot.suppliername ?? "Not recorded"}
              />
              <DetailRow
                label="Supplier lot number"
                value={lot.supplierLotNumber ?? "Not recorded"}
              />
            </dl>
          </section>
          <Alert
            variant={notice.variant}
            title={notice.title}
            description={notice.description}
            dismissible={false}
          />
        </div>
        <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
          <h3 className="text-base font-bold">Allocation eligibility</h3>
          <p className="mt-2 text-sm leading-6 text-slate-500">
            {lot.isEligible
              ? "This lot is eligible for FEFO allocation."
              : "This lot is excluded from FEFO allocation."}
          </p>
          <div
            className={`mt-5 rounded-xl border p-4 ${statusClass[administrativeStatus]}`}
          >
            <p className="text-sm font-semibold">
              Administrative status: {statusLabel[administrativeStatus]}
            </p>
          </div>
        </section>
      </section>

      {/* Form Disposal */}
      <FormModal
        isOpen={isDisposalOpen}
        onClose={closeDisposal}
        title="Dispose this lot?"
        description="Record the quantity and reason for disposal."
        mode="create"
        submitLabel="Confirm disposal"
        onSubmit={saveDisposal}
        busy={isBusy}
        error={disposalFormError || createDisposal.error?.message}
        closeOnOverlayClick={!isBusy}
        identity={
          <div className="text-sm">
            <p className="font-semibold text-slate-800">{lot.id}</p>
            <p className="mt-1 text-slate-500">
              Current quantity:{" "}
              {formatQuantity(lot.remainingQuantity, lot.baseUnit)}
            </p>
          </div>
        }
      >
        <ModalField label="Quantity" htmlFor="quantity" required>
          <input
            id="quantity"
            name="quantity"
            type="number"
            step="0.01"
            min="0"
            max={lot.remainingQuantity}
            defaultValue={lot.remainingQuantity}
            className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none ring-teal-500 focus:ring-2"
          />
        </ModalField>
        <ModalField label="Reason" htmlFor="disposalReason" required>
          <textarea
            id="disposalReason"
            name="reason"
            rows={3}
            placeholder="Explain why this lot is being disposed"
            className="w-full resize-none rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none ring-teal-500 placeholder:text-slate-400 focus:ring-2"
          />
        </ModalField>
        <Alert
          variant="error"
          title="Disposal is irreversible"
          description="The disposed quantity will be permanently removed from allocation and inventory."
          dismissible={false}
        />
      </FormModal>

      {/* Form Manage Status (Tanpa Reason) */}
      <FormModal
        isOpen={isStatusFormOpen}
        onClose={closeStatusForm}
        title="Manage administrative status"
        description="Update the lot administrative status."
        mode="update"
        submitLabel="Save status"
        onSubmit={saveAdministrativeStatus}
        busy={isBusy}
        error={formError || updateLot.error?.message}
        closeOnOverlayClick={!isBusy}
        identity={
          <div className="text-sm">
            <p className="font-semibold text-slate-800">{lot.id}</p>
            <p className="mt-1 text-slate-500">
              Current status: {statusLabel[administrativeStatus]}
            </p>
          </div>
        }
      >
        <ModalField
          label="New administrative status"
          htmlFor="administrativeStatus"
          required
        >
          <select
            id="administrativeStatus"
            name="administrativeStatus"
            defaultValue={
              administrativeStatus === "Disposed"
                ? "Active"
                : administrativeStatus
            }
            className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none ring-teal-500 focus:ring-2"
          >
            <option value="Active">Active</option>
            <option value="Quarantined">Quarantined</option>
            <option value="ManuallyBlocked">Manually blocked</option>
          </select>
        </ModalField>

        {/* Field reason dihapus sesuai permintaan */}

        <Alert
          variant="warning"
          title="Allocation will be blocked"
          description="Quarantined and manually blocked lots are not eligible for FEFO allocation."
          dismissible={false}
        />
      </FormModal>
    </div>
  );
}

// --- Helper Functions ---
function getInitialNotice(lot: Lot): Notice {
  if (lot.expiryCondition === "Expired") {
    return {
      variant: "error",
      title: "Lot has expired",
      description: "This lot is not eligible for FEFO allocation.",
    };
  }
  if (lot.expiryCondition === "ExpiringSoon") {
    return {
      variant: "warning",
      title: "Expiry approaching",
      description: "Prioritize this lot in FEFO allocation.",
    };
  }
  return {
    variant: "info",
    title: "Lot is within its expiry period",
    description: "Continue monitoring quantity and administrative status.",
  };
}

function formatQuantity(quantity: number, unit: string) {
  return `${new Intl.NumberFormat("en-US", { maximumFractionDigits: 2 }).format(quantity)} ${unit}`;
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  }).format(new Date(value));
}

function MetricCard({
  label,
  value,
  dotClass,
  warning = false,
}: {
  label: string;
  value: string;
  dotClass: string;
  warning?: boolean;
}) {
  return (
    <article
      className={`rounded-xl border p-4 ${warning ? "border-amber-500 bg-amber-50" : "border-teal-500 bg-teal-50"}`}
    >
      <div className="flex items-center gap-3">
        <span className="grid size-9 place-items-center rounded-lg bg-white">
          <span className={`size-3 rounded-full ${dotClass}`} />
        </span>
        <p className="text-xl font-bold leading-none">{value}</p>
      </div>
      <p className="mt-5 text-xs text-slate-500">{label}</p>
    </article>
  );
}

function DetailRow({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-xs font-medium text-slate-500">{label}</dt>
      <dd className="mt-1 text-sm font-semibold text-slate-800">{value}</dd>
    </div>
  );
}
