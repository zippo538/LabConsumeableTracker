import { ConfirmDialog } from "../Modal";
import type { JobDto } from "../../types/job.types";

interface StartJobModalProps {
    job: JobDto | null;
    isOpen: boolean;
    busy: boolean;
    onClose: () => void;
    onConfirm: () => void;
}


export const StartJobModal = ({
    job,
    isOpen,
    busy,
    onClose,
    onConfirm,
}: StartJobModalProps) => {
    if (!job) return null;

    return (
        <ConfirmDialog
            isOpen={isOpen}
            onClose={onClose}
            onConfirm={onConfirm}
            title="Start this job?"
            confirmLabel="Start Job"
            cancelLabel="Keep as Draft"
            busy={busy}
            impact={
                <div className="space-y-4">
                    <p className="text-[12px] text-slate-500">
                        Please review the job details before starting
                        this job.
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

                                <dd className="text-right text-[12px] font-semibold text-slate-600">
                                    Draft
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

                    <div className="rounded-lg border border-teal-200 bg-teal-50 px-3 py-2.5">
                        <p className="text-[11px] leading-5 text-teal-700">
                            Starting this job will change its status to
                            In Progress and allow consumable usage to be
                            recorded.
                        </p>
                    </div>
                </div>
            }
        />
    );
};