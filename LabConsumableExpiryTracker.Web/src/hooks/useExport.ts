import { useState, useCallback, useRef } from "react";
import {
  exportConsumptionsCsv,
  exportDisposalsCsv,
  exportStockCsv,
  triggerFileDownload,
  parseExportError,
} from "../api/export.api";
import type {
  ExportType,
  ExportState,
} from "../types/export.types";

/**
 * Helper: Generate nama file berdasarkan tipe dan tanggal hari ini.
 * PERBAIKAN: Selalu gunakan nilai dari dropdown, abaikan filename dari API.
 */
const generateFilename = (type: ExportType): string => {
  const today = new Date().toISOString().slice(0, 10); // Format YYYY-MM-DD
  return `export-${type}-${today}.csv`;
};

export const useExport = () => {
  const [state, setState] = useState<ExportState>({ status: "idle" });
  const resetTimerRef = useRef<number | null>(null);

  const clearResetTimer = useCallback(() => {
    if (resetTimerRef.current) {
      clearTimeout(resetTimerRef.current);
      resetTimerRef.current = null;
    }
  }, []);

  const scheduleReset = useCallback(() => {
    clearResetTimer();
    resetTimerRef.current = window.setTimeout(() => {
      setState({ status: "idle" });
    }, 5000);
  }, [clearResetTimer]);

  const resetState = useCallback(() => {
    clearResetTimer();
    setState({ status: "idle" });
  }, [clearResetTimer]);

  /**
   * Unified export handler.
   * PERBAIKAN: Nama file selalu dari tipe dropdown, bukan dari API response.
   */
  const handleExport = useCallback(
    async (
      type: ExportType,
      filters: {
        from?: string | null;
        to?: string | null;
        itemId?: string | null;
        itemName?: string | null;
        lotId?: string | null;
        jobId?: string | null;
        isLowStock?: boolean;
      }
    ) => {
      // 1. Reset state lama sebelum mulai request baru
      resetState();
      setState({ status: "loading" });

      // 2. PERBAIKAN: Generate nama file di awal berdasarkan tipe dropdown
      const filename = generateFilename(type);

      try {
        let blob: Blob;

        // 3. Panggil API sesuai tipe (hanya ambil blob, abaikan filename dari API)
        switch (type) {
          case "consumptions":
            const consumptionResult = await exportConsumptionsCsv({
              format: "csv",
              jobId: filters.jobId || undefined,
              itemId: filters.itemId || undefined,
              lotId: filters.lotId || undefined,
              from: filters.from || undefined,
              to: filters.to || undefined,
            });
            blob = consumptionResult.blob;
            break;

          case "disposals":
            const disposalResult = await exportDisposalsCsv({
              format: "csv",
              itemId: filters.itemId || undefined,
              itemName: filters.itemName || undefined,
              lotId: filters.lotId || undefined,
              from: filters.from || undefined,
              to: filters.to || undefined,
            });
            blob = disposalResult.blob;
            break;

          case "stock":
            const stockResult = await exportStockCsv({
              format: "csv",
              itemId: filters.itemId || undefined,
              isLowStock: filters.isLowStock,
            });
            blob = stockResult.blob;
            break;

          default:
            throw new Error("Tipe export tidak valid.");
        }

        // 4. PERBAIKAN: Selalu gunakan filename dari dropdown
        triggerFileDownload(blob, filename);

        // 5. Set success dengan nama file yang konsisten
        setState({ status: "success", filename });
        scheduleReset();

      } catch (error) {
        const parsed = parseExportError(error);

        // Jangan tampilkan toast error untuk 401/403
        if (parsed.code !== 401 && parsed.code !== 403) {
          setState({ status: "error", message: parsed.message });
          scheduleReset();
        } else {
          setState({ status: "idle" });
        }
      }
    },
    [resetState, scheduleReset]
  );

  return {
    state,
    handleExport,
    resetState,
    isExporting: state.status === "loading",
  };
};

/**
 * Hook: Format filter audit untuk export
 * Mengkonversi AuditFilter dates ke format ISO 8601 UTC (+00:00)
 */
export const useExportFilterMapper = () => {
  const mapDatesForExport = useCallback(
    (from: string | null, to: string | null) => {
      return {
        from: from ? `${from}T00:00:00+00:00` : undefined,
        to: to ? `${to}T00:00:00+00:00` : undefined,
      };
    },
    []
  );

  return { mapDatesForExport };
};