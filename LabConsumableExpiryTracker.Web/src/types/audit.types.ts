export type AuditEventType = "Consumption" | "Disposal";

export interface AuditEvent {
  eventType: AuditEventType;
  eventId: string;
  itemId: string;
  lotId: string;
  lotNumber: string;
  itemName: string;
  itemCode: string;
  quantity: number;
  baseUnit: string;
  occurredAt: string;
  actorId: string;
  fullName: string;
  jobId: string | null;
  jobNumber: string | null;
  reason: string | null;
}

export interface AuditEventListResponse {
  items: AuditEvent[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface AuditFilter {
  eventType: AuditEventType | "All";
  itemId: string | null;
  lotId: string | null;
  jobId: string | null;
  from: string | null;
  to: string | null;
  search: string | null;
  page: number;
  pageSize: number;
}

export const DEFAULT_AUDIT_FILTER: AuditFilter = {
  eventType: "All",
  itemId: null,
  lotId: null,
  jobId: null,
  from: null,
  to: null,
  search : "",
  page: 1,
  pageSize: 20,
};

export interface LookupOption {
  id: string;
  label: string;
  sublabel?: string;
}

export interface AuditStats {
  allEvents: number;
  consumption: number;
  disposal: number;
}

export type AuditPageState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "success"; data: AuditEventListResponse; stats: AuditStats }
  | { status: "empty" }
  | { status: "error"; message: string; code?: number }
  | { status: "forbidden" }
  | { status: "unauthorized" };