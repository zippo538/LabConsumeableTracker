import type { JobStatus } from "../types/job.types";

const STATUS_CONFIG: Record<
  JobStatus,
  { label: string; badge: string; dot: string; summary: string }
> = {
  Draft: {
    label: "Draft",
    badge: "bg-amber-50 text-amber-600",
    dot: "bg-amber-500",
    summary: "No consumption yet",
  },
  InProgress: {
    label: "In Progress",
    badge: "bg-emerald-50 text-emerald-600",
    dot: "bg-emerald-500",
    summary: "Ready for consumption",
  },
  Completed: {
    label: "Completed",
    badge: "bg-sky-50 text-sky-600",
    dot: "bg-sky-500",
    summary: "Job completed",
  },
  Cancelled: {
    label: "Cancelled",
    badge: "bg-rose-50 text-rose-500",
    dot: "bg-rose-500",
    summary: "Job cancelled",
  },
};

export const getStatusConfig = (status: JobStatus) =>
  STATUS_CONFIG[status] ?? {
    label: String(status),
    badge: "bg-slate-100 text-slate-500",
    dot: "bg-slate-400",
    summary: "—",
  };

export const formatDate = (value: string | null) => {
  if (!value) {
    return "—";
  }

  return new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
};

export const canEditJob = (status: JobStatus) =>
  status === "Draft" || status === "InProgress";

export const canCancelJob = (status: JobStatus) =>
  status === "Draft" || status === "InProgress";