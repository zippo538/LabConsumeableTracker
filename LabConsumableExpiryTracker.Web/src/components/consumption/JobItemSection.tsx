import React from "react";
import { FlaskConical, AlertCircle,Loader2 } from "lucide-react";
import type { JobDto } from "../../types/job.types";
import type { ItemDto } from "../../types/item.types";

interface JobItemSectionProps {
  jobs: JobDto[];
  items: ItemDto[];
  selectedJobId: string;
  onSelectJobId: (jobId: string) => void;
  selectedItemId: string;
  onSelectItemId: (itemId: string) => void;
  requestedQuantity: string;
  onRequestedQuantityChange: (val: string) => void;
  onPreview: () => void;
  isPreviewLoading: boolean;
  isLoadingJobs: boolean;
  isLoadingItems: boolean;
}

export const JobItemSection: React.FC<JobItemSectionProps> = ({
  jobs,
  items,
  selectedJobId,
  onSelectJobId,
  selectedItemId,
  onSelectItemId,
  requestedQuantity,
  onRequestedQuantityChange,
  onPreview,
  isPreviewLoading,
  isLoadingJobs,
  isLoadingItems,
}) => {
  const inProgressJobs = jobs.filter((j) => j.status === "InProgress");
  const selectedJob = inProgressJobs.find((j) => j.id === selectedJobId);
  const selectedItem = items.find((i) => i.id === selectedItemId);

  const isFormValid =
    Boolean(selectedJobId) &&
    Boolean(selectedItemId) &&
    parseFloat(requestedQuantity) > 0;

  return (
    <div className="flex flex-col justify-between rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
      <div className="space-y-5">
        <div>
          <h2 className="text-[17px] font-bold tracking-tight text-slate-900">
            1. Select Job
          </h2>
          <p className="mt-0.5 text-[12px] text-slate-400">
            Usage context for the laboratory
          </p>
        </div>

        <div>
          <label
            htmlFor="job-select"
            className="mb-1.5 block text-[11px] font-semibold uppercase tracking-wider text-slate-500"
          >
            Active job
          </label>

          {isLoadingJobs ? (
            <div className="h-10 w-full animate-pulse rounded-lg bg-slate-100" />
          ) : inProgressJobs.length === 0 ? (
            <div className="flex items-center gap-2 rounded-lg border border-amber-200 bg-amber-50 p-3 text-[12px] text-amber-800">
              <AlertCircle className="h-4 w-4 shrink-0 text-amber-600" />
              <span>
                No jobs currently in progress. Start a job in Job List first.
              </span>
            </div>
          ) : (
            <select
              id="job-select"
              value={selectedJobId}
              onChange={(e) => onSelectJobId(e.target.value)}
              className="h-11 w-full rounded-xl border border-slate-200 bg-white px-3 text-[13px] font-medium text-slate-800 outline-none transition focus:border-[#12a9ad] focus:ring-2 focus:ring-[#12a9ad]/10"
            >
              <option value="">-- Choose active job --</option>
              {inProgressJobs.map((job) => (
                <option key={job.id} value={job.id}>
                  {job.jobNumber} - {job.description || "No description"}
                </option>
              ))}
            </select>
          )}

          {selectedJob && (
            <div className="mt-2.5 rounded-xl border border-[#b9e8eb] bg-[#eafafd] p-3.5">
              <div className="flex items-center justify-between">
                <span className="text-[13px] font-bold text-[#0da7ad]">
                  {selectedJob.jobNumber}
                </span>
                <span className="inline-flex items-center gap-1 rounded-full bg-emerald-100 px-2.5 py-0.5 text-[10px] font-semibold text-emerald-700">
                  <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
                  In Progress
                </span>
              </div>
              <p className="mt-1 text-[11px] text-slate-600">
                {selectedJob.description || "No description provided."}
              </p>
            </div>
          )}
        </div>

        <div>
          <label
            htmlFor="item-select"
            className="mb-1.5 block text-[11px] font-semibold uppercase tracking-wider text-slate-500"
          >
            Item
          </label>

          {isLoadingItems ? (
            <div className="h-10 w-full animate-pulse rounded-lg bg-slate-100" />
          ) : (
            <select
              id="item-select"
              value={selectedItemId}
              onChange={(e) => onSelectItemId(e.target.value)}
              className="h-11 w-full rounded-xl border border-slate-200 bg-white px-3 text-[13px] font-medium text-slate-800 outline-none transition focus:border-[#12a9ad] focus:ring-2 focus:ring-[#12a9ad]/10"
            >
              <option value="">-- Choose master item --</option>
              {items.map((item) => (
                <option key={item.id} value={item.id}>
                  [{item.code}] {item.name} ({item.baseUnit})
                </option>
              ))}
            </select>
          )}

          {selectedItem && (
            <div className="mt-2 flex items-center justify-between text-[11px] text-slate-500">
              <span>
                Unit:{" "}
                <strong className="text-slate-700">{selectedItem.baseUnit}</strong>
              </span>
              <span>
                Available Usable Stock:{" "}
                <strong className="text-[#0da7ad]">
                  {selectedItem.totalRemainingQuantity} {selectedItem.baseUnit}
                </strong>
              </span>
            </div>
          )}
        </div>

        <div>
          <label
            htmlFor="quantity-input"
            className="mb-1.5 block text-[11px] font-semibold uppercase tracking-wider text-slate-500"
          >
            Requested quantity
          </label>
          <div className="relative">
            <input
              id="quantity-input"
              type="number"
              min="0.001"
              step="any"
              value={requestedQuantity}
              onChange={(e) => onRequestedQuantityChange(e.target.value)}
              placeholder="e.g. 27"
              className="h-11 w-full rounded-xl border border-slate-200 bg-white px-3.5 pr-16 text-[14px] font-semibold text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-[#12a9ad] focus:ring-2 focus:ring-[#12a9ad]/10"
            />
            {selectedItem && (
              <span className="absolute right-3.5 top-1/2 -translate-y-1/2 text-[12px] font-medium text-slate-400">
                {selectedItem.baseUnit}
              </span>
            )}
          </div>
        </div>

        <div>
          <span className="block text-[11px] font-semibold uppercase tracking-wider text-slate-500">
            Job notes
          </span>
          <p className="mt-1 rounded-lg border border-slate-100 bg-slate-50/70 p-2.5 text-[11px] text-slate-600">
            {selectedJob?.description || "No specific notes for this job."}
          </p>
        </div>

        <div className="rounded-xl border border-slate-200 bg-slate-50/50 p-3">
          <p className="text-[11px] font-bold text-slate-700">Scientist access</p>
          <p className="mt-0.5 text-[10px] text-slate-500">
            Can only consume for assigned in-progress jobs using FEFO allocation.
          </p>
        </div>
      </div>

  <div className="mt-6 pt-4 border-t border-slate-100">
  <button
    type="button"
    onClick={onPreview}
    disabled={!isFormValid || isPreviewLoading}
    className="flex h-11 w-full items-center justify-center gap-2 rounded-xl bg-[#12a9ad] px-4 text-[13px] font-semibold text-white shadow-sm transition hover:bg-[#0e969a] disabled:cursor-not-allowed disabled:opacity-40"
  >
    {isPreviewLoading ? (
      <>
        <Loader2 className="h-4 w-4 animate-spin" />
        <span>Calculating FEFO Preview...</span>
      </>
    ) : (
      <>
        <FlaskConical className="h-4 w-4" />
        <span>Preview FEFO Allocation</span>
      </>
    )}
  </button>
</div>
    </div>
  );
};
