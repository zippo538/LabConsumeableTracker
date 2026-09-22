import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { disposalApi } from "../api/dispoalApi";
import type {
  CreateDisposalRequest,
  DisposalListParams,
} from "../types/disposal.types";
import { lotQueryKeys } from "./useLot";

export const disposalQueryKeys = {
  all: ["disposals"] as const,
  lists: () => [...disposalQueryKeys.all, "list"] as const,
  list: (filters: DisposalListParams) =>
    [...disposalQueryKeys.lists(), filters] as const,
  details: () => [...disposalQueryKeys.all, "detail"] as const,
  detail: (id: string) => [...disposalQueryKeys.details(), id] as const,
};

export const useDisposals = (params?: DisposalListParams) => {
  return useQuery({
    queryKey: disposalQueryKeys.list(params || {}),
    queryFn: () => disposalApi.getDisposals(params),
    staleTime: 30 * 1000,
  });
};

export const useDisposal = (id: string | null) => {
  return useQuery({
    queryKey: disposalQueryKeys.detail(id || ""),
    queryFn: () => {
      if (!id) throw new Error("Disposal ID is required");
      return disposalApi.getDisposalById(id);
    },
    enabled: !!id,
  });
};

export const useCreateDisposal = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      lotId,
      payload,
    }: {
      lotId: string;
      payload: CreateDisposalRequest;
    }) => disposalApi.createDisposal(lotId, payload),

    onSuccess: (data, variables) => {
      if (data.lot) {
        queryClient.setQueryData(
          lotQueryKeys.detail(variables.lotId),
          (old: any) => {
            if (!old) return old;
            return { ...old, ...data.lot };
          },
        );
      }

      queryClient.invalidateQueries({ queryKey: lotQueryKeys.lists() });
      queryClient.invalidateQueries({ queryKey: lotQueryKeys.summary() });

      queryClient.invalidateQueries({ queryKey: disposalQueryKeys.lists() });
    },
  });
};
