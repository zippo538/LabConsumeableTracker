// src/components/lots/UpdateLotModal.tsx
import { useState, useEffect } from "react";
import { FormModal, ModalField } from "../Modal";
import type {
  Lot,
  UpdateLotRequest,
} from "../../types/lot.types";

interface UpdateLotModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (payload: UpdateLotRequest) => void;
  lot: Lot | null;
  isLoading?: boolean;
  error?: string;
}


export const UpdateLotModal = ({
  isOpen,
  onClose,
  onSubmit,
  lot,
  isLoading,
  error,
}: UpdateLotModalProps) => {
  const [formData, setFormData] = useState<UpdateLotRequest>({
    lotNumber: "",
    expiryDate: "",
    storageLocation: "",
    suppliername: "",
    rowVersion: "",
    supplierLotNumber: "",
    administrativeStatus: "Active",
  });
  useEffect(() => {
    if (lot) {
      const formattedDate = lot.expiryDate ? lot.expiryDate.split("T")[0] : "";

      setFormData({
        lotNumber: lot.lotNumber,
        administrativeStatus: lot.administrativeStatus,
        expiryDate: formattedDate,
        supplierLotNumber : lot.supplierLotNumber ?? "",
        storageLocation: lot.storageLocation,
        suppliername: lot.suppliername ?? "",
        rowVersion: lot.rowVersion,
      });
    }
  }, [lot]);

  if (!lot) return null;

  const handleSubmit = (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <FormModal
      isOpen={isOpen}
      onClose={onClose}
      mode="update"
      title="Update Lot"
      description="Update administrative information for this lot."
      submitLabel="Save Changes"
      onSubmit={handleSubmit}
      busy={isLoading}
      error={error ? <p>{error}</p> : undefined}
      identity={
        <div className="flex items-center gap-3">
          <div className="grid h-10 w-10 shrink-0 place-items-center rounded-lg bg-teal-50 text-sm font-bold text-teal-700">
            {lot.itemName?.slice(0, 1).toUpperCase() ?? "?"}
          </div>
          <div>
            <p className="text-sm font-semibold text-slate-900">
              {lot.itemName ?? "Unknown Item"}
            </p>
            <p className="text-xs text-slate-500">
              {lot.lotNumber} · {lot.baseUnit}
            </p>
          </div>
        </div>
      }
    >
      {/* Read-only summary */}
      <div className="grid grid-cols-2 gap-3 rounded-xl border border-slate-200 bg-slate-50 p-4 text-xs">
        <div>
          <p className="text-[10px] font-bold uppercase tracking-wider text-slate-400">
            Initial Qty
          </p>
          <p className="mt-0.5 font-semibold text-slate-800">
            {lot.initialQuantity} {lot.baseUnit}
          </p>
        </div>
        <div>
          <p className="text-[10px] font-bold uppercase tracking-wider text-slate-400">
            Remaining
          </p>
          <p className="mt-0.5 font-semibold text-slate-800">
            {lot.remainingQuantity} {lot.baseUnit}
          </p>
        </div>
        <div>
          <p className="text-[10px] font-bold uppercase tracking-wider text-slate-400">
            Expiry
          </p>
          <p className="mt-0.5 font-semibold text-slate-800">
            {new Date(lot.expiryDate).toLocaleDateString("en-US", {
              day: "numeric",
              month: "short",
              year: "numeric",
            })}
          </p>
        </div>
        <div>
          <p className="text-[10px] font-bold uppercase tracking-wider text-slate-400">
            Received At
          </p>
          <p className="mt-0.5 font-semibold text-slate-800">
            {new Date(lot.receivedAt).toLocaleDateString("en-US", {
              day: "numeric",
              month: "short",
              year: "numeric",
            })}
          </p>
        </div>
      </div>

      <ModalField label="Lot Number" htmlFor="update-lotNumber" required>
        <input
          id="update-lotNumber"
          type="text"
          value={formData.lotNumber}
          onChange={(e) =>
            setFormData({ ...formData, lotNumber: e.target.value })
          }
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      {/* 2. Expiry Date (Required, Editable) */}
      <ModalField label="Expiry Date" htmlFor="update-expiryDate" required>
        <input
          id="update-expiryDate"
          type="date"
          value={formData.expiryDate}
          onChange={(e) =>
            setFormData({ ...formData, expiryDate: e.target.value })
          }
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      {/* ✅ Supplier Name - Wajib sesuai validasi backend */}
      <ModalField label="Supplier Name" htmlFor="update-supplierName" required>
        <input
          id="update-supplierName"
          type="text"
          value={formData.suppliername}
          onChange={(e) =>
            setFormData({ ...formData, suppliername: e.target.value })
          }
          placeholder="Nama supplier"
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      {/* ✅ Supplier Lot Number - Opsional */}
      <ModalField
        label="Supplier Lot Number"
        htmlFor="update-supplierLotNumber"
        hint="Nomor lot dari supplier (opsional)"
      >
        <input
          id="update-supplierLotNumber"
          type="text"
          value={formData.supplierLotNumber ?? ""}
          onChange={(e) =>
            setFormData({
              ...formData,
              lotNumber: e.target.value,
            })
          }
          placeholder="SUP-LOT-001"
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      <ModalField
        label="Storage Location"
        htmlFor="update-storageLocation"
        required
      >
        <input
          id="update-storageLocation"
          type="text"
          value={formData.storageLocation}
          onChange={(e) =>
            setFormData({ ...formData, storageLocation: e.target.value })
          }
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>
      {/* ✅ RowVersion tersembunyi - dikirim otomatis, tidak perlu ditampilkan */}
      <input type="hidden" value={formData.rowVersion} />
    </FormModal>
  );
};
