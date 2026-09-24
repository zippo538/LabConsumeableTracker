import { useMemo, useState } from "react";
import {
    AlertCircle,
    CheckCircle2,
    Info,
} from "lucide-react";
import {
    useLocation,
    useNavigate,
    useSearchParams,
} from "react-router";
import type { FormEvent } from "react";

import {
    useCreateJob,
    useJobs,
    useStartJob,
} from "../hooks/useJob";

import type {
    JobDto,
    JobStatus,
} from "../types/job.types";

import { AppLayout } from "../components/layout/AppLayout";

import { getErrorMessage } from "../helpers/getErrorMessage";

import { JobWorkflowInfo } from "../components/job/JobWorkfowInfo";;

import {
    JobStatusTabs,
    type StatusTab,
} from "../components/job/JobStatusTabs";

import {
    JobEmptyState,
    type EmptyAction,
} from "../components/job/JobEmptyState";

import { JobTable } from "../components/job/JobTable";
import { JobMobileList } from "../components/job/JobMobileList";
import { CreateJobModal } from "../components/job/CreateJobModal";
import { StartJobModal } from "../components/job/StartJobModal";
import { JobFilters } from "../components/job/JobFilters";

const TABS: {
    key: StatusTab;
    label: string;
}[] = [
        {
            key: "InProgress",
            label: "In Progress",
        },
        {
            key: "Draft",
            label: "Draft",
        },
        {
            key: "Completed",
            label: "Completed",
        },
        {
            key: "Cancelled",
            label: "Cancelled",
        },
        {
            key: "All",
            label: "All",
        },
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

        const sequence = parseInt(
            job.jobNumber.slice(prefix.length),
            10
        );

        return Number.isNaN(sequence)
            ? acc
            : Math.max(acc, sequence);
    }, 0);

    return `${prefix}${String(max + 1).padStart(3, "0")}`;
};

const parseTab = (
    value: string | null
): StatusTab => {
    return TABS.some(
        (tab) => tab.key === value
    )
        ? (value as StatusTab)
        : DEFAULT_TAB;
};

const getEmptyState = (
    tab: StatusTab,
    hasSearch: boolean,
    draftCount: number
): {
    title: string;
    hint: string;
    action: EmptyAction;
} => {
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
                action:
                    draftCount > 0
                        ? "drafts"
                        : "create",
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

    const [
        searchParams,
        setSearchParams,
    ] = useSearchParams();

    const {
        data: jobs = [],
        isLoading,
        isError,
    } = useJobs();

    const startJobMutation = useStartJob();
    const createJobMutation = useCreateJob();

    const [search, setSearch] = useState("");
    const [sortLatest, setSortLatest] =
        useState(true);

    const [
        isCreateOpen,
        setIsCreateOpen,
    ] = useState(false);

    const [
        createSuccess,
        setCreateSuccess,
    ] = useState<string | null>(null);

    const activeTab = parseTab(
        searchParams.get("status")
    );

    const nextJobNumber = useMemo(
        () => getNextJobNumber(jobs),
        [jobs]
    );

    const searchedJobs = useMemo(() => {
        const keyword = search
            .trim()
            .toLowerCase();

        if (!keyword) {
            return jobs;
        }

        return jobs.filter(
            (job) =>
                job.jobNumber
                    .toLowerCase()
                    .includes(keyword) ||
                (job.description ?? "")
                    .toLowerCase()
                    .includes(keyword)
        );
    }, [jobs, search]);

    const [selectedJob, setSelectedJob] = useState<JobDto | null>(null);
    const [isStartOpen, setIsStartOpen] = useState(false);

    const counts = useMemo(() => {
        const result: Record<
            StatusTab,
            number
        > = {
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
                : searchedJobs.filter(
                    (job) =>
                        job.status ===
                        activeTab
                );

        return [...list].sort((a, b) => {
            if (activeTab === "All") {
                const priority =
                    STATUS_PRIORITY[
                    a.status
                    ] -
                    STATUS_PRIORITY[
                    b.status
                    ];

                if (priority !== 0) {
                    return priority;
                }
            }

            const dateA = toTime(
                a.startedAt
            );

            const dateB = toTime(
                b.startedAt
            );

            const byDate =
                dateA !== dateB
                    ? dateB - dateA
                    : b.jobNumber.localeCompare(
                        a.jobNumber,
                        undefined,
                        {
                            numeric: true,
                        }
                    );

            return sortLatest
                ? byDate
                : -byDate;
        });
    }, [
        searchedJobs,
        activeTab,
        sortLatest,
    ]);

    const handleTabChange = (
        tab: StatusTab
    ) => {
        setSearchParams(
            {
                status: tab,
            },
            {
                replace: true,
            }
        );
    };

    const openDetail = (job: JobDto) => {
        navigate(
            `${JOB_LIST_PATH}/${job.id}`,
            {
                state: {
                    from: location.search,
                },
            }
        );
    };

    const handleStartJob = (job: JobDto) => {
        startJobMutation.reset();
        setSelectedJob(job);
        setIsStartOpen(true);
    };

    const handleConfirmStartJob = () => {
        if (!selectedJob) return;

        startJobMutation.mutate(selectedJob.id, {
            onSuccess: () => {
                setIsStartOpen(false);
                setSelectedJob(null);
            },
        });
    };

    const handleConsume = (
        job: JobDto
    ) => {
        navigate(
            `/app/consumption?jobId=${job.id}`
        );
    };

    const handleOpenCreate = () => {
        createJobMutation.reset();

        setCreateSuccess(null);

        setIsCreateOpen(true);
    };

    const handleCreateJob = (
        event: FormEvent<HTMLFormElement>
    ) => {
        event.preventDefault();

        const data = new FormData(
            event.currentTarget
        );

        createJobMutation.mutate(
            {
                jobNumber: String(
                    data.get("jobNumber") ??
                    ""
                ).trim(),

                description: String(
                    data.get("description") ??
                    ""
                ).trim(),
            },
            {
                onSuccess: () => {
                    setCreateSuccess(
                        "Job created successfully."
                    );

                    setIsCreateOpen(false);

                    setSearch("");

                    handleTabChange(
                        "Draft"
                    );
                },
            }
        );
    };

    const emptyState =
        getEmptyState(
            activeTab,
            search.trim().length > 0,
            counts.Draft
        );

    const emptyContent = (
        <JobEmptyState
            title={emptyState.title}
            hint={emptyState.hint}
            action={emptyState.action}
            onCreate={handleOpenCreate}
            onViewDrafts={() =>
                handleTabChange("Draft")
            }
        />
    );

    return (
        <AppLayout
            title="Job List"
            description="Manage laboratory jobs and stock consumption"
        >
            <div className="min-h-full space-y-7 bg-[#f7fafc]">
                <div className="mx-auto max-w-[1200px]">
                    {/* Header */}
                    <div className="mb-5">
                        <h1 className="text-[26px] font-bold tracking-tight text-slate-900">
                            Job List
                        </h1>

                        <p className="mt-1 text-[13px] text-slate-500">
                            Manage laboratory activities before
                            consuming stock.
                        </p>
                    </div>

                    {/* Workflow */}
                    <JobWorkflowInfo />

                    {/* Search / Sort / Create */}
                    <JobFilters
                        search={search}
                        sortLatest={sortLatest}
                        onSearchChange={
                            setSearch
                        }
                        onSortChange={
                            setSortLatest
                        }
                        onCreate={
                            handleOpenCreate
                        }
                    />

                    {/* Success */}
                    {createSuccess && (
                        <div className="mb-5 flex items-center gap-3 rounded-xl border border-green-200 bg-green-50 px-4 py-3 text-[12px] text-green-700">
                            <CheckCircle2 className="h-4 w-4 shrink-0" />

                            {createSuccess}
                        </div>
                    )}

                    {/* Load Error */}
                    {isError && (
                        <div className="mb-5 flex items-center gap-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-[12px] text-rose-600">
                            <AlertCircle className="h-4 w-4 shrink-0" />

                            Failed to load jobs.
                            Please try again.
                        </div>
                    )}

                    {/* Job List */}
                    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
                        {/* Status Tabs */}
                        <JobStatusTabs
                            tabs={TABS}
                            activeTab={activeTab}
                            counts={counts}
                            onChange={
                                handleTabChange
                            }
                        />

                        {isLoading ? (
                            <>
                                {/* Desktop Loading */}
                                <div className="hidden px-6 py-14 text-center text-[12px] text-slate-400 md:block">
                                    Loading jobs...
                                </div>

                                {/* Mobile Loading */}
                                <div className="px-5 py-12 text-center text-[12px] text-slate-400 md:hidden">
                                    Loading jobs...
                                </div>
                            </>
                        ) : (
                            <>
                                {/* Desktop */}
                                <JobTable
                                    jobs={
                                        visibleJobs
                                    }
                                    activeTab={
                                        activeTab
                                    }
                                    isStarting={
                                        startJobMutation.isPending
                                    }
                                    onOpenDetail={
                                        openDetail
                                    }
                                    onStartJob={
                                        handleStartJob
                                    }
                                    onConsume={
                                        handleConsume
                                    }
                                    emptyContent={
                                        emptyContent
                                    }
                                />

                                {/* Mobile */}
                                <JobMobileList
                                    jobs={
                                        visibleJobs
                                    }
                                    activeTab={
                                        activeTab
                                    }
                                    isStarting={
                                        startJobMutation.isPending
                                    }
                                    onOpenDetail={
                                        openDetail
                                    }
                                    onStartJob={
                                        handleStartJob
                                    }
                                    onConsume={
                                        handleConsume
                                    }
                                    emptyContent={
                                        emptyContent
                                    }
                                />
                            </>
                        )}
                    </div>

                    {/* Bottom Information */}
                    <div className="mt-6 flex items-start gap-3 rounded-xl bg-[#fff4d9] px-4 py-3.5 text-[#875400]">
                        <Info className="mt-0.5 h-4 w-4 shrink-0" />

                        <p className="text-[11px] leading-5">
                            Lots are automatically
                            selected using FEFO during
                            the preview; the preview is
                            not a reservation and
                            requires re-confirmation.
                        </p>
                    </div>
                </div>
            </div>

            {/* Create Job Modal */}
            <CreateJobModal
                isOpen={isCreateOpen}
                nextJobNumber={
                    nextJobNumber
                }
                busy={
                    createJobMutation.isPending
                }
                error={
                    createJobMutation.isError
                        ? getErrorMessage(
                            createJobMutation.error,
                            "Failed to create job."
                        )
                        : null
                }
                onClose={() =>
                    setIsCreateOpen(false)
                }
                onSubmit={
                    handleCreateJob
                }
            />
            <StartJobModal
                job={selectedJob}
                isOpen={isStartOpen}
                busy={startJobMutation.isPending}
                onClose={() => {
                    if (!startJobMutation.isPending) {
                        setIsStartOpen(false);
                        setSelectedJob(null);
                    }
                }}
                onConfirm={handleConfirmStartJob}
            />
        </AppLayout>
    );
};