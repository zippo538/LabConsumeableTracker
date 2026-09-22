// src/components/lots/DeleteLotModal.tsx
import { CheckCircle2} from "lucide-react";
import { ConfirmDialog } from "../Modal";
import { Alert } from "../MessageAlert";
import type { Lot } from "../../types/lot.types";

interface DeleteLotModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => Promise<void> | void;
  lot: Lot | null;
  isLoading?: boolean;
  error?: string;
  successMessage?: string; // Tambahkan prop opsional ini
}

export const DeleteLotModal = ({
  isOpen,
  onClose,
  onConfirm,
  lot,
  isLoading,
  error,
  successMessage,
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
        <div className="space-y-3">
          {/* Success State inside Impact area */}
          {successMessage && (
            <div className="flex items-start gap-3 rounded-xl border border-[#7ada96] bg-[#e3faeb] px-4 py-3 text-[12px] text-[#14873b]">
              <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0" />
              <span>{successMessage}</span>
            </div>
          )}

          {/* Error State using Alert Component */}
          {error && (
            <Alert
              variant="error"
              title="Failed to delete lot"
              description={error}
              dismissible={false}
            />
          )}

          <div className="space-y-1 pt-2">
            <p>
              <span className="font-semibold text-slate-800">
                {lot.lotNumber}
              </span>
            </p>
            <p className="text-slate-500">
              {lot.itemName} · {lot.remainingQuantity} {lot.baseUnit} remaining
            </p>
          </div>
        </div>
      }
    />
  );
};
