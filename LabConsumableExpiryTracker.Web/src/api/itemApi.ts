import { apiClient } from "./authApi";

import type {
  ItemDto,
  CreateItemPayload,
  UpdateItemPayload,
} from "../types/item.types";

import type { ServiceResult } from "../types/auth.types";
import type { PagedResult } from "../types/item.types";

export const itemApi = {
  getAll: async (): Promise<ItemDto[]> => {
    const response =
      await apiClient.get<ServiceResult<ItemDto[]>>(
        "/Item",
      );

    return response.data.data ?? [];
  },
  getPaged: async (
    page: number = 1,
    pageSize: number = 20,
  ): Promise<PagedResult<ItemDto>> => {
    const response =
      await apiClient.get<
        ServiceResult<PagedResult<ItemDto>>
      >("/Item/paged", {
        params: {
          page,
          pageSize,
        },
      });

    if (
      !response.data.success ||
      !response.data.data
    ) {
      throw new Error(
        response.data.message ||
          "Failed to retrieve paged items",
      );
    }

    return response.data.data;
  },

  getById: async (
    id: string,
  ): Promise<ItemDto> => {
    const response =
      await apiClient.get<ServiceResult<ItemDto>>(
        `/Item/${id}`,
      );

    if (!response.data.data) {
      throw new Error(
        response.data.message ||
          "Item not found",
      );
    }

    return response.data.data;
  },

  create: async (
    payload: CreateItemPayload,
  ): Promise<ItemDto> => {
    const formData = new FormData();

    formData.append("Code", payload.code);
    formData.append("Name", payload.name);
    formData.append("BaseUnit", payload.baseUnit);
    formData.append(
      "MinimumStock",
      payload.minimumStock.toString(),
    );
    formData.append(
      "ExpiringSoonDays",
      payload.expiringSoonDays.toString(),
    );

    if (payload.image) {
      formData.append("Image", payload.image);
    }

    const response =
      await apiClient.post<ServiceResult<ItemDto>>(
        "/Item",
        formData,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        },
      );

    if (
      !response.data.success ||
      !response.data.data
    ) {
      throw new Error(
        response.data.message ||
          "Failed to create item",
      );
    }

    return response.data.data;
  },

  update: async (
    payload: UpdateItemPayload,
  ): Promise<ItemDto> => {
    const formData = new FormData();

    formData.append("Name", payload.name);
    formData.append("BaseUnit", payload.baseUnit);
    formData.append(
      "MinimumStock",
      payload.minimumStock.toString(),
    );
    formData.append(
      "ExpiringSoonDays",
      payload.expiringSoonDays.toString(),
    );

    if (payload.image) {
      formData.append("Image", payload.image);
    }

    const response =
      await apiClient.put<ServiceResult<ItemDto>>(
        `/Item/${payload.id}`,
        formData,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        },
      );

    if (
      !response.data.success ||
      !response.data.data
    ) {
      throw new Error(
        response.data.message ||
          "Failed to update item",
      );
    }

    return response.data.data;
  },

  delete: async (id: string): Promise<boolean> => {
    const response = await apiClient.delete(`/Item/${id}`);

    return response.data;
  },
};