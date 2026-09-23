import { useQuery, keepPreviousData } from "@tanstack/react-query";

import { getConsumptionHistory, getConsumptionHistoryDetail } from "../api/consumptionHistoryApi";
import type { GetConsumptionHistoryParams } from "../types/consumptionHistory.types";

export const useConsumptionHistory = (
  params?: GetConsumptionHistoryParams
) => {
  return useQuery({
    queryKey: ["consumption-history", params],
    queryFn: () => getConsumptionHistory(params),
    placeholderData: keepPreviousData,
  });
};

export const useConsumptionHistoryDetail = (id?: string) => {
  return useQuery({
    queryKey: ["consumption-history-detail", id],
    queryFn: () => getConsumptionHistoryDetail(id!),
    enabled: !!id,
  });
};