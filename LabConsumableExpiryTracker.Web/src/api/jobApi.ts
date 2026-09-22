import { apiClient } from "./authApi";
import type {
  CreateJobPayload,
  JobDto,
  ServiceResult,
  UpdateJobPayload,
} from "../types/job.types";

export const getJobs = async (): Promise<JobDto[]> => {
  const response = await apiClient.get<ServiceResult<JobDto[]>>("/jobs");

  return response.data.data;
};

export const getJobById = async (id: string): Promise<JobDto> => {
  const response = await apiClient.get<ServiceResult<JobDto>>(
    `/jobs/${id}`
  );

  return response.data.data;
};

export const createJob = async (
  payload: CreateJobPayload
): Promise<JobDto> => {
  const response = await apiClient.post<ServiceResult<JobDto>>(
    "/jobs",
    payload
  );

  return response.data.data;
};

export const startJob = async (id: string): Promise<JobDto> => {
  const response = await apiClient.post<ServiceResult<JobDto>>(
    `/jobs/${id}/start`
  );

  return response.data.data;
};

export const cancelJob = async (id: string): Promise<JobDto> => {
  const response = await apiClient.post<ServiceResult<JobDto>>(
    `/jobs/${id}/cancel`
  );

  return response.data.data;
};

export const completeJob = async (id: string): Promise<JobDto> => {
  const response = await apiClient.post<ServiceResult<JobDto>>(
    `/jobs/${id}/complete`
  );

  return response.data.data;
};


export const updateJob = async (
  id: string,
  payload: UpdateJobPayload
): Promise<JobDto> => {
  const response = await apiClient.put<ServiceResult<JobDto>>(
    `/jobs/${id}/`,
    payload
  );

  return response.data.data;
};

export const deleteJob = async (id: string): Promise<void> => {
  await apiClient.delete(`/jobs/${id}/`);
};