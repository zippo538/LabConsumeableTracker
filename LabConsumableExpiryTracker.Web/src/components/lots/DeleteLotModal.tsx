// src/components/lots/DeleteLotModal.tsx
import { ConfirmDialog } from "../Modal";
import type { Lot } from "../../types/lot.types";

interface DeleteLotModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  lot: Lot | null;
  isLoading?: boolean;
}

export const DeleteLotModal = ({
  isOpen,
  onClose,
  onConfirm,
  lot,
  isLoading,
}: DeleteLotModalProps) => {
  if (!lot) return null;

  return (
    <ConfirmDialog
      isOpen={isOpen}
      onClose={onClose}
      onConfirm={onConfirm}
      confirmLabel="Delete Lot"
      title="Delete Lot?"
      description="This action cannot be undone. The lot will be permanently removed."
      busy={isLoading}
      impact={
        <div className="space-y-1">
          <p>
            <span className="font-semibold text-slate-800">
              {lot.lotNumber}
            </span>
          </p>
          <p className="text-slate-500">
            {lot.itemName} · {lot.remainingQuantity} {lot.baseUnit} remaining
          </p>
        </div>
      }
    />
  );
};
