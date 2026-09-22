import { apiClient } from "./authApi";
import type { ServiceResult } from "../types/job.types";
import type {
  ConfirmConsumptionPayload,
  ConsumptionResponseDto,
  FefoAllocationPreviewDto,
  RequestFefoAllocationPayload,
} from "../types/consumption.types";

export const consumptionApi = {
  previewAllocation: async (
    jobId: string,
    payload: RequestFefoAllocationPayload
  ): Promise<FefoAllocationPreviewDto> => {
    const response = await apiClient.post<ServiceResult<FefoAllocationPreviewDto>>(
      `/jobs/${jobId}/allocations/preview`,
      payload
    );

    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to preview FEFO allocation");
    }

    return response.data.data;
  },

  confirmConsumption: async (
    jobId: string,
    payload: ConfirmConsumptionPayload
  ): Promise<ConsumptionResponseDto> => {
    const response = await apiClient.post<ServiceResult<ConsumptionResponseDto>>(
      `/jobs/${jobId}/consumptions`,
      payload
    );

    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to confirm consumption");
    }

    return response.data.data;
  },
};
