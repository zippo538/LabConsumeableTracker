import QRCode from "react-qr-code";
import { buildLotQrPayload } from "../../lib/qr/lotQr";
import type { Lot } from "../../types/lot.types";

type LotQrProps = {
  lot: Pick<Lot, "id" | "itemId" | "lotNumber">;
  size?: number;
  /** Level koreksi error. H untuk label cetak (ukuran kecil, perlu H). */
  level?: "L" | "M" | "Q" | "H";
  className?: string;
  /** Judul untuk aksesibilitas; Defaults ke nomor lot. */
  title?: string;
};

/**
 * F-16: QR label untuk satu Lot. isi QR dibangun dari lot.id, lot.itemId,
 * dan lot.lotNumber — tidak ada kolom DB tambahan.
 */
export function LotQr({
  lot,
  size = 40,
  level = "M",
  className = "",
  title,
}: LotQrProps) {
  return (
    <QRCode
      value={buildLotQrPayload(lot)}
      size={size}
      level={level}
      title={title ?? `QR code for lot ${lot.lotNumber}`}
      bgColor="#FFFFFF"
      fgColor="#0F172A"
      className={className}
    />
  );
}
