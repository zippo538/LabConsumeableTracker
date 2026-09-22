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
  Lot,
  LotSummary,
  CreateLotRequest,
  UpdateLotRequest,
  ItemOption,
} from "../types/lot.types";

const toLotRequestError = (error: unknown): Error => {
  if (!isAxiosError<ApiErrorPayload>(error)) {
    return error instanceof Error
      ? error
      : new Error("An unexpected error occurred while saving the lot.");
  }

  const payload = error.response?.data;
  return new ApiRequestError(
    payload ? getApiErrorMessage(payload) : error.message,
    payload,
  );
};

export const lotApi = {
  getLots: async (params?: {
    page?: number;
    pageSize?: number;
    search?: string;
    itemId?: string;
    administrativeStatus?: string;
    expiryCondition?: string;
  }): Promise<PageResult<Lot>> => {
    const response = await apiClient.get<ServiceResult<PageResult<Lot>>>(
      "/Lot/paged",
      {
        params: {
          Page: params?.page || 1,
          PageSize: params?.pageSize || 20,
          Search: params?.search || undefined,
          ItemId: params?.itemId || undefined,
          AdministrativeStatus: params?.administrativeStatus || undefined,
          ExpiryCondition: params?.expiryCondition || undefined,
        },
      },
    );
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to fetch lots.");
    }
    return response.data.data;
  },

  getLotSummary: async (): Promise<LotSummary> => {
    const response = await apiClient.get<ServiceResult<Lot[]>>("/Lot");

    if (!response.data.success || !response.data.data) {
      throw new Error(
        response.data.message || "Failed to fetch lots for summary.",
      );
    }

    const lots = response.data.data;

    let activeLotCount = 0;
    let expiringSoonLotCount = 0;
    let notEligibleLotCount = 0;
    let latestEvaluatedAt = "";

    for (const lot of lots) {
      // 1. Hitung Active
      if (lot.administrativeStatus === "Active") activeLotCount++;

      // 2. Hitung ExpiringSoon
      if (lot.expiryCondition === "ExpiringSoon") expiringSoonLotCount++;

      // 3. Hitung Not Eligible
      if (!lot.isEligible) notEligibleLotCount++;

      // 4. Cari timestamp evaluasi terbaru
      if (!latestEvaluatedAt || lot.evaluatedAt > latestEvaluatedAt) {
        latestEvaluatedAt = lot.evaluatedAt;
      }
    }

    return {
      activeLotCount,
      expiringSoonLotCount,
      notEligibleLotCount,
      evaluatedAt: latestEvaluatedAt || new Date().toISOString(),
    };
  },

  getItems: async (): Promise<ItemOption[]> => {
    const response = await apiClient.get<ServiceResult<ItemOption[]>>("/Item");
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to fetch items.");
    }
    return response.data.data;
  },

  getLotById: async (id: string): Promise<Lot> => {
    const response = await apiClient.get<ServiceResult<Lot>>(`/Lot/${id}`);
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to fetch lot.");
    }
    return response.data.data;
  },

  createLot: async (payload: CreateLotRequest): Promise<Lot> => {
    try {
      const response = await apiClient.post<ServiceResult<Lot>>(
        "/Lot",
        payload,
      );
      if (!response.data.success || !response.data.data) {
        throw new Error(response.data.message || "Failed to create lot.");
      }
      return response.data.data;
    } catch (error) {
      throw toLotRequestError(error);
    }
  },

  updateLot: async (id: string, payload: UpdateLotRequest): Promise<Lot> => {
    try {
      const response = await apiClient.put<ServiceResult<Lot>>(
        `/lot/${id}`,
        payload,
      );
      if (!response.data.success || !response.data.data) {
        throw new Error(response.data.message || "Failed to update lot.");
      }
      return response.data.data;
    } catch (error) {
      throw toLotRequestError(error);
    }
  },

  deleteLot: async (id: string): Promise<void> => {
    const response = await apiClient.delete<ServiceResult<boolean>>(
      `/lot/${id}`,
    );
    if (!response.data.success) {
      throw new Error(response.data.message || "Failed to delete lot.");
    }
  },

  // Block lot
  blockLot: async (id: string, reason: string): Promise<Lot> => {
    const response = await apiClient.post<ServiceResult<Lot>>(
      `/lots/${id}/block`,
      { reason },
    );
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to block lot.");
    }
    return response.data.data;
  },

  // Quarantine lot
  quarantineLot: async (id: string, reason: string): Promise<Lot> => {
    const response = await apiClient.post<ServiceResult<Lot>>(
      `/lots/${id}/quarantine`,
      { reason },
    );
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to quarantine lot.");
    }
    return response.data.data;
  },

  // Release lot
  releaseLot: async (id: string): Promise<Lot> => {
    const response = await apiClient.post<ServiceResult<Lot>>(
      `/Lots/${id}/release`,
    );
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to release lot.");
    }
    return response.data.data;
  },
};
