import { ChevronDown, Search } from "lucide-react";

interface JobFiltersProps {
    search: string;
    sortLatest: boolean;
    onSearchChange: (value: string) => void;
    onSortChange: (latest: boolean) => void;
    onCreate: () => void;
}

export const JobFilters = ({
    search,
    sortLatest,
    onSearchChange,
    onSortChange,
    onCreate,
}: JobFiltersProps) => {
    return (
        <div className="mb-5 flex flex-col gap-3 rounded-xl border border-slate-200 bg-white p-2 shadow-sm md:flex-row">
            <div className="relative flex-1">
                <Search className="absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />

                <input
                    type="text"
                    value={search}
                    onChange={(event) =>
                        onSearchChange(event.target.value)
                    }
                    placeholder="Search for the job number..."
                    className="h-10 w-full rounded-lg border-0 bg-transparent pl-11 pr-4 text-[12px] text-slate-700 outline-none placeholder:text-slate-400 focus:ring-0"
                />
            </div>

            <div className="relative">
                <select
                    value={sortLatest ? "latest" : "oldest"}
                    onChange={(event) =>
                        onSortChange(
                            event.target.value === "latest"
                        )
                    }
                    className="h-10 min-w-[160px] appearance-none rounded-lg border border-slate-200 bg-white px-4 pr-9 text-[12px] font-medium text-slate-600 outline-none focus:border-[#18b4b5]"
                >
                    <option value="latest">Latest</option>
                    <option value="oldest">Oldest</option>
                </select>

                <ChevronDown className="pointer-events-none absolute right-3 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-slate-400" />
            </div>

            <button
                type="button"
                onClick={onCreate}
                className="h-10 rounded-lg bg-[#12a9ad] px-6 text-[12px] font-semibold text-white transition-colors hover:bg-[#0e969a]"
            >
                + Create Job
            </button>
        </div>
    );
};