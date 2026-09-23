import { apiClient } from "./authApi";
import type { ServiceResult } from "../types/job.types";
import type {
    ConsumptionHistoryDetailResponse,
  ConsumptionRecordDto,
  GetConsumptionHistoryParams,
  PagedResult,
} from "../types/consumptionHistory.types";

export const getConsumptionHistory = async (
  params?: GetConsumptionHistoryParams
): Promise<PagedResult<ConsumptionRecordDto>> => {
  const response = await apiClient.get<
    ServiceResult<PagedResult<ConsumptionRecordDto>>
  >("/consumptions-history/me", { params });

  return response.data.data;
};

export const getConsumptionHistoryDetail = async (
  id: string
): Promise<ConsumptionHistoryDetailResponse> => {
  const response = await apiClient.get(`/consumptions-history/${id}`);

  return response.data;
};