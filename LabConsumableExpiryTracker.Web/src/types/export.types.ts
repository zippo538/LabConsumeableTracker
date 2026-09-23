export type ExportType = "consumptions" | "disposals" | "stock";

export const EXPORT_TYPE_LABELS: Record<ExportType, string> = {
  consumptions: "Consumption",
  disposals: "Disposal",
  stock: "Item Stock",
};

export interface ExportConsumptionsFilter {
  format: "csv";
  jobId?: string;
  itemId?: string;
  lotId?: string;
  from?: string;
  to?: string;
}

export interface ExportDisposalsFilter {
  format: "csv";
  itemId?: string;
  itemName? : string;
  lotId?: string;
  from?: string;
  to?: string;
}

export interface ExportStockFilter {
  format: "csv";
  itemId?: string;
  isLowStock?: boolean;
}

export type ExportFilter =
  | ExportConsumptionsFilter
  | ExportDisposalsFilter
  | ExportStockFilter;

export interface ExportResponse {
  blob: Blob;
  filename: string;
  contentType: string;
}

export type ExportState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "success"; filename: string }
  | { status: "error"; message: string };

export interface AuditFilterToExportMapping {
  from: string | null;
  to: string | null;
  itemId: string | null;
  lotId: string | null;
  jobId: string | null;
}