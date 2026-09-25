import { FlaskConical } from "lucide-react";

export const JobWorkflowInfo = () => {
    return (
        <div className="mb-5 rounded-xl border border-[#b9e8eb] bg-[#eafafd] px-5 py-3.5">
            <div className="flex items-start gap-3">
                <div className="mt-0.5">
                    <FlaskConical className="h-4 w-4 text-[#11a9ae]" />
                </div>

                <div>
                    <p className="text-[12px] font-semibold text-[#0da3a8]">
                        Scientist workflow
                    </p>

                    <p className="mt-1.5 text-[12px] font-medium text-[#14546a]">
                        Draft → Start → Consume (FEFO) → Complete
                    </p>

                    <p className="mt-1 text-[10px] text-[#367286]">
                        Consume is only available for Jobs In Progress.
                    </p>
                </div>
            </div>
        </div>
    );
};