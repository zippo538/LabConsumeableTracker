import { Clock3 } from "lucide-react";

export type EmptyAction = "create" | "drafts" | null;

interface JobEmptyStateProps {
    title: string;
    hint: string;
    action: EmptyAction;
    onCreate: () => void;
    onViewDrafts: () => void;
}

export const JobEmptyState = ({
    title,
    hint,
    action,
    onCreate,
    onViewDrafts,
}: JobEmptyStateProps) => {
    return (
        <div className="flex flex-col items-center">
            <Clock3 className="mb-3 h-6 w-6 text-slate-300" />

            <p className="text-[12px] font-medium text-slate-500">
                {title}
            </p>

            <p className="mt-1 text-[11px] text-slate-400">
                {hint}
            </p>

            {action && (
                <button
                    type="button"
                    onClick={
                        action === "create"
                            ? onCreate
                            : onViewDrafts
                    }
                    className="mt-4 h-9 rounded-lg border border-slate-200 bg-white px-4 text-[11px] font-semibold text-slate-700 transition-colors hover:border-[#18b4b5] hover:text-[#0da7ad]"
                >
                    {action === "create"
                        ? "+ Create Job"
                        : "View drafts"}
                </button>
            )}
        </div>
    );
};