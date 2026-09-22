import {
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";

import {
  cancelJob,
  completeJob,
  createJob,
  getJobById,
  getJobs,
  startJob,
  updateJob,
} from "../api/jobApi";

import type { CreateJobPayload,UpdateJobPayload } from "../types/job.types";

export const useJobs = () => {
  return useQuery({
    queryKey: ["jobs"],
    queryFn: getJobs,
  });
};

export const useJob = (id: string) => {
  return useQuery({
    queryKey: ["jobs", id],
    queryFn: () => getJobById(id),
    enabled: Boolean(id),
  });
};

export const useCreateJob = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateJobPayload) => createJob(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["jobs"],
      });
    },
  });
};

export const useStartJob = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => startJob(id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["jobs"],
      });
    },
  });
};

export const useCancelJob = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => cancelJob(id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["jobs"],
      });
    },
  });
};

export const useCompleteJob = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => completeJob(id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["jobs"],
      });
    },
  });
};

export const useUpdateJob = () => {
  const queryClient = useQueryClient();
 
  return useMutation({
    mutationFn: ({ id, ...payload }: { id: string } & UpdateJobPayload) =>
      updateJob(id, payload),
    onSuccess: () =>
      queryClient.invalidateQueries({
        queryKey: ["jobs"],
      }),
  });
};