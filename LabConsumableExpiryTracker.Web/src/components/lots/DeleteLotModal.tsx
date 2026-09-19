// src/components/lots/DeleteLotModal.tsx
import type { Lot } from "../../types/lot.types";

interface DeleteLotModalProps {
  isOpen: boolean;
  onClose: () => void;
  lot: Lot | null;
}

export const DeleteLotModal = ({
  isOpen,
  onClose,
  lot,
}: DeleteLotModalProps) => {
  if (!isOpen || !lot) return null;

  const handleDelete = () => {
    // TODO: Call API to delete lot
    console.log("Delete lot:", lot.id);
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="w-full max-w-sm rounded-lg bg-white p-6 shadow-xl">
        {/* Header */}
        <div className="mb-4 flex items-start gap-3">
          <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-full bg-red-100">
            <svg
              className="h-5 w-5 text-red-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
              />
            </svg>
          </div>
          <div>
            <h2 className="text-lg font-bold text-slate-800">Delete Lot?</h2>
            <p className="mt-1 text-sm text-slate-500">
              This action cannot be undone. Related data will be removed from
              the active list.
            </p>
          </div>
        </div>

        {/* Lot Info */}
        <div className="mb-4 rounded-lg bg-slate-50 p-3">
          <p className="text-sm font-medium text-slate-800">{lot.lotNumber}</p>
          <p className="text-xs text-slate-500">{lot.itemName}</p>
        </div>

        {/* Actions */}
        <div className="flex justify-end gap-2">
          <button
            onClick={onClose}
            className="rounded-lg border border-slate-200 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
          >
            Cancel
          </button>
          <button
            onClick={handleDelete}
            className="rounded-lg bg-red-500 px-4 py-2 text-sm font-medium text-white hover:bg-red-600"
          >
            Delete
          </button>
        </div>
      </div>
    </div>
  );
};
