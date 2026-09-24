import { type FormEvent, useState } from "react";
import { useNavigate, useParams } from "react-router";
import {
    ArrowLeft,
    CheckCircle2,
} from "lucide-react";

import {
    useCancelJob,
    useCompleteJob,
    useJob,
    useStartJob,
    useUpdateJob,
} from "../hooks/useJob";

import { AppLayout } from "../components/layout/AppLayout";

import { getErrorMessage } from "../helpers/getErrorMessage";

import { JobDetailHeader } from "../components/job/JobDetailHeader";
import { JobInformation } from "../components/job/JobInformation";
import { EditJobModal } from "../components/job/EditJobModal";
import { StartJobModal } from "../components/job/StartJobModal";
import { CompleteJobModal } from "../components/job/CompleteJobModal";
import { CancelJobModal } from "../components/job/CancelJobModal";

export const JobDetailPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const {
        data: job,
        isLoading,
        isError,
    } = useJob(id ?? "");

    const updateJobMutation = useUpdateJob();
    const cancelJobMutation = useCancelJob();
    const startJobMutation = useStartJob();
    const completeJobMutation = useCompleteJob();

    const [isEditOpen, setIsEditOpen] = useState(false);
    const [isCancelOpen, setIsCancelOpen] = useState(false);
    const [isStartOpen, setIsStartOpen] = useState(false);
    const [isCompleteOpen, setIsCompleteOpen] = useState(false);

    const [updateSuccess, setUpdateSuccess] =
        useState<string | null>(null);

    const handleOpenEdit = () => {
        updateJobMutation.reset();
        setUpdateSuccess(null);
        setIsEditOpen(true);
    };

    const handleOpenStart = () => {
        startJobMutation.reset();
        setIsStartOpen(true);
    };

    const handleStartJob = () => {
        if (!job) {
            return;
        }

        startJobMutation.mutate(job.id, {
            onSuccess: () => {
                setIsStartOpen(false);
            },
        });
    };

    const handleOpenComplete = () => {
        completeJobMutation.reset();
        setIsCompleteOpen(true);
    };

    const handleCompleteJob = () => {
        if (!job) {
            return;
        }

        completeJobMutation.mutate(job.id, {
            onSuccess: () => {
                setIsCompleteOpen(false);
            },
        });
    };

    const handleOpenCancel = () => {
        cancelJobMutation.reset();
        setIsCancelOpen(true);
    };

    const handleCancelJob = () => {
        if (!job) {
            return;
        }

        cancelJobMutation.mutate(job.id, {
            onSuccess: () => {
                setIsCancelOpen(false);
            },
        });
    };

    const handleUpdate = (
        event: FormEvent<HTMLFormElement>
    ) => {
        event.preventDefault();

        if (!job) {
            return;
        }

        const data = new FormData(event.currentTarget);

        updateJobMutation.mutate(
            {
                id: job.id,
                jobNumber: String(
                    data.get("jobNumber") ?? ""
                ).trim(),
                description: String(
                    data.get("description") ?? ""
                ).trim(),
            },
            {
                onSuccess: () => {
                    setIsEditOpen(false);
                    setUpdateSuccess(
                        "Job updated successfully."
                    );
                },
            }
        );
    };

    const renderContent = () => {
        if (isLoading) {
            return (
                <div className="rounded-xl border border-slate-200 bg-white px-6 py-16 text-center text-[12px] text-slate-400 shadow-sm">
                    Loading job...
                </div>
            );
        }

        if (isError) {
            return (
                <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-[12px] text-rose-600">
                    Failed to load the job. Please try again.
                </div>
            );
        }

        if (!job) {
            return (
                <div className="rounded-xl border border-slate-200 bg-white px-6 py-16 text-center shadow-sm">
                    <p className="text-[13px] font-medium text-slate-600">
                        Job not found
                    </p>

                    <p className="mt-1 text-[12px] text-slate-400">
                        It may have been deleted or the link is
                        incorrect.
                    </p>
                </div>
            );
        }

        return (
            <>
                <JobDetailHeader
                    job={job}
                    onStart={handleOpenStart}
                    onConsume={() =>
                        navigate(
                            `/app/consumption?jobId=${job.id}`
                        )
                    }
                    onComplete={handleOpenComplete}
                    onEdit={handleOpenEdit}
                    onCancel={handleOpenCancel}
                />

                <JobInformation job={job} />

                <EditJobModal
                    job={job}
                    isOpen={isEditOpen}
                    busy={updateJobMutation.isPending}
                    error={
                        updateJobMutation.isError
                            ? getErrorMessage(
                                  updateJobMutation.error,
                                  "Failed to update job."
                              )
                            : null
                    }
                    onClose={() => setIsEditOpen(false)}
                    onSubmit={handleUpdate}
                />

                <StartJobModal
                    job={job}
                    isOpen={isStartOpen}
                    busy={startJobMutation.isPending}
                    onClose={() => setIsStartOpen(false)}
                    onConfirm={handleStartJob}
                />

                <CompleteJobModal
                    job={job}
                    isOpen={isCompleteOpen}
                    busy={completeJobMutation.isPending}
                    onClose={() =>
                        setIsCompleteOpen(false)
                    }
                    onConfirm={handleCompleteJob}
                />

                <CancelJobModal
                    job={job}
                    isOpen={isCancelOpen}
                    busy={cancelJobMutation.isPending}
                    onClose={() => setIsCancelOpen(false)}
                    onConfirm={handleCancelJob}
                />
            </>
        );
    };

    return (
        <AppLayout
            title="Job Detail"
            description="Review and manage a laboratory job"
        >
            <div className="min-h-full bg-[#f7fafc] px-6 py-7 lg:px-10">
                <div className="mx-auto max-w-[900px]">
                    <button
                        type="button"
                        onClick={() =>
                            navigate("/app/jobs")
                        }
                        className="mb-5 inline-flex items-center gap-2 text-[12px] font-medium text-slate-500 transition-colors hover:text-[#0da7ad]"
                    >
                        <ArrowLeft className="h-3.5 w-3.5" />
                        Back to Job List
                    </button>

                    {updateSuccess && (
                        <div className="mb-5 flex items-center gap-3 rounded-xl border border-green-200 bg-green-50 px-4 py-3 text-[12px] text-green-700">
                            <CheckCircle2 className="h-4 w-4 shrink-0" />
                            {updateSuccess}
                        </div>
                    )}

                    {renderContent()}
                </div>
            </div>
        </AppLayout>
    );
};