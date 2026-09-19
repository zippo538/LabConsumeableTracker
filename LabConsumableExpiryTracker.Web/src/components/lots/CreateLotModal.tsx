// src/components/lots/CreateLotModal.tsx
import { useState } from "react";
import type { CreateLotRequest, ItemOption } from "../../types/lot.types";

interface CreateLotModalProps {
  isOpen: boolean;
  onClose: () => void;
  items: ItemOption[];
}

export const CreateLotModal = ({
  isOpen,
  onClose,
  items,
}: CreateLotModalProps) => {
  const [formData, setFormData] = useState<CreateLotRequest>({
    itemId: "",
    lotNumber: "",
    initialQuantity: 0,
    expiryDate: "",
    storageLocation: "",
  });

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // TODO: Call API to create lot
    console.log("Create lot:", formData);
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl">
        {/* Header */}
        <div className="mb-4 flex items-center justify-between">
          <div>
            <span className="text-xs font-semibold uppercase text-teal-600">
              Create
            </span>
            <h2 className="text-lg font-bold text-slate-800">
              Register New Lot
            </h2>
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
          {/* Master Item */}
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Master Item
            </label>
            <select
              value={formData.itemId}
              onChange={(e) =>
                setFormData({ ...formData, itemId: e.target.value })
              }
              required
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-teal-500 focus:outline-none focus:ring-2 focus:ring-teal-100"
            >
              <option value="">Select item...</option>
              {items.map((item) => (
                <option key={item.id} value={item.id}>
                  {item.name}
                </option>
              ))}
            </select>
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
              placeholder="LOT-XXX-001"
            />
          </div>

          {/* Initial Quantity */}
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Initial Quantity
            </label>
            <input
              type="number"
              step="0.01"
              min="0"
              value={formData.initialQuantity}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  initialQuantity: parseFloat(e.target.value),
                })
              }
              required
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-teal-500 focus:outline-none focus:ring-2 focus:ring-teal-100"
              placeholder="50"
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
              placeholder="Rack A-02"
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
              Save Lot
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
