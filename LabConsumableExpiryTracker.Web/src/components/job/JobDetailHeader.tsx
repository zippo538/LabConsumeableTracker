import {
    Ban,
    CircleCheck,
    FlaskConical,
    Pencil,
    Play,
} from "lucide-react";

import type { JobDto } from "../../types/job.types";
import {
    canCancelJob,
    canEditJob,
    getStatusConfig,
} from "../../helpers/jobStatus";

interface JobDetailHeaderProps {
    job: JobDto;
    onStart: () => void;
    onConsume: () => void;
    onComplete: () => void;
    onEdit: () => void;
    onCancel: () => void;
}

export const JobDetailHeader = ({
    job,
    onStart,
    onConsume,
    onComplete,
    onEdit,
    onCancel,
}: JobDetailHeaderProps) => {
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

                <div className="flex shrink-0 flex-wrap items-center gap-3">
                    {job.status === "Draft" && (
                        <button
                            type="button"
                            onClick={onStart}
                            className="inline-flex h-10 items-center gap-2 rounded-lg border border-yellow-300 bg-amber-100 px-4 text-[11px] font-semibold text-amber-600 transition-colors hover:bg-amber-300"
                        >
                            <Play className="h-3.5 w-3.5" />
                            Start Job
                        </button>
                    )}

                    {job.status === "InProgress" && (
                        <>
                            <button
                                type="button"
                                onClick={onConsume}
                                className="inline-flex h-10 items-center gap-2 rounded-lg bg-[#12a9ad] px-4 text-[12px] font-semibold text-white transition-colors hover:bg-[#0e969a]"
                            >
                                <FlaskConical className="h-3.5 w-3.5" />
                                Consume
                            </button>

                            <button
                                type="button"
                                onClick={onComplete}
                                className="inline-flex h-10 items-center gap-2 rounded-lg border border-green-200 bg-green-50 px-4 text-[12px] font-semibold text-green-700 transition-colors hover:border-green-300 hover:bg-green-100"
                            >
                                <CircleCheck className="h-3.5 w-3.5" />
                                Complete Job
                            </button>
                        </>
                    )}

                    <button
                        type="button"
                        onClick={onEdit}
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
                        onClick={onCancel}
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
        </>
    );
};