import { apiClient } from "./authApi";
import type { ServiceResult } from "../types/auth.types";
import type {
  AuditEventListResponse,
  AuditFilter,
  AuditStats,
  AuditEvent,
} from "../types/audit.types";

function buildAuditQueryParams(filter: AuditFilter): URLSearchParams {
  const params = new URLSearchParams();

  if (filter.eventType !== "All") {
    params.set("EventType", filter.eventType);
  }
  if (filter.itemId) {
    params.set("ItemId", filter.itemId);
  }
  if (filter.lotId) {
    params.set("LotId", filter.lotId);
  }
  if (filter.jobId) {
    params.set("JobId", filter.jobId);
  }
  if (filter.from) {
    params.set("From", filter.from);
  }
  if (filter.to) {
    params.set("To", filter.to);
  }
  if (filter.from) {
    params.set("From", `${filter.from}T00:00:00+00:00`);
  }
  if (filter.to) {
    params.set("To", `${filter.to}T00:00:00+00:00`);
  }

  params.set("Page", String(filter.page));
  params.set("PageSize", String(Math.min(filter.pageSize, 100)));

  return params;
}

export async function fetchAuditEvents(
  filter: AuditFilter,
): Promise<AuditEventListResponse> {
  const params = buildAuditQueryParams(filter);
  const response = await apiClient.get<ServiceResult<AuditEventListResponse>>(
    "/audit-events",
    {
      params,
    },
  );

  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || "Failed to retrieve audit events");
  }

  return response.data.data;
}

export async function fetchConsumptions(
  filter: Omit<AuditFilter, "eventType">,
): Promise<AuditEventListResponse> {
  const params = buildAuditQueryParams({
    ...filter,
    eventType: "Consumption",
  });
  const response = await apiClient.get<ServiceResult<AuditEventListResponse>>(
    "/consumptions",
    {
      params,
    },
  );

  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || "Failed to retrieve consumptions");
  }

  return response.data.data;
}

export async function fetchDisposals(
  filter: Omit<AuditFilter, "eventType">,
): Promise<AuditEventListResponse> {
  const params = buildAuditQueryParams({
    ...filter,
    eventType: "Disposal",
  });
  const response = await apiClient.get<ServiceResult<AuditEventListResponse>>(
    "/disposals",
    {
      params,
    },
  );

  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || "Failed to retrieve disposals");
  }

  return response.data.data;
}

export async function fetchConsumptionDetail(id: string): Promise<AuditEvent> {
  const response = await apiClient.get<ServiceResult<AuditEvent>>(
    `/consumptions/${id}`,
  );

  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || "Consumption detail not found");
  }

  return response.data.data;
}

export async function fetchDisposalDetail(id: string): Promise<AuditEvent> {
  const response = await apiClient.get<ServiceResult<AuditEvent>>(
    `/disposals/${id}`,
  );

  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || "Disposal detail not found");
  }

  return response.data.data;
}

export async function fetchAuditStats(
  filter: AuditFilter,
): Promise<AuditStats> {
  const [allResponse, consumptionResponse, disposalResponse] =
    await Promise.all([
      fetchAuditEvents({ ...filter, eventType: "All", page: 1, pageSize: 1 }),
      fetchAuditEvents({
        ...filter,
        eventType: "Consumption",
        page: 1,
        pageSize: 1,
      }),
      fetchAuditEvents({
        ...filter,
        eventType: "Disposal",
        page: 1,
        pageSize: 1,
      }),
    ]);

  return {
    allEvents: allResponse.totalCount,
    consumption: consumptionResponse.totalCount,
    disposal: disposalResponse.totalCount,
  };
}

export function parseAuditError(error: unknown): {
  message: string;
  code?: number;
} {
  if (error instanceof Error) {
    if (error.message.includes("401"))
      return {
        message: "Sesi berakhir. Silakan login kembali.",
        code: 401,
      };
    if (error.message.includes("403"))
      return {
        message: "Anda tidak memiliki akses ke halaman ini.",
        code: 403,
      };
    if (error.message.includes("404"))
      return {
        message: "Data audit tidak ditemukan.",
        code: 404,
      };
    if (error.message.includes("400"))
      return {
        message: "Filter tidak valid. Periksa kembali input Anda.",
        code: 400,
      };
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
