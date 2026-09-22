import { Link, useParams } from "react-router";
import { AppLayout } from "../components/layout/AppLayout";
import { DetailLot } from "../components/lots/DetailLot";
import { useLotDetail } from "../hooks/useLotDetail";


export function LotDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { data: lot, isLoading, isError, error } = useLotDetail(id);

  return (
    <AppLayout
      title="Detail Lot"
      description={
        lot
          ? `Detail Lot ${lot.lotNumber}`
          : "Detail Lot....."
      }
    >
      <Link
        to="/app/lots"
        className="mb-5 inline-flex text-sm font-medium text-teal-700 hover:text-teal-800"
      >
        ← Back to lots
      </Link>

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
    </AppLayout>
  );
}
