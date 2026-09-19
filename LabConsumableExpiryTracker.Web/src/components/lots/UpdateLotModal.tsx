// src/components/lots/UpdateLotModal.tsx
import { useState, useEffect } from "react";
import type { Lot, UpdateLotRequest, ItemOption } from "../../types/lot.types";

interface UpdateLotModalProps {
  isOpen: boolean;
  onClose: () => void;
  lot: Lot | null;
  items: ItemOption[];
}

export const UpdateLotModal = ({
  isOpen,
  onClose,
  lot,
  items,
}: UpdateLotModalProps) => {
  const [formData, setFormData] = useState<UpdateLotRequest>({
    lotNumber: "",
    expiryDate: "",
    storageLocation: "",
    expectedRowVersion: "",
  });

  useEffect(() => {
    if (lot) {
      setFormData({
        lotNumber: lot.lotNumber,
        expiryDate: lot.expiryDate,
        storageLocation: lot.storageLocation,
        expectedRowVersion: lot.rowVersion,
      });
    }
  }, [lot]);

  if (!isOpen || !lot) return null;

  const selectedItem = items.find((item) => item.id === lot.itemId);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // TODO: Call API to update lot
    console.log("Update lot:", { id: lot.id, ...formData });
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl">
        {/* Header */}
        <div className="mb-4 flex items-center justify-between">
          <div>
            <span className="text-xs font-semibold uppercase text-teal-600">
              Update
            </span>
            <h2 className="text-lg font-bold text-slate-800">Update Lot</h2>
            <p className="text-xs text-slate-500">
              Fill in the required information.
            </p>
          </div>
          <button
            onClick={onClose}
            className="rounded p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-600"
          >
            <svg
              className="h-5 w-5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Master Item (Read-only) */}
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Master Item
            </label>
            <input
              type="text"
              value={selectedItem?.name || ""}
              disabled
              className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-500"
            />
          </div>

          {/* Lot Number */}
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Lot Number
            </label>
            <input
              type="text"
              value={formData.lotNumber}
              onChange={(e) =>
                setFormData({ ...formData, lotNumber: e.target.value })
              }
              required
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-teal-500 focus:outline-none focus:ring-2 focus:ring-teal-100"
            />
          </div>

          {/* Initial Quantity (Read-only) */}
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Initial Quantity
            </label>
            <input
              type="text"
              value={`${lot.initialQuantity} ${lot.baseUnit}`}
              disabled
              className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-500"
            />
          </div>

          {/* Expiry Date */}
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Expiry Date
            </label>
            <input
              type="date"
              value={formData.expiryDate}
              onChange={(e) =>
                setFormData({ ...formData, expiryDate: e.target.value })
              }
              required
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-teal-500 focus:outline-none focus:ring-2 focus:ring-teal-100"
            />
          </div>

          {/* Storage Location */}
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Storage Location
            </label>
            <input
              type="text"
              value={formData.storageLocation}
              onChange={(e) =>
                setFormData({ ...formData, storageLocation: e.target.value })
              }
              required
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-teal-500 focus:outline-none focus:ring-2 focus:ring-teal-100"
            />
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-2 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="rounded-lg border border-slate-200 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="rounded-lg bg-teal-500 px-4 py-2 text-sm font-medium text-white hover:bg-teal-600"
            >
              Save Changes
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
