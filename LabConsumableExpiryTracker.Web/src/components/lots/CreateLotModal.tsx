// src/components/lots/CreateLotModal.tsx
import { useState } from "react";
import { FormModal, ModalField } from "../Modal";
import type { CreateLotRequest, ItemOption } from "../../types/lot.types";

interface CreateLotModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (payload: CreateLotRequest) => void;
  items: ItemOption[];
  isLoading?: boolean;
  error?: string;
}

export const CreateLotModal = ({
  isOpen,
  onClose,
  onSubmit,
  items,
  isLoading,
  error,
}: CreateLotModalProps) => {
  const [formData, setFormData] = useState<CreateLotRequest>({
    itemId: "",
    lotNumber: "",
    initialQuantity: 0,
    expiryDate: "",
    storageLocation: "",
    receivedAt: new Date().toISOString().slice(0, 10),
    supplierName: "",
    supplierLotNumber: "",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  const selectedItem = items.find((item) => item.id === formData.itemId);

  return (
    <FormModal
      isOpen={isOpen}
      onClose={onClose}
      mode="create"
      title="Register New Lot"
      description="Fill in the required information."
      submitLabel="Save Lot"
      onSubmit={handleSubmit}
      busy={isLoading}
      error={error ? <p>{error}</p> : undefined}
      identity={
        selectedItem ? (
          <div className="flex items-center gap-3">
            <div className="grid h-10 w-10 shrink-0 place-items-center rounded-lg bg-teal-50 text-sm font-bold text-teal-700">
              {selectedItem.name.slice(0, 1).toUpperCase()}
            </div>
            <div>
              <p className="text-sm font-semibold text-slate-900">
                {selectedItem.name}
              </p>
              <p className="text-xs text-slate-500">
                {selectedItem.code} · {selectedItem.baseUnit}
              </p>
            </div>
          </div>
        ) : undefined
      }
    >
      <ModalField label="Master Item" htmlFor="create-itemId" required>
        <select
          id="create-itemId"
          value={formData.itemId}
          onChange={(e) => setFormData({ ...formData, itemId: e.target.value })}
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        >
          <option value="">Select item...</option>
          {items.map((item) => (
            <option key={item.id} value={item.id}>
              {item.name} ({item.code})
            </option>
          ))}
        </select>
      </ModalField>

      <ModalField label="Lot Number" htmlFor="create-lotNumber" required>
        <input
          id="create-lotNumber"
          type="text"
          value={formData.lotNumber}
          onChange={(e) =>
            setFormData({ ...formData, lotNumber: e.target.value })
          }
          placeholder="LOT-XXX-001"
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      <ModalField
        label="Initial Quantity"
        htmlFor="create-initialQuantity"
        required
      >
        <input
          id="create-initialQuantity"
          type="number"
          step="0.01"
          min="0"
          value={formData.initialQuantity || ""}
          onChange={(e) =>
            setFormData({
              ...formData,
              initialQuantity: parseFloat(e.target.value) || 0,
            })
          }
          placeholder="50"
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      <ModalField label="Expiry Date" htmlFor="create-expiryDate" required>
        <input
          id="create-expiryDate"
          type="date"
          value={formData.expiryDate}
          onChange={(e) =>
            setFormData({ ...formData, expiryDate: e.target.value })
          }
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>
      {/* ✅ Received At - Baru */}
      <ModalField label="Received At" htmlFor="create-receivedAt" required>
        <input
          id="create-receivedAt"
          type="date"
          value={formData.receivedAt}
          onChange={(e) =>
            setFormData({ ...formData, receivedAt: e.target.value })
          }
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      {/* ✅ Supplier Name - Baru */}
      <ModalField label="Supplier Name" htmlFor="create-supplierName" required>
        <input
          id="create-supplierName"
          type="text"
          value={formData.supplierName}
          onChange={(e) =>
            setFormData({ ...formData, supplierName: e.target.value })
          }
          placeholder="Nama supplier"
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      {/* ✅ Supplier Lot Number - Baru */}
      <ModalField
        label="Supplier Lot Number"
        htmlFor="create-supplierLotNumber"
        required
      >
        <input
          id="create-supplierLotNumber"
          type="text"
          value={formData.supplierLotNumber}
          onChange={(e) =>
            setFormData({ ...formData, supplierLotNumber: e.target.value })
          }
          placeholder="Nomor lot dari supplier"
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      <ModalField
        label="Storage Location"
        htmlFor="create-storageLocation"
        required
      >
        <input
          id="create-storageLocation"
          type="text"
          value={formData.storageLocation}
          onChange={(e) =>
            setFormData({ ...formData, storageLocation: e.target.value })
          }
          placeholder="Rack A-02"
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>
    </FormModal>
  );
};
