import { apiClient } from "./authApi";
import type {
  ExportType,
  ExportConsumptionsFilter,
  ExportDisposalsFilter,
  ExportStockFilter,
  ExportResponse,
} from "../types/export.types";

function parseContentDisposition(
  header: string | undefined,
  defaultType: ExportType,
): string {
  if (!header) {
    return `export-${defaultType}-${new Date().toISOString().slice(0, 10)}.csv`;
  }

  const filenameMatch = header.match(
    /filename\*?=(?:UTF-8''|)?["']?([^"';]+)["']?/i,
  );
  if (filenameMatch && filenameMatch[1]) {
    return decodeURIComponent(filenameMatch[1].trim());
  }

  return `export-${defaultType}-${new Date().toISOString().slice(0, 10)}.csv`;
}

function buildConsumptionsParams(
  filter: ExportConsumptionsFilter,
): URLSearchParams {
  const params = new URLSearchParams();
  params.set("Format", filter.format);

  if (filter.jobId) params.set("JobId", filter.jobId);
  if (filter.itemId) params.set("ItemId", filter.itemId);
  if (filter.lotId) params.set("LotId", filter.lotId);
  if (filter.from) params.set("From", filter.from);
  if (filter.to) params.set("To", filter.to);

  return params;
}

function buildDisposalsParams(filter: ExportDisposalsFilter): URLSearchParams {
  const params = new URLSearchParams();
  params.set("Format", filter.format);

  if (filter.itemId) params.set("ItemId", filter.itemId);
  if (filter.lotId) params.set("LotId", filter.lotId);
  if (filter.from) params.set("From", filter.from);
  if (filter.to) params.set("To", filter.to);

  return params;
}

function buildStockParams(filter: ExportStockFilter): URLSearchParams {
  const params = new URLSearchParams();
  params.set("Format", filter.format);

  if (filter.itemId) params.set("ItemId", filter.itemId);
  if (filter.isLowStock !== undefined) {
    params.set("IsLowStock", String(filter.isLowStock));
  }

  return params;
}
export async function exportConsumptionsCsv(
  filter: ExportConsumptionsFilter,
): Promise<ExportResponse> {
  const params = buildConsumptionsParams(filter);
  const url = `/exports/consumptions?${params.toString()}`;

  const response = await apiClient.get(url, {
    responseType: "blob",
    headers: {
      Accept: "text/csv",
    },
  });

  const contentType =
    (response.headers["content-type"] as string) || "text/csv";
  const contentDisposition = response.headers["content-disposition"] as
    | string
    | undefined;
  const filename = parseContentDisposition(contentDisposition, "consumptions");

  return {
    blob: response.data,
    filename,
    contentType,
  };
}

export async function exportDisposalsCsv(
  filter: ExportDisposalsFilter,
): Promise<ExportResponse> {
  const params = buildDisposalsParams(filter);
  const url = `/exports/disposals?${params.toString()}`;

  const response = await apiClient.get(url, {
    responseType: "blob",
    headers: {
      Accept: "text/csv",
    },
  });

  const contentType =
    (response.headers["content-type"] as string) || "text/csv";
  const contentDisposition = response.headers["content-disposition"] as
    | string
    | undefined;
  const filename = parseContentDisposition(contentDisposition, "consumptions");

  return {
    blob: response.data,
    filename,
    contentType,
  };
}

export async function exportStockCsv(
  filter: ExportStockFilter,
): Promise<ExportResponse> {
  const params = buildStockParams(filter);
  const url = `/exports/stock?${params.toString()}`;

  const response = await apiClient.get(url, {
    responseType: "blob",
    headers: {
      Accept: "text/csv",
    },
  });

  const contentType =
    (response.headers["content-type"] as string) || "text/csv";
  const contentDisposition = response.headers["content-disposition"] as
    | string
    | undefined;
  const filename = parseContentDisposition(contentDisposition, "consumptions");

  return {
    blob: response.data,
    filename,
    contentType,
  };
}

export function triggerFileDownload(blob: Blob, filename: string): void {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = filename;
  link.style.display = "none";

  document.body.appendChild(link);
  link.click();

  // Cleanup
  document.body.removeChild(link);
  window.URL.revokeObjectURL(url);
}

export function parseExportError(error: unknown): {
  message: string;
  code?: number;
} {
  if (error instanceof Error) {
    if (error.message.includes("401")) {
      return { message: "Sesi berakhir. Silakan login kembali.", code: 401 };
    }
    if (error.message.includes("403")) {
      return { message: "Anda tidak memiliki akses untuk export.", code: 403 };
    }
    if (error.message.includes("400")) {
      return {
        message: "Filter export tidak valid. Periksa rentang tanggal.",
        code: 400,
      };
    }
    if (
      error.message.includes("Failed to fetch") ||
      error.message.includes("NetworkError")
    ) {
      return {
        message: "Koneksi jaringan gagal. Periksa koneksi Anda.",
        code: 0,
      };
    }
    return { message: error.message };
  }
  return { message: "Terjadi kesalahan yang tidak terduga." };
}
