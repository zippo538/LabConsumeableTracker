import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { lotApi } from "../api/lotApi";
import type {
  CreateLotRequest,
  UpdateLotRequest,
  LotAdministrativeStatus,
  ExpiryCondition,
} from "../types/lot.types";

export const lotQueryKeys = {
  all: ["lots"] as const,
  lists: () => [...lotQueryKeys.all, "list"] as const,
  list: (filters: Record<string, unknown>) =>
    [...lotQueryKeys.lists(), filters] as const,
  details: () => [...lotQueryKeys.all, "detail"] as const,
  detail: (id: string) => [...lotQueryKeys.details(), id] as const,
  summary: () => [...lotQueryKeys.all, "summary"] as const,
  items: () => [...lotQueryKeys.all, "items"] as const,
};

export const useLots = (params?: {
  page?: number;
  pageSize?: number;
  search?: string;
  itemId?: string;
  status?: LotAdministrativeStatus | string;
  expiryCondition?: ExpiryCondition | string;
}) => {
  return useQuery({
    queryKey: lotQueryKeys.list(params || {}),
    queryFn: () => lotApi.getLots(params),
    staleTime: 30 * 1000, 
  });
};

export const useLotSummary = () => {
  return useQuery({
    queryKey: lotQueryKeys.summary(),
    queryFn: lotApi.getLotSummary,
    staleTime: 60 * 1000, 
  });
};

export const useItems = () => {
  return useQuery({
    queryKey: lotQueryKeys.items(),
    queryFn: lotApi.getItems,
    staleTime: 5 * 60 * 1000, 
  });
};

export const useLot = (id: string | null) => {
  return useQuery({
    queryKey: lotQueryKeys.detail(id || ""),
    queryFn: () => {
      if (!id) throw new Error("Lot ID is required");
      return lotApi.getLotById(id);
    },
    enabled: !!id,
  });
};

export const useCreateLot = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateLotRequest) => lotApi.createLot(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.lists() });
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.summary() });
    },
  });
};

export const useUpdateLot = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateLotRequest }) =>
      lotApi.updateLot(id, payload),
    onSuccess: (data) => {
      queryClient.setQueryData(lotQueryKeys.detail(data.id), data);
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.lists() });
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.summary() });
    },
  });
};

export const useDeleteLot = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => lotApi.deleteLot(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.lists() });
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.summary() });
    },
  });
};

// Hook: Block lot
export const useBlockLot = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      lotApi.blockLot(id, reason),
    onSuccess: (data) => {
      queryClient.setQueryData(lotQueryKeys.detail(data.id), data);
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.lists() });
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.summary() });
    },
  });
};

// Hook: Quarantine lot
export const useQuarantineLot = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      lotApi.quarantineLot(id, reason),
    onSuccess: (data) => {
      queryClient.setQueryData(lotQueryKeys.detail(data.id), data);
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.lists() });
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.summary() });
    },
  });
};

// Hook: Release lot
export const useReleaseLot = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => lotApi.releaseLot(id),
    onSuccess: (data) => {
      queryClient.setQueryData(lotQueryKeys.detail(data.id), data);
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.lists() });
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.summary() });
    },
  });
};
