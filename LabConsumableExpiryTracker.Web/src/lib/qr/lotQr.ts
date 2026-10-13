/**
 * F-16: format isi QR Lot.
 *
 * Payload: `LCET|<ItemId>|<LotId>|<LotNumber>`
 *
 * Prefix `LCET` berfungsi sebagai version marker. Format baru wajib menaikkan
 * versi (mis. `LCET2|...`) supaya parser bisa menerima keduanya selama transisi.
 *
 * `ponytail:` LotNumber tidak divalidasi tolak `|` di level Domain (Lot hanya
 * Trim()). Payload dengan `|` di lotNumber jadi punya field > 4 dan ditolak
 * parser di bawah dengan pesan jelas, bukan lolos ke server. Tambahkan
 * validasi karakter di Lot.Create/UpdateLotNumber kalau nomor lot dari supplier
 * memang boleh mengandung pipe.
 */

const PREFIX = "LCET";
const FIELD_COUNT = 4;

export type LotQrPayload = {
  itemId: string;
  lotId: string;
  lotNumber: string;
};

export function buildLotQrPayload(lot: {
  id: string;
  itemId: string;
  lotNumber: string;
}): string {
  return [PREFIX, lot.itemId, lot.id, lot.lotNumber].join("|");
}

/**
 * Mengembalikan null bila payload bukan format LCET yang valid. Sengaja tidak
 * menebak-nebak field: pemanggil harus menampilkan "barcode tidak valid" dan
 * TIDAK mengirim request ke verify-scan.
 */
export function parseLotQrPayload(raw: string): LotQrPayload | null {
  if (typeof raw !== "string") {
    return null;
  }

  const parts = raw.trim().split("|");

  if (parts.length !== FIELD_COUNT || parts[0] !== PREFIX) {
    return null;
  }

  const [, itemId, lotId, lotNumber] = parts;

  if (!itemId || !lotId || !lotNumber) {
    return null;
  }

  return { itemId, lotId, lotNumber };
}
