import { isAxiosError } from "axios";
import { apiClient } from "./authApi";
import type { ServiceResult } from "../types/auth.types";
import {
  ApiRequestError,
  getApiErrorMessage,
  type ApiErrorPayload,
  type PageResult,
} from "../types/api";
import type {
  Disposal,
  CreateDisposalRequest,
  DisposalListParams,
  CreateDisposalResponse,
} from "../types/disposal.types";

const toDisposalRequestError = (error: unknown): Error => {
  if (!isAxiosError<ApiErrorPayload>(error)) {
    return error instanceof Error
      ? error
      : new Error("An unexpected error occurred while disposing the lot.");
  }
  const payload = error.response?.data;
  return new ApiRequestError(
    payload ? getApiErrorMessage(payload) : error.message,
    payload,
  );
};

export const disposalApi = {
  getDisposals: async (
    params?: DisposalListParams,
  ): Promise<PageResult<Disposal>> => {
    const response = await apiClient.get<ServiceResult<PageResult<Disposal>>>(
      "/disposals",
      { params },
    );
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to fetch disposals.");
    }
    return response.data.data;
  },

  getDisposalById: async (id: string): Promise<Disposal> => {
    const response = await apiClient.get<ServiceResult<Disposal>>(
      `/disposals/${id}`,
    );
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to fetch disposal.");
    }
    return response.data.data;
  },

  createDisposal: async (
    lotId: string,
    payload: CreateDisposalRequest,
  ): Promise<CreateDisposalResponse> => {
    try {
      const response = await apiClient.post<
        ServiceResult<CreateDisposalResponse>
      >(`/lot/${lotId}/disposals`, payload);
      if (!response.data.success || !response.data.data) {
        throw new Error(response.data.message || "Failed to create disposal.");
      }
      return response.data.data;
    } catch (error) {
      throw toDisposalRequestError(error);
    }
  },
};
