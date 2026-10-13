import React, { useState } from "react";
import {
  CheckCircle2,
  ArrowLeft,
  Loader2,
  ScanLine,
  FileText,
  AlertTriangle,
} from "lucide-react";
import { parseLotQrPayload } from "../../lib/qr/lotQr";

interface ConsumptionConfirmationProps {
  /** null = belum ada draft. String[] = nomor lot yang menunggu scan. */
  pendingLotNumbers: string[] | null;
  isDrafting: boolean;
  isVerifying: boolean;
  onCreateDraft: () => void;
  onVerifyScan: (lotNumber: string) => void;
  onBack: () => void;
}

export const ConsumptionConfirmation: React.FC<ConsumptionConfirmationProps> = ({
  pendingLotNumbers,
  isDrafting,
  isVerifying,
  onCreateDraft,
  onVerifyScan,
  onBack,
}) => {
  const [lotNumber, setLotNumber] = useState("");
  const [scanError, setScanError] = useState<string | null>(null);

  const hasDraft = pendingLotNumbers !== null;
  const trimmed = lotNumber.trim();

  // Scanner barcode tembak isi QR utuh ("LCET|itemId|lotId|lotNumber"), bukan
  // cuma nomor lot. Parser di sini yang menerjemahkan ke lotNumber; kalau
  // payload tidak valid, tolak sebelum request, jangan kirim apa adanya ke
  // server yang akan membalas LOT_NOT_FOUND.
  const handleVerify = () => {
    if (!trimmed) return;

    const parsed = parseLotQrPayload(trimmed);
    const value = parsed ? parsed.lotNumber : trimmed;

    // Kalau string mengandung "|" tapi gagal parse, itu QR rusak — jangan
    // diteruskan sebagai nomor lot.
    if (!parsed && trimmed.includes("|")) {
      setScanError(
        `Barcode tidak valid: "${trimmed}". Format QR lot yang benar adalah LCET|ItemId|LotId|LotNumber.`
      );
      return;
    }

    setScanError(null);
    onVerifyScan(value);
  };

  return (
    <div className="flex flex-col rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
      <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
        <div className="max-w-xl">
          <h2 className="text-[16px] font-bold tracking-tight text-slate-900">
            3. Verify consumption
          </h2>
          <p className="mt-1 text-[12px] text-slate-500 leading-relaxed">
            Stock only decreases when a lot barcode is scanned. Create a draft
            first, then scan each lot physically taken from storage.
          </p>
        </div>

        <div className="flex items-center gap-3 shrink-0">
          <button
            type="button"
            onClick={onBack}
            disabled={isDrafting || isVerifying}
            className="inline-flex h-11 items-center gap-2 rounded-xl border border-slate-200 bg-white px-5 text-[12px] font-semibold text-slate-700 transition hover:bg-slate-50 hover:border-slate-300 disabled:opacity-50"
          >
            <ArrowLeft className="h-4 w-4" />
            <span>Back</span>
          </button>

          {!hasDraft && (
            <button
              type="button"
              onClick={onCreateDraft}
              disabled={isDrafting || isVerifying}
              className="inline-flex h-11 items-center justify-center gap-2 rounded-xl bg-[#0e9f6e] px-6 text-[13px] font-semibold text-white shadow-sm transition hover:bg-[#0b8259] disabled:cursor-not-allowed disabled:opacity-40"
            >
              {isDrafting ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  <span>Creating Draft...</span>
                </>
              ) : (
                <>
                  <FileText className="h-4 w-4" />
                  <span>Create Draft</span>
                </>
              )}
            </button>
          )}
        </div>
      </div>

      {hasDraft && (
        <div className="mt-5 rounded-xl border border-[#b9e8eb] bg-[#eafafd] p-4">
          <div className="flex items-start gap-3">
            <ScanLine className="mt-0.5 h-4 w-4 shrink-0 text-[#11a9ae]" />

            <div className="min-w-0 flex-1">
              <p className="text-[12px] font-semibold text-[#14546a]">
                Scan each lot below to release stock
              </p>

              <div className="mt-3 flex flex-wrap gap-2">
                {pendingLotNumbers.map((n) => (
                  <span
                    key={n}
                    className="rounded-full border border-[#b9e8eb] bg-white px-3 py-1 text-[11px] font-bold tracking-wide text-[#0da7ad]"
                  >
                    {n}
                  </span>
                ))}
              </div>

              <div className="mt-4 flex flex-col gap-2 sm:flex-row">
                <input
                  type="text"
                  value={lotNumber}
                  onChange={(e) => {
                    setLotNumber(e.target.value);
                    setScanError(null);
                  }}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      handleVerify();
                    }
                  }}
                  aria-invalid={scanError ? true : undefined}
                  placeholder="Scan QR label or type lot number..."
                  className="h-10 flex-1 rounded-lg border border-slate-200 bg-white px-3 text-[13px] text-slate-800 outline-none focus:border-[#12a9ad] focus:ring-2 focus:ring-[#12a9ad]/20"
                />

                <button
                  type="button"
                  onClick={handleVerify}
                  disabled={!trimmed || isVerifying}
                  className="inline-flex h-10 items-center justify-center gap-2 rounded-lg bg-[#0e9f6e] px-5 text-[12px] font-semibold text-white transition hover:bg-[#0b8259] disabled:cursor-not-allowed disabled:opacity-40"
                >
                  {isVerifying ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      <span>Verifying...</span>
                    </>
                  ) : (
                    <>
                      <CheckCircle2 className="h-4 w-4" />
                      <span>Verify Scan</span>
                    </>
                  )}
                </button>
              </div>

              {scanError && (
                <p className="mt-2 flex items-start gap-1.5 text-[12px] font-medium text-red-700">
                  <AlertTriangle className="mt-0.5 h-3.5 w-3.5 shrink-0" />
                  <span>{scanError}</span>
                </p>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
