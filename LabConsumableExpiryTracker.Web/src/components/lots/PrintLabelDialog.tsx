import { Printer, X } from "lucide-react";
import { LotLabelPrintSheet } from "./LotLabelPrintSheet";
import type { Lot } from "../../types/lot.types";

type PrintableLot = Pick<
  Lot,
  "id" | "itemId" | "lotNumber" | "itemName" | "expiryDate" | "storageLocation"
>;

type PrintLabelDialogProps = {
  lots: PrintableLot[];
  onClose: () => void;
};

/**
 * F-16: dialog cetak label. Sengaja modal di halaman yang sama, bukan route
 * terpisah — LotPage sudah memegang data lot, jadi tidak perlu mengirim state
 * lewat navigasi. `ponytail:` kalau nanti butuh share-link hasil cetak,
 * pindah ke route dengan ID di query string.
 */
export function PrintLabelDialog({ lots, onClose }: PrintLabelDialogProps) {
  if (lots.length === 0) {
    return null;
  }

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-slate-900/50 p-6">
      <div className="lot-label-print-root w-full max-w-4xl rounded-2xl bg-white p-6 shadow-xl">
        <div className="lot-label-print-controls mb-4 flex items-center justify-between">
          <div>
            <h2 className="text-[16px] font-bold text-slate-900">
              Print {lots.length} label
            </h2>
            <p className="text-[12px] text-slate-500">
              Gunakan dialog print browser, pilih A4 dan skala 100%.
            </p>
          </div>

          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => window.print()}
              className="inline-flex h-10 items-center gap-2 rounded-lg bg-[#0e9f6e] px-4 text-[12px] font-semibold text-white transition hover:bg-[#0b8259]"
            >
              <Printer className="h-4 w-4" />
              <span>Print</span>
            </button>

            <button
              type="button"
              onClick={onClose}
              aria-label="Close print preview"
              className="grid h-10 w-10 place-items-center rounded-lg border border-slate-200 text-slate-500 transition hover:bg-slate-50"
            >
              <X className="h-4 w-4" />
            </button>
          </div>
        </div>

        <LotLabelPrintSheet lots={lots} />
      </div>
    </div>
  );
}
