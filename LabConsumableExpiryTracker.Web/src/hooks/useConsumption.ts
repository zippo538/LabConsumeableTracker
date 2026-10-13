import { useMutation, useQueryClient } from "@tanstack/react-query";
import { consumptionApi } from "../api/consumptionApi";
import type {
  CreateDraftConsumptionPayload,
  DraftConsumptionResponseDto,
  FefoAllocationPreviewDto,
  RequestFefoAllocationPayload,
  VerifyScanPayload,
  VerifyScanResponseDto,
} from "../types/consumption.types";

export const useFefoPreview = () => {
  return useMutation<
    FefoAllocationPreviewDto,
    Error,
    { jobId: string; payload: RequestFefoAllocationPayload }
  >({
    mutationFn: ({ jobId, payload }) =>
      consumptionApi.previewAllocation(jobId, payload),
  });
};

/** F-15 Fase 1. Draft tidak mengubah stok, jadi cache lot tak perlu di-invalidate. */
export const useCreateDraftConsumption = () =>
  useMutation<
    DraftConsumptionResponseDto,
    Error,
    { jobId: string; payload: CreateDraftConsumptionPayload }
  >({
    mutationFn: ({ jobId, payload }) =>
      consumptionApi.createDraft(jobId, payload),
  });

/** F-15 Fase 2. Di sini stok benar-benar berkurang. */
export const useVerifyScan = () => {
  const queryClient = useQueryClient();

  return useMutation<
    VerifyScanResponseDto,
    Error,
    { jobId: string; payload: VerifyScanPayload }
  >({
    mutationFn: ({ jobId, payload }) =>
      consumptionApi.verifyScan(jobId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["jobs"] });
      queryClient.invalidateQueries({ queryKey: ["items"] });
      queryClient.invalidateQueries({ queryKey: ["lots"] });
    },
  });
};
