import { useState } from "react";
import { Link, useParams } from "react-router";
import { Printer } from "lucide-react";
import { AppLayout } from "../components/layout/AppLayout";
import { DetailLot } from "../components/lots/DetailLot";
import { PrintLabelDialog } from "../components/lots/PrintLabelDialog";
import { useLotDetail } from "../hooks/useLotDetail";


export function LotDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { data: lot, isLoading, isError, error } = useLotDetail(id);
  // F-16: cetak label QR untuk lot ini saja.
  const [isPrintOpen, setIsPrintOpen] = useState(false);

  return (
    <AppLayout
      title="Detail Lot"
      description={
        lot
          ? `Detail Lot ${lot.lotNumber}`
          : "Detail Lot....."
      }
    >
      <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
        <Link
          to="/app/lots"
          className="inline-flex text-sm font-medium text-teal-700 hover:text-teal-800"
        >
          ← Back to lots
        </Link>

        {lot && (
          <button
            type="button"
            onClick={() => setIsPrintOpen(true)}
            className="inline-flex h-10 items-center gap-2 rounded-lg bg-[#0e9f6e] px-4 text-[12px] font-semibold text-white transition hover:bg-[#0b8259]"
          >
            <Printer className="h-4 w-4" />
            <span>Print Label</span>
          </button>
        )}
      </div>

      {isLoading && (
        <section className="rounded-2xl border border-slate-200 bg-white p-6 text-sm text-slate-500 shadow-sm">
          Memuat detail lot...
        </section>
      )}

      {!id && (
        <section className="rounded-2xl border border-red-200 bg-red-50 p-6 text-sm text-red-700">
          not valid Lot ID .
        </section>
      )}

      {id && isError && (
        <section className="rounded-2xl border border-red-200 bg-red-50 p-6 text-sm text-red-700">
          {error instanceof Error
            ? error.message
            : "Detail lot tidak dapat dimuat. Silakan coba lagi."}
        </section>
      )}

      {lot && <DetailLot lot={lot} />}

      {lot && isPrintOpen && (
        <PrintLabelDialog
          lots={[lot]}
          onClose={() => setIsPrintOpen(false)}
        />
      )}
    </AppLayout>
  );
}
