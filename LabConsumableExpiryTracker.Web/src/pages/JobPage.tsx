import { useMemo, useState } from "react";
import {
  AlertCircle,
  CheckCircle2,
  Clock3,
  ChevronDown,
  Info,
  Play,
  Search,
  FlaskConical,
} from "lucide-react";

import { Link, useLocation, useNavigate, useSearchParams } from "react-router";

import { useJobs, useStartJob, useCreateJob } from "../hooks/useJob";

import type { JobDto, JobStatus } from "../types/job.types";
import { AppLayout } from "../components/layout/AppLayout";
import {
  FormModal,
  ModalField,
} from "../components/Modal";
import { getErrorMessage } from "../helpers/getErrorMessage";
import { formatDate, getStatusConfig } from "../helpers/jobStatus";
import type { FormEvent } from "react";

type StatusTab = "All" | JobStatus;

const TABS: { key: StatusTab; label: string }[] = [
  { key: "InProgress", label: "In Progress" },
  { key: "Draft", label: "Draft" },
  { key: "Completed", label: "Completed" },
  { key: "Cancelled", label: "Cancelled" },
  { key: "All", label: "All" },
];

const DEFAULT_TAB: StatusTab = "InProgress";

const JOB_LIST_PATH = "/jobs";

const STATUS_PRIORITY: Record<JobStatus, number> = {
  InProgress: 0,
  Draft: 1,
  Completed: 2,
  Cancelled: 3,
};

const toTime = (value: string | null) =>
  value ? new Date(value).getTime() : 0;

const getNextJobNumber = (jobs: JobDto[]) => {
  const prefix = `JOB-${new Date().getFullYear()}-`;

  const max = jobs.reduce((acc, job) => {
    if (!job.jobNumber.startsWith(prefix)) {
      return acc;
    }

    const sequence = parseInt(job.jobNumber.slice(prefix.length), 10);

    return Number.isNaN(sequence) ? acc : Math.max(acc, sequence);
  }, 0);

  return `${prefix}${String(max + 1).padStart(3, "0")}`;
};


const parseTab = (value: string | null): StatusTab =>
  TABS.some((tab) => tab.key === value) ? (value as StatusTab) : DEFAULT_TAB;

type EmptyAction = "create" | "drafts" | null;

const getEmptyState = (
  tab: StatusTab,
  hasSearch: boolean,
  draftCount: number
): { title: string; hint: string; action: EmptyAction } => {
  if (hasSearch) {
    return {
      title: "No jobs found",
      hint: "Try a different job number or keyword.",
      action: null,
    };
  }

  switch (tab) {
    case "InProgress":
      return {
        title: "No jobs in progress",
        hint: "Start a draft job to begin consuming stock.",
        action: draftCount > 0 ? "drafts" : "create",
      };

    case "Draft":
      return {
        title: "No drafts yet",
        hint: "Create a job to get started.",
        action: "create",
      };

    case "Completed":
      return {
        title: "No completed jobs yet",
        hint: "Finished jobs will show up here.",
        action: null,
      };

    case "Cancelled":
      return {
        title: "No cancelled jobs",
        hint: "Cancelled jobs will show up here.",
        action: null,
      };

    default:
      return {
        title: "No jobs yet",
        hint: "Create your first job to get started.",
        action: "create",
      };
  }
};

export const JobPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams, setSearchParams] = useSearchParams();

  const { data: jobs = [], isLoading, isError } = useJobs();
 const nextJobNumber = useMemo(() => getNextJobNumber(jobs), [jobs]);

  const startJobMutation = useStartJob();
  const createJobMutation = useCreateJob();

  const [search, setSearch] = useState("");
  const [sortLatest, setSortLatest] = useState(true);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [createSuccess, setCreateSuccess] = useState<string | null>(null);
  
  const activeTab = parseTab(searchParams.get("status"));

const inputClass =
  "w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-normal text-slate-900 placeholder:text-slate-400 focus:border-teal-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-teal-500 disabled:opacity-50";

  const searchedJobs = useMemo(() => {
    const keyword = search.trim().toLowerCase();

    if (!keyword) {
      return jobs;
    }

    return jobs.filter(
      (job) =>
        job.jobNumber.toLowerCase().includes(keyword) ||
        (job.description ?? "").toLowerCase().includes(keyword)
    );
  }, [jobs, search]);

  const counts = useMemo(() => {
    const result: Record<StatusTab, number> = {
      All: searchedJobs.length,
      InProgress: 0,
      Draft: 0,
      Completed: 0,
      Cancelled: 0,
    };

    searchedJobs.forEach((job) => {
      if (job.status in result) {
        result[job.status] += 1;
      }
    });

    return result;
  }, [searchedJobs]);

  const visibleJobs = useMemo(() => {
    const list =
      activeTab === "All"
        ? searchedJobs
        : searchedJobs.filter((job) => job.status === activeTab);

    return [...list].sort((a, b) => {
      if (activeTab === "All") {
        const priority =
          (STATUS_PRIORITY[a.status] ?? 99) -
          (STATUS_PRIORITY[b.status] ?? 99);

        if (priority !== 0) {
          return priority;
        }
      }

      const dateA = toTime(a.startedAt);
      const dateB = toTime(b.startedAt);

      const byDate =
        dateA !== dateB
          ? dateB - dateA
          : b.jobNumber.localeCompare(a.jobNumber, undefined, {
              numeric: true,
            });

      return sortLatest ? byDate : -byDate;
    });
  }, [searchedJobs, activeTab, sortLatest]);

  const handleTabChange = (tab: StatusTab) => {
    setSearchParams({ status: tab }, { replace: true });
  };

  const openDetail = (job: JobDto) => {
    navigate(`${JOB_LIST_PATH}/${job.id}`, {
      state: { from: location.search },
    });
  };

  const handleStartJob = (job: JobDto) => {
    startJobMutation.mutate(job.id);
  };

  const handleOpenCreate = () => {
    createJobMutation.reset();
    setCreateSuccess(null);
    setIsCreateOpen(true);
  };

  const handleCreateJob = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const data = new FormData(event.currentTarget);

    createJobMutation.mutate(
        {
          jobNumber: String(data.get("jobNumber") ?? "").trim(),
          description: String(data.get("description") ?? "").trim(),
        },
        {
          onSuccess: () => {
            setCreateSuccess("Job created successfully.");
            setIsCreateOpen(false);
            setSearch("");
            handleTabChange("Draft");
          },
        }
    );
  };
  const emptyState = getEmptyState(
    activeTab,
    search.trim().length > 0,
    counts.Draft
  );

  const emptyContent = (
    <div className="flex flex-col items-center">
      <Clock3 className="mb-3 h-6 w-6 text-slate-300" />

      <p className="text-[12px] font-medium text-slate-500">
        {emptyState.title}
      </p>

      <p className="mt-1 text-[11px] text-slate-400">{emptyState.hint}</p>

      {emptyState.action && (
        <button
          type="button"
          onClick={
            emptyState.action === "create"
              ? handleOpenCreate
              : () => handleTabChange("Draft")
          }
          className="mt-4 h-9 rounded-lg border border-slate-200 bg-white px-4 text-[11px] font-semibold text-slate-700 transition-colors hover:border-[#18b4b5] hover:text-[#0da7ad]"
        >
          {emptyState.action === "create" ? "+ Create Job" : "View drafts"}
        </button>
      )}
    </div>
  );

  return (
    <AppLayout
      title="Job List"
      description="Manage laboratory jobs and stock consumption"
    >
      <div className="min-h-full bg-[#f7fafc] px-6 py-7 lg:px-10">
        <div className="mx-auto max-w-[1200px]">
          {/* Header */}
          <div className="mb-5">
            <h1 className="text-[26px] font-bold tracking-tight text-slate-900">
              Job List
            </h1>

            <p className="mt-1 text-[13px] text-slate-500">
              Manage laboratory activities before consuming stock.
            </p>
          </div>

          <div className="mb-5 rounded-xl border border-[#b9e8eb] bg-[#eafafd] px-5 py-3.5">
            <div className="flex items-start gap-3">
              <div className="mt-0.5">
                <FlaskConical className="h-4 w-4 text-[#11a9ae]" />
              </div>

              <div>
                <p className="text-[12px] font-semibold text-[#0da3a8]">
                  Scientist workflow
                </p>

                <p className="mt-1.5 text-[12px] font-medium text-[#14546a]">
                  Draft → Start → Consume (FEFO) → Complete
                </p>

                <p className="mt-1 text-[10px] text-[#367286]">
                  Consume is only available for Jobs In Progress.
                </p>
              </div>
            </div>
          </div>

          <div className="mb-5 flex flex-col gap-3 rounded-xl border border-slate-200 bg-white p-2 shadow-sm md:flex-row">
            <div className="relative flex-1">
              <Search className="absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />

              <input
                type="text"
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Search for the job number..."
                className="h-10 w-full rounded-lg border-0 bg-transparent pl-11 pr-4 text-[12px] text-slate-700 outline-none placeholder:text-slate-400 focus:ring-0"
              />
            </div>

            <div className="relative">
              <select
                value={sortLatest ? "latest" : "oldest"}
                onChange={(event) =>
                  setSortLatest(event.target.value === "latest")
                }
                className="h-10 min-w-[160px] appearance-none rounded-lg border border-slate-200 bg-white px-4 pr-9 text-[12px] font-medium text-slate-600 outline-none focus:border-[#18b4b5]"
              >
                <option value="latest">Latest</option>
                <option value="oldest">Oldest</option>
              </select>

              <ChevronDown className="pointer-events-none absolute right-3 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-slate-400" />
            </div>

            <button
              type="button"
              onClick={handleOpenCreate}
              className="h-10 rounded-lg bg-[#12a9ad] px-6 text-[12px] font-semibold text-white transition-colors hover:bg-[#0e969a]"
            >
              + Create Job
            </button>
          </div>

          {createSuccess && (
              <div className="mb-5 flex items-center gap-3 rounded-xl border border-green-200 bg-green-50 px-4 py-3 text-[12px] text-green-700">
                <CheckCircle2 className="h-4 w-4 shrink-0" />
                {createSuccess}
              </div>
          )}

          {isError && (
              <div className="mb-5 flex items-center gap-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-[12px] text-rose-600">
                <AlertCircle className="h-4 w-4 shrink-0" />
                Failed to load jobs. Please try again.
              </div>
          )}

          <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
            
            <div
              role="tablist"
              aria-label="Filter jobs by status"
              className="flex overflow-x-auto border-b border-slate-200 px-3"
            >
              {TABS.map((tab) => {
                const isActive = activeTab === tab.key;

                return (
                  <button
                    key={tab.key}
                    type="button"
                    role="tab"
                    aria-selected={isActive}
                    onClick={() => handleTabChange(tab.key)}
                    className={`relative flex shrink-0 items-center gap-2 whitespace-nowrap px-4 py-3.5 text-[12px] font-semibold transition-colors ${
                      isActive
                        ? "text-[#0da7ad]"
                        : "text-slate-500 hover:text-slate-700"
                    }`}
                  >
                    {tab.label}

                    <span
                      className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${
                        isActive
                          ? "bg-[#12a9ad]/10 text-[#0da7ad]"
                          : "bg-slate-100 text-slate-500"
                      }`}
                    >
                      {counts[tab.key]}
                    </span>

                    {isActive && (
                      <span className="absolute inset-x-3 bottom-0 h-0.5 rounded-full bg-[#12a9ad]" />
                    )}
                  </button>
                );
              })}
            </div>

            <div className="hidden overflow-x-auto md:block">
              <table className="w-full min-w-[850px] border-collapse">
                <thead>
                  <tr className="border-b border-slate-200">
                    <th className="px-6 py-4 text-left text-[10px] font-semibold uppercase tracking-wide text-slate-500">
                      Job
                    </th>

                    <th className="px-4 py-4 text-left text-[10px] font-semibold uppercase tracking-wide text-slate-500">
                      Status
                    </th>

                    <th className="px-4 py-4 text-left text-[10px] font-semibold uppercase tracking-wide text-slate-500">
                      Start
                    </th>

                    <th className="px-4 py-4 text-left text-[10px] font-semibold uppercase tracking-wide text-slate-500">
                      Finish
                    </th>

                    <th className="px-4 py-4 text-left text-[10px] font-semibold uppercase tracking-wide text-slate-500">
                      Summary
                    </th>

                    <th className="px-6 py-4 text-right text-[10px] font-semibold uppercase tracking-wide text-slate-500">
                      Action
                    </th>
                  </tr>
                </thead>

                <tbody>
                  {isLoading ? (
                    <tr>
                      <td
                        colSpan={6}
                        className="px-6 py-14 text-center text-[12px] text-slate-400"
                      >
                        Loading jobs...
                      </td>
                    </tr>
                  ) : visibleJobs.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="px-6 py-14 text-center">
                        {emptyContent}
                      </td>
                    </tr>
                  ) : (
                    visibleJobs.map((job) => {
                      const cfg = getStatusConfig(job.status);

                      const isDimmed =
                        activeTab === "All" &&
                        (job.status === "Completed" ||
                          job.status === "Cancelled");

                      return (
                        <tr
                          key={job.id}
                          onClick={() => openDetail(job)}
                          className={`cursor-pointer border-b border-slate-100 last:border-b-0 hover:bg-slate-50/70 ${
                            isDimmed ? "opacity-70 hover:opacity-100" : ""
                          }`}
                        >
                  
                          <td className="px-6 py-5">
                            <div>
                              <Link
                                to={`${JOB_LIST_PATH}/${job.id}`}
                                state={{ from: location.search }}
                                onClick={(event) => event.stopPropagation()}
                                className="text-[13px] font-semibold text-[#0da7ad] hover:underline"
                              >
                                {job.jobNumber}
                              </Link>

                              <p className="mt-1 max-w-[220px] truncate text-[11px] text-slate-500">
                                {job.description || "No description"}
                              </p>
                            </div>
                          </td>

                        
                          <td className="px-4 py-5">
                            <span
                              className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1.5 text-[10px] font-semibold ${cfg.badge}`}
                            >
                              <span
                                className={`h-1.5 w-1.5 rounded-full ${cfg.dot}`}
                              />

                              {cfg.label}
                            </span>
                          </td>

                    
                          <td className="whitespace-nowrap px-4 py-5 text-[11px] text-slate-700">
                            {formatDate(job.startedAt)}
                          </td>

                      
                          <td className="whitespace-nowrap px-4 py-5 text-[11px] text-slate-700">
                            {formatDate(job.completedAt)}
                          </td>

                          <td className="px-4 py-5 text-[11px] text-slate-700">
                            {cfg.summary}
                          </td>

                          <td className="px-6 py-5 text-right">
                            {job.status === "Draft" && (
                              <button
                                type="button"
                                onClick={(event) => {
                                  event.stopPropagation();
                                  handleStartJob(job);
                                }}
                                disabled={startJobMutation.isPending}
                                className="inline-flex h-10 items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 text-[11px] font-semibold text-slate-700 transition-colors hover:border-[#18b4b5] hover:text-[#0da7ad] disabled:cursor-not-allowed disabled:opacity-50"
                              >
                                <Play className="h-3.5 w-3.5" />

                                {startJobMutation.isPending
                                  ? "Starting..."
                                  : "Start Job"}
                              </button>
                            )}

                            {job.status === "InProgress" && (
                              <button
                                type="button"
                                onClick={(event) => event.stopPropagation()}
                                className="inline-flex h-10 items-center gap-2 rounded-lg bg-[#12a9ad] px-5 text-[11px] font-semibold text-white transition-colors hover:bg-[#0e969a]"
                              >
                                <FlaskConical className="h-3.5 w-3.5" />
                                Consume
                              </button>
                            )}

                            {job.status === "Completed" && (
                              <button
                                type="button"
                                className="inline-flex h-10 items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 text-[11px] font-semibold text-slate-700 transition-colors hover:border-[#18b4b5] hover:text-[#0da7ad]"
                              >
                                <CheckCircle2 className="h-3.5 w-3.5" />
                                View details
                              </button>
                            )}

                            {job.status === "Cancelled" && (
                              <span className="text-[11px] text-slate-400">
                                No action
                              </span>
                            )}
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>

           
            <div className="divide-y divide-slate-100 md:hidden">
              {isLoading ? (
                <div className="px-5 py-12 text-center text-[12px] text-slate-400">
                  Loading jobs...
                </div>
              ) : visibleJobs.length === 0 ? (
                <div className="px-5 py-12 text-center">{emptyContent}</div>
              ) : (
                visibleJobs.map((job) => {
                  const cfg = getStatusConfig(job.status);

                  const isDimmed =
                    activeTab === "All" &&
                    (job.status === "Completed" || job.status === "Cancelled");

                  return (
                    <div
                      key={job.id}
                      onClick={() => openDetail(job)}
                      className={`cursor-pointer p-5 ${isDimmed ? "opacity-70" : ""}`}
                    >
                      <div className="flex items-start justify-between gap-4">
                        <div>
                          <p className="text-[13px] font-semibold text-[#0da7ad]">
                            {job.jobNumber}
                          </p>

                          <p className="mt-1 text-[11px] text-slate-500">
                            {job.description || "No description"}
                          </p>
                        </div>

                        <span
                          className={`shrink-0 rounded-full px-2.5 py-1 text-[9px] font-semibold ${cfg.badge}`}
                        >
                          {cfg.label}
                        </span>
                      </div>

                      <div className="mt-4 grid grid-cols-2 gap-3">
                        <div>
                          <p className="text-[9px] uppercase text-slate-400">
                            Start
                          </p>

                          <p className="mt-1 text-[11px] text-slate-700">
                            {formatDate(job.startedAt)}
                          </p>
                        </div>

                        <div>
                          <p className="text-[9px] uppercase text-slate-400">
                            Finish
                          </p>

                          <p className="mt-1 text-[11px] text-slate-700">
                            {formatDate(job.completedAt)}
                          </p>
                        </div>
                      </div>

                      <div className="mt-4">
                        {job.status === "Draft" && (
                          <button
                            type="button"
                            onClick={(event) => {
                                  event.stopPropagation();
                                  handleStartJob(job);
                                }}
                            disabled={startJobMutation.isPending}
                            className="w-full rounded-lg border border-slate-200 py-2.5 text-[11px] font-semibold text-slate-700 disabled:opacity-50"
                          >
                            {startJobMutation.isPending
                              ? "Starting..."
                              : "Start Job"}
                          </button>
                        )}

                        {job.status === "InProgress" && (
                          <button
                            type="button"
                            onClick={(event) => event.stopPropagation()}
                            className="w-full rounded-lg bg-[#12a9ad] py-2.5 text-[11px] font-semibold text-white"
                          >
                            Consume
                          </button>
                        )}

                        {job.status === "Completed" && (
                          <button
                            type="button"
                            className="w-full rounded-lg border border-slate-200 py-2.5 text-[11px] font-semibold text-slate-700"
                          >
                            View details
                          </button>
                        )}
                      </div>
                    </div>
                  );
                })
              )}
            </div>
          </div>

          {/* Bottom info */}
          <div className="mt-6 flex items-start gap-3 rounded-xl bg-[#fff4d9] px-4 py-3.5 text-[#875400]">
            <Info className="mt-0.5 h-4 w-4 shrink-0" />

            <p className="text-[11px] leading-5">
              Lots are automatically selected using FEFO during the preview;
              the preview is not a reservation and requires re-confirmation.
            </p>
          </div>
        </div>
      </div>

  <FormModal
  isOpen={isCreateOpen}
  onClose={() => setIsCreateOpen(false)}
  mode="create"
  title="Create job"
  description="New jobs start as Draft. Start a job when you're ready to consume stock."
  submitLabel="Create job"
  busy={createJobMutation.isPending}
  error={
    createJobMutation.isError
      ? getErrorMessage(createJobMutation.error, "Failed to create job.")
      : null
  }
  onSubmit={handleCreateJob}
>
  <ModalField
    label="Job number"
    required
    hint="Suggested automatically. You can change it."
  >
    <input
      name="jobNumber"
      required
      defaultValue={nextJobNumber}
      className={`${inputClass} h-11`}
    />
  </ModalField>

  <ModalField label="Description">
    <textarea
      name="description"
      rows={4}
      className={`${inputClass} resize-y py-2.5`}
    />
  </ModalField>
</FormModal>
    </AppLayout>
  );
};