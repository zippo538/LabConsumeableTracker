import { useMutation, useQueryClient } from "@tanstack/react-query";
import { consumptionApi } from "../api/consumptionApi";
import type {
  ConfirmConsumptionPayload,
  ConsumptionResponseDto,
  FefoAllocationPreviewDto,
  RequestFefoAllocationPayload,
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

export const useConfirmConsumption = () => {
  const queryClient = useQueryClient();

  return useMutation<
    ConsumptionResponseDto,
    Error,
    { jobId: string; payload: ConfirmConsumptionPayload }
  >({
    mutationFn: ({ jobId, payload }) =>
      consumptionApi.confirmConsumption(jobId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["jobs"] });
      queryClient.invalidateQueries({ queryKey: ["items"] });
      queryClient.invalidateQueries({ queryKey: ["lots"] });
    },
  });
};
