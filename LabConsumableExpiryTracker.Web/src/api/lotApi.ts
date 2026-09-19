import { apiClient } from "./authApi";
import type { ServiceResult } from "../types/auth.types";
import type { PageResult } from "../types/api";
import type {
  Lot,
  LotSummary,
  CreateLotRequest,
  UpdateLotRequest,
  LotAdministrativeStatus,
  ExpiryCondition,
  ItemOption,
} from "../types/lot.types";

export const lotApi = {
  // Get lots with pagination and filters
  getLots: async (params?: {
    page?: number;
    pageSize?: number;
    search?: string;
    itemId?: string;
    status?: LotAdministrativeStatus | string;
    expiryCondition?: ExpiryCondition | string;
  }): Promise<PageResult<Lot>> => {
    const response = await apiClient.get<ServiceResult<PageResult<Lot>>>(
      "/lots/paged",
      { params },
    );
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to fetch lots.");
    }
    return response.data.data;
  },

  // Get lot summary for dashboard cards
  getLotSummary: async (): Promise<LotSummary> => {
    const response =
      await apiClient.get<ServiceResult<LotSummary>>("/lots/summary");
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to fetch lot summary.");
    }
    return response.data.data;
  },

  // Get items for dropdown
  getItems: async (): Promise<ItemOption[]> => {
    const response = await apiClient.get<ServiceResult<ItemOption[]>>("/items");
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to fetch items.");
    }
    return response.data.data;
  },

  // Get single lot by ID
  getLotById: async (id: string): Promise<Lot> => {
    const response = await apiClient.get<ServiceResult<Lot>>(`/lots/${id}`);
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to fetch lot.");
    }
    return response.data.data;
  },

  // Create new lot
  createLot: async (payload: CreateLotRequest): Promise<Lot> => {
    const response = await apiClient.post<ServiceResult<Lot>>("/lots", payload);
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to create lot.");
    }
    return response.data.data;
  },

  // Update lot
  updateLot: async (id: string, payload: UpdateLotRequest): Promise<Lot> => {
    const response = await apiClient.put<ServiceResult<Lot>>(
      `/lots/${id}`,
      payload,
    );
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to update lot.");
    }
    return response.data.data;
  },

  // Delete lot
  deleteLot: async (id: string): Promise<void> => {
    const response = await apiClient.delete<ServiceResult<boolean>>(
      `/lots/${id}`,
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
      `/lots/${id}/release`,
    );
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to release lot.");
    }
    return response.data.data;
  },
};
