import {
    CheckCircle2,
    FlaskConical,
    Play,
} from "lucide-react";
import { Link } from "react-router";

import type { JobDto } from "../../types/job.types";
import { formatDate, getStatusConfig } from "../../helpers/jobStatus";

interface JobTableProps {
    jobs: JobDto[];
    activeTab: string;
    isStarting: boolean;
    onOpenDetail: (job: JobDto) => void;
    onStartJob: (job: JobDto) => void;
    onConsume: (job: JobDto) => void;
    emptyContent: React.ReactNode;
}

export const JobTable = ({
    jobs,
    activeTab,
    isStarting,
    onOpenDetail,
    onStartJob,
    onConsume,
    emptyContent,
}: JobTableProps) => {
    return (
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
                    {jobs.length === 0 ? (
                        <tr>
                            <td
                                colSpan={6}
                                className="px-6 py-14 text-center"
                            >
                                {emptyContent}
                            </td>
                        </tr>
                    ) : (
                        jobs.map((job) => {
                            const cfg = getStatusConfig(job.status);

                            const isDimmed =
                                activeTab === "All" &&
                                (job.status === "Completed" ||
                                    job.status === "Cancelled");

                            return (
                                <tr
                                    key={job.id}
                                    onClick={() =>
                                        onOpenDetail(job)
                                    }
                                    className={`cursor-pointer border-b border-slate-100 last:border-b-0 hover:bg-slate-50/70 ${
                                        isDimmed
                                            ? "opacity-70 hover:opacity-100"
                                            : ""
                                    }`}
                                >
                                    <td className="px-6 py-5">
                                        <div>
                                            <Link
                                                to={`/jobs/${job.id}`}
                                                onClick={(event) =>
                                                    event.stopPropagation()
                                                }
                                                className="text-[13px] font-semibold text-[#0da7ad] hover:underline"
                                            >
                                                {job.jobNumber}
                                            </Link>

                                            <p className="mt-1 max-w-[220px] truncate text-[11px] text-slate-500">
                                                {job.description ||
                                                    "No description"}
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
                                                    onStartJob(job);
                                                }}
                                                disabled={isStarting}
                                                className="inline-flex h-10 items-center gap-2 rounded-lg border border-yellow-300 bg-amber-100 px-4 text-[11px] font-semibold text-amber-600 transition-colors hover:bg-amber-300 disabled:cursor-not-allowed disabled:opacity-50"
                                            >
                                                <Play className="h-3.5 w-3.5" />

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
                                                className="inline-flex h-10 items-center gap-2 rounded-lg bg-[#12a9ad] px-5 text-[11px] font-semibold text-white transition-colors hover:bg-[#0e969a]"
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
    );
};