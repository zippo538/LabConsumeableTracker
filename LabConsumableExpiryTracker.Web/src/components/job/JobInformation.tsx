import type { JobDto } from "../../types/job.types";
import { formatDate, getStatusConfig } from "../../helpers/jobStatus";

interface JobInformationProps {
    job: JobDto;
}

export const JobInformation = ({
    job,
}: JobInformationProps) => {
    const cfg = getStatusConfig(job.status);

    return (
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
    );
};