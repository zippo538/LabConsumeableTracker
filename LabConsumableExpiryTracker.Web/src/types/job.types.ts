export type JobStatus =
  | "Draft"
  | "InProgress"
  | "Completed"
  | "Cancelled";

export interface JobDto {
  id: string;
  jobNumber: string;
  description: string;
  status: JobStatus;
  startedAt: string | null;
  completedAt: string | null;
}

export interface CreateJobPayload {
  jobNumber: string;
  description: string;
}

export interface UpdateJobPayload {
  jobNumber: string;
  description: string;
}

export interface ServiceResult<T> {
  success: boolean;
  message: string;
  data: T;
  /** Kode error mesin-baca, mis. "INSUFFICIENT_STOCK". F-15. */
  code?: string | null;
}