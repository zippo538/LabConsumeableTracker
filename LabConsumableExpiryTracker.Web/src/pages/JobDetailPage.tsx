import { type FormEvent, useState } from "react";
import { useNavigate, useParams } from "react-router";
import { ArrowLeft, Ban, Pencil, CheckCircle2 } from "lucide-react";

import { useCancelJob, useJob, useUpdateJob } from "../hooks/useJob";

import { AppLayout } from "../components/layout/AppLayout";
import {
    ConfirmDialog,
    FormModal,
    ModalField,
} from "../components/Modal";

import { getErrorMessage } from "../helpers/getErrorMessage";

import {
    canCancelJob,
    canEditJob,
    formatDate,
    getStatusConfig,
} from "../helpers/jobStatus";


const inputClass =
    "w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-normal text-slate-900 placeholder:text-slate-400 focus:border-teal-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-teal-500 disabled:opacity-50";

export const JobDetailPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const { data: job, isLoading, isError } = useJob(id ?? "");

    const updateJobMutation = useUpdateJob();
    const cancelJobMutation = useCancelJob();

    const [isEditOpen, setIsEditOpen] = useState(false);
    const [isCancelOpen, setIsCancelOpen] = useState(false);

    const [updateSuccess, setUpdateSuccess] = useState<string | null>(null);

    const handleOpenEdit = () => {
        updateJobMutation.reset();
        setUpdateSuccess(null);
        setIsEditOpen(true);
    };

    const handleOpenCancel = () => {
        cancelJobMutation.reset();
        setIsCancelOpen(true);
    };
    const handleUpdate = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        if (!job) {
            return;
        }

        const data = new FormData(event.currentTarget);

        updateJobMutation.mutate(
            {
                id: job.id,
                jobNumber: String(data.get("jobNumber") ?? "").trim(),
                description: String(data.get("description") ?? "").trim(),
            },
            {
                onSuccess: () => {
                    setIsEditOpen(false);
                    setUpdateSuccess("Job updated successfully.");
                },
            }
        );
    };

    const handleCancelJob = () => {
        if (!job) {
            return;
        }

        cancelJobMutation.mutate(job.id, {
            onSuccess: () => setIsCancelOpen(false),
        });
    };

    const renderBody = () => {
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
                        It may have been deleted or the link is incorrect.
                    </p>
                </div>
            );
        }

        const cfg = getStatusConfig(job.status);
        const editable = canEditJob(job.status);
        const cancellable = canCancelJob(job.status);

        const restrictionNote =
            !editable && !cancellable
                ? "Completed and cancelled jobs are read-only."
                : null;

        return (
            <>
                <div className="mb-6 flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
                    <div>
                        <div className="flex flex-wrap items-center gap-3">
                            <h1 className="text-[26px] font-bold tracking-tight text-slate-900">
                                {job.jobNumber}
                            </h1>

                            <span
                                className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1.5 text-[10px] font-semibold ${cfg.badge}`}
                            >
                                <span
                                    className={`h-1.5 w-1.5 rounded-full ${cfg.dot}`}
                                />
                                {cfg.label}
                            </span>
                        </div>

                        <p className="mt-1 text-[13px] text-slate-500">
                            {cfg.summary}
                        </p>
                    </div>

                    <div className="flex shrink-0 items-center gap-3">
                        <button
                            type="button"
                            onClick={handleOpenEdit}
                            disabled={!editable}
                            title={
                                editable
                                    ? undefined
                                    : "This job can't be edited"
                            }
                            className="inline-flex h-10 items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 text-[12px] font-semibold text-slate-700 transition-colors hover:border-[#18b4b5] hover:text-[#0da7ad] disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:border-slate-200 disabled:hover:text-slate-700"
                        >
                            <Pencil className="h-3.5 w-3.5" />
                            Edit
                        </button>

                        <button
                            type="button"
                            onClick={handleOpenCancel}
                            disabled={!cancellable}
                            title={
                                cancellable
                                    ? undefined
                                    : "This job can't be cancelled"
                            }
                            className="inline-flex h-10 items-center gap-2 rounded-lg border border-rose-200 bg-white px-4 text-[12px] font-semibold text-rose-600 transition-colors hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-white"
                        >
                            <Ban className="h-3.5 w-3.5" />
                            Cancel Job
                        </button>
                    </div>
                </div>

                {restrictionNote && (
                    <p className="-mt-3 mb-6 text-[11px] text-slate-400">
                        {restrictionNote}
                    </p>
                )}
                
                <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
                    <div className="border-b border-slate-200 px-6 py-4">
                        <h2 className="text-[14px] font-semibold text-slate-900">
                            Job information
                        </h2>
                    </div>

                    <dl className="grid grid-cols-1 gap-x-8 gap-y-6 px-6 py-6 md:grid-cols-2">
                        <div>
                            <dt className="text-[10px] font-semibold uppercase tracking-wide text-slate-400">
                                Job number
                            </dt>

                            <dd className="mt-1.5 text-[13px] font-semibold text-[#0da7ad]">
                                {job.jobNumber}
                            </dd>
                        </div>

                        <div>
                            <dt className="text-[10px] font-semibold uppercase tracking-wide text-slate-400">
                                Status
                            </dt>

                            <dd className="mt-1.5 text-[13px] text-slate-700">
                                {cfg.label}
                            </dd>
                        </div>

                        <div>
                            <dt className="text-[10px] font-semibold uppercase tracking-wide text-slate-400">
                                Start
                            </dt>

                            <dd className="mt-1.5 text-[13px] text-slate-700">
                                {formatDate(job.startedAt)}
                            </dd>
                        </div>

                        <div>
                            <dt className="text-[10px] font-semibold uppercase tracking-wide text-slate-400">
                                Finish
                            </dt>

                            <dd className="mt-1.5 text-[13px] text-slate-700">
                                {formatDate(job.completedAt)}
                            </dd>
                        </div>

                        <div className="md:col-span-2">
                            <dt className="text-[10px] font-semibold uppercase tracking-wide text-slate-400">
                                Description
                            </dt>

                            <dd className="mt-1.5 whitespace-pre-wrap text-[13px] leading-6 text-slate-700">
                                {job.description || "No description"}
                            </dd>
                        </div>
                    </dl>
                </div>
                
                <FormModal
                    isOpen={isEditOpen}
                    onClose={() => setIsEditOpen(false)}
                    mode="update"
                    title="Edit job"
                    description="Update the job number or description."
                    submitLabel="Save changes"
                    busy={updateJobMutation.isPending}
                    error={
                        updateJobMutation.isError
                            ? getErrorMessage(
                                updateJobMutation.error,
                                "Failed to update job."
                            )
                            : null
                    }
                    identity={
                        <div className="flex items-center justify-between text-sm">
                            <span className="font-medium text-slate-500">
                                Status
                            </span>

                            <span className="font-semibold text-slate-800">
                                {cfg.label}
                            </span>
                        </div>
                    }
                    onSubmit={handleUpdate}
                >
                    <ModalField label="Job number" required>
                        <input
                            name="jobNumber"
                            required
                            defaultValue={job.jobNumber}
                            className={`${inputClass} h-11`}
                        />
                    </ModalField>

                    <ModalField label="Description">
                        <textarea
                            name="description"
                            rows={4}
                            defaultValue={job.description ?? ""}
                            className={`${inputClass} resize-y py-2.5`}
                        />
                    </ModalField>
                </FormModal>

                <ConfirmDialog
                    isOpen={isCancelOpen}
                    onClose={() => setIsCancelOpen(false)}
                    onConfirm={handleCancelJob}
                    title="Cancel this job?"
                    confirmLabel="Cancel Job"
                    cancelLabel="Keep Job"
                    busy={cancelJobMutation.isPending}
                    impact={
                        <>
                            <span className="font-semibold text-slate-700">
                                {job.jobNumber}
                            </span>{" "}
                            will be marked as Cancelled. Cancelled jobs are
                            read-only.
                        </>
                    }
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
                        onClick={() => navigate("/app/jobs")}
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
                    {renderBody()}
                </div>
            </div>
        </AppLayout>
    );
};