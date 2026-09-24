import type { JobStatus } from "../../types/job.types";

export type StatusTab = "All" | JobStatus;

interface JobStatusTabsProps {
    tabs: { key: StatusTab; label: string }[];
    activeTab: StatusTab;
    counts: Record<StatusTab, number>;
    onChange: (tab: StatusTab) => void;
}

export const JobStatusTabs = ({
    tabs,
    activeTab,
    counts,
    onChange,
}: JobStatusTabsProps) => {
    return (
        <div
            role="tablist"
            aria-label="Filter jobs by status"
            className="flex overflow-x-auto border-b border-slate-200 px-3"
        >
            {tabs.map((tab) => {
                const isActive = activeTab === tab.key;

                return (
                    <button
                        key={tab.key}
                        type="button"
                        role="tab"
                        aria-selected={isActive}
                        onClick={() => onChange(tab.key)}
                        className={`relative flex shrink-0 items-center gap-2 whitespace-nowrap px-4 py-3.5 text-[12px] font-semibold transition-colors ${
                            isActive
                                ? "text-[#0da7ad]"
                                : "text-slate-500 hover:text-slate-700"
                        }`}
                    >
                        {tab.label}

                        <span
                            className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${
                                isActive
                                    ? "bg-[#12a9ad]/10 text-[#0da7ad]"
                                    : "bg-slate-100 text-slate-500"
                            }`}
                        >
                            {counts[tab.key]}
                        </span>

                        {isActive && (
                            <span className="absolute inset-x-3 bottom-0 h-0.5 rounded-full bg-[#12a9ad]" />
                        )}
                    </button>
                );
            })}
        </div>
    );
};