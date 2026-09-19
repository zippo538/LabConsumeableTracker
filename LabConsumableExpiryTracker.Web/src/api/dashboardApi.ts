import { apiClient } from "./authApi";
import type { ServiceResult } from "../types/auth.types";
import type { PageResult } from "../types/api";
import type {
  DashboardAlert,
  DashboardAlertQuery,
  DashboardSummary,
} from "../types/dashboard.types";

export const getDashboardSummary = async (
  itemId?: string,
): Promise<DashboardSummary> => {
  const response = await apiClient.get<ServiceResult<DashboardSummary>>(
    "/dashboard/summary",
    {
      params: itemId ? { ItemId: itemId } : undefined,
    },
  );

  if (!response.data.success || !response.data.data) {
    throw new Error(
      response.data.message || "Gagal memuat ringkasan dashboard.",
    );
  }

  return response.data.data;
};

export const getDashboardAlerts = async (
  query?: DashboardAlertQuery,
): Promise<PageResult<DashboardAlert>> => {
  const response = await apiClient.get<
    ServiceResult<PageResult<DashboardAlert>>
  >("/dashboard/alerts", {
    params: query,
  });

  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || "Gagal memuat alert dashboard.");
  }

  return response.data.data;
};
