import {
    CheckCircle2,
    FlaskConical,
} from "lucide-react";

import type { JobDto } from "../../types/job.types";
import { formatDate, getStatusConfig } from "../../helpers/jobStatus";

interface JobMobileListProps {
    jobs: JobDto[];
    activeTab: string;
    isStarting: boolean;
    onOpenDetail: (job: JobDto) => void;
    onStartJob: (job: JobDto) => void;
    onConsume: (job: JobDto) => void;
    emptyContent: React.ReactNode;
}

export const JobMobileList = ({
    jobs,
    activeTab,
    isStarting,
    onOpenDetail,
    onStartJob,
    onConsume,
    emptyContent,
}: JobMobileListProps) => {
    if (jobs.length === 0) {
        return (
            <div className="px-5 py-12 text-center">
                {emptyContent}
            </div>
        );
    }

    return (
        <div className="divide-y divide-slate-100 md:hidden">
            {jobs.map((job) => {
                const cfg = getStatusConfig(job.status);

                const isDimmed =
                    activeTab === "All" &&
                    (job.status === "Completed" ||
                        job.status === "Cancelled");

                return (
                    <div
                        key={job.id}
                        onClick={() => onOpenDetail(job)}
                        className={`cursor-pointer p-5 ${
                            isDimmed ? "opacity-70" : ""
                        }`}
                    >
                        <div className="flex items-start justify-between gap-4">
                            <div>
                                <p className="text-[13px] font-semibold text-[#0da7ad]">
                                    {job.jobNumber}
                                </p>

                                <p className="mt-1 text-[11px] text-slate-500">
                                    {job.description ||
                                        "No description"}
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
                                        onStartJob(job);
                                    }}
                                    disabled={isStarting}
                                    className="w-full rounded-lg border border-slate-200 py-2.5 text-[11px] font-semibold text-slate-700 disabled:opacity-50"
                                >
                                    {isStarting
                                        ? "Starting..."
                                        : "Start Job"}
                                </button>
                            )}

                            {job.status === "InProgress" && (
                                <button
                                    type="button"
                                    onClick={(event) => {
                                        event.stopPropagation();
                                        onConsume(job);
                                    }}
                                    className="inline-flex w-full items-center justify-center gap-2 rounded-lg bg-[#12a9ad] py-2.5 text-[11px] font-semibold text-white"
                                >
                                    <FlaskConical className="h-3.5 w-3.5" />
                                    Consume
                                </button>
                            )}

                            {job.status === "Completed" && (
                                <button
                                    type="button"
                                    onClick={(event) => {
                                        event.stopPropagation();
                                        onOpenDetail(job);
                                    }}
                                    className="inline-flex w-full items-center justify-center gap-2 rounded-lg border border-slate-200 py-2.5 text-[11px] font-semibold text-slate-700"
                                >
                                    <CheckCircle2 className="h-3.5 w-3.5" />
                                    View details
                                </button>
                            )}
                        </div>
                    </div>
                );
            })}
        </div>
    );
};