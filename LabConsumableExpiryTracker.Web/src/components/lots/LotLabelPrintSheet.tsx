import { LotQr } from "./LotQr";
import type { Lot } from "../../types/lot.types";

const formatExpiry = (dateStr: string) => {
  if (!dateStr) return "-";

  const d = new Date(dateStr);

  return Number.isNaN(d.getTime())
    ? dateStr
    : d.toLocaleDateString("en-GB", {
        day: "numeric",
        month: "short",
        year: "numeric",
      });
};

type LotLabelPrintSheetProps = {
  lots: Array<
    Pick<
      Lot,
      | "id"
      | "itemId"
      | "lotNumber"
      | "itemName"
      | "expiryDate"
      | "storageLocation"
    >
  >;
};

/**
 * F-16: sheet cetak label. Hanya elemen ini yang tampil saat print; seluruh
 * halaman aplikasi disembunyikan lewat `@media print` di index.css.
 */
export function LotLabelPrintSheet({ lots }: LotLabelPrintSheetProps) {
  return (
    <div className="lot-label-print-sheet">
      {lots.map((lot) => (
        <div
          key={lot.id}
          className="lot-label flex flex-col items-center justify-center gap-2 border border-slate-300 bg-white p-3 text-center"
        >
          <LotQr lot={lot} size={120} level="H" />

          <p className="font-mono text-[13px] font-bold tracking-wide text-slate-900">
            {lot.lotNumber}
          </p>

          <p className="text-[10px] text-slate-600">{lot.itemName}</p>

          <div className="flex flex-col gap-0.5 text-[9px] text-slate-500">
            <span>Exp: {formatExpiry(lot.expiryDate)}</span>
            <span>Loc: {lot.storageLocation || "-"}</span>
          </div>
        </div>
      ))}
    </div>
  );
}
