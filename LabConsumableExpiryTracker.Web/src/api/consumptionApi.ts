import { apiClient } from "./authApi";
import type { ServiceResult } from "../types/job.types";
import type {
  CreateDraftConsumptionPayload,
  DraftConsumptionResponseDto,
  FefoAllocationPreviewDto,
  RequestFefoAllocationPayload,
  VerifyScanPayload,
  VerifyScanResponseDto,
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

  createDraft: async (
    jobId: string,
    payload: CreateDraftConsumptionPayload
  ): Promise<DraftConsumptionResponseDto> => {
    const response = await apiClient.post<ServiceResult<DraftConsumptionResponseDto>>(
      `/jobs/${jobId}/consumptions/draft`,
      payload
    );

    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to create draft consumption");
    }

    return response.data.data;
  },

  verifyScan: async (
    jobId: string,
    payload: VerifyScanPayload
  ): Promise<VerifyScanResponseDto> => {
    const response = await apiClient.post<ServiceResult<VerifyScanResponseDto>>(
      `/jobs/${jobId}/consumptions/verify-scan`,
      payload
    );

    if (!response.data.success || !response.data.data) {
      // Code INSUFFICIENT_STOCK = draft kalah race (D-15), stok sudah habis
      // oleh scan orang lain. Pesan server apa adanya, code buat refresh preview.
      throw Object.assign(
        new Error(response.data.message || "Failed to verify scan"),
        { code: response.data.code }
      );
    }

    return response.data.data;
  },
};
