import { ConfirmDialog } from "../Modal";
import type { JobDto } from "../../types/job.types";
import { formatDate } from "../../helpers/jobStatus";

interface CompleteJobModalProps {
    job: JobDto;
    isOpen: boolean;
    busy: boolean;
    onClose: () => void;
    onConfirm: () => void;
}

export const CompleteJobModal = ({
    job,
    isOpen,
    busy,
    onClose,
    onConfirm,
}: CompleteJobModalProps) => {
    return (
        <ConfirmDialog
            isOpen={isOpen}
            onClose={onClose}
            onConfirm={onConfirm}
            title="Complete this job?"
            confirmLabel="Complete Job"
            cancelLabel="Keep In Progress"
            busy={busy}
            impact={
                <div className="space-y-4">
                    <p className="text-[12px] text-slate-500">
                        Please review the job details before marking
                        this job as completed.
                    </p>

                    <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
                        <dl className="space-y-3">
                            <div className="flex items-start justify-between gap-4">
                                <dt className="text-[11px] font-medium text-slate-500">
                                    Job number
                                </dt>

                                <dd className="text-right text-[12px] font-semibold text-slate-800">
                                    {job.jobNumber}
                                </dd>
                            </div>

                            <div className="flex items-start justify-between gap-4">
                                <dt className="text-[11px] font-medium text-slate-500">
                                    Status
                                </dt>

                                <dd className="text-right text-[12px] font-semibold text-teal-700">
                                    In Progress
                                </dd>
                            </div>

                            <div className="flex items-start justify-between gap-4">
                                <dt className="text-[11px] font-medium text-slate-500">
                                    Started
                                </dt>

                                <dd className="text-right text-[12px] text-slate-700">
                                    {formatDate(job.startedAt)}
                                </dd>
                            </div>

                            <div>
                                <dt className="text-[11px] font-medium text-slate-500">
                                    Description
                                </dt>

                                <dd className="mt-1 text-[12px] leading-5 text-slate-700">
                                    {job.description || "No description"}
                                </dd>
                            </div>
                        </dl>
                    </div>

                    <div className="rounded-lg border border-amber-200 bg-amber-50 px-3 py-2.5">
                        <p className="text-[11px] leading-5 text-amber-700">
                            Once completed, this job will become read-only
                            and can no longer be edited, consumed, or
                            cancelled.
                        </p>
                    </div>
                </div>
            }
        />
    );
};