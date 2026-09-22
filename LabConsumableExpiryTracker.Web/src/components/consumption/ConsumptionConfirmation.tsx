import React from "react";
import { CheckCircle2, ArrowLeft, Loader2 } from "lucide-react";

interface ConsumptionConfirmationProps {
  canConfirm: boolean;
  isSubmitting: boolean;
  onConfirm: () => void;
  onBack: () => void;
}

export const ConsumptionConfirmation: React.FC<ConsumptionConfirmationProps> = ({
  canConfirm,
  isSubmitting,
  onConfirm,
  onBack,
}) => {
  return (
    <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
      <div className="max-w-xl">
        <h2 className="text-[16px] font-bold tracking-tight text-slate-900">
          3. Confirm consumption
        </h2>
        <p className="mt-1 text-[12px] text-slate-500 leading-relaxed">
          When confirmed, the system records consumption and updates lot stock atomically.
          Make sure the job and allocation quantity are correct.
        </p>
      </div>

      <div className="flex items-center gap-3 shrink-0">
        <button
          type="button"
          onClick={onBack}
          disabled={isSubmitting}
          className="inline-flex h-11 items-center gap-2 rounded-xl border border-slate-200 bg-white px-5 text-[12px] font-semibold text-slate-700 transition hover:bg-slate-50 hover:border-slate-300 disabled:opacity-50"
        >
          <ArrowLeft className="h-4 w-4" />
          <span>Back</span>
        </button>

        <button
          type="button"
          onClick={onConfirm}
          disabled={!canConfirm || isSubmitting}
          className="inline-flex h-11 items-center justify-center gap-2 rounded-xl bg-[#0e9f6e] px-6 text-[13px] font-semibold text-white shadow-sm transition hover:bg-[#0b8259] disabled:cursor-not-allowed disabled:opacity-40"
        >
          {isSubmitting ? (
            <>
              <Loader2 className="h-4 w-4 animate-spin" />
              <span>Recording Consumption...</span>
            </>
          ) : (
            <>
              <CheckCircle2 className="h-4 w-4" />
              <span>Confirm Consumption</span>
            </>
          )}
        </button>
      </div>
    </div>
  );
};
