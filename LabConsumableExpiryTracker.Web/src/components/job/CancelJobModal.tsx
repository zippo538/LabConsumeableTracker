import { ConfirmDialog } from "../Modal";
import type { JobDto } from "../../types/job.types";

interface CancelJobModalProps {
    job: JobDto;
    isOpen: boolean;
    busy: boolean;
    onClose: () => void;
    onConfirm: () => void;
}

export const CancelJobModal = ({
    job,
    isOpen,
    busy,
    onClose,
    onConfirm,
}: CancelJobModalProps) => {
    return (
        <ConfirmDialog
            isOpen={isOpen}
            onClose={onClose}
            onConfirm={onConfirm}
            title="Cancel this job?"
            confirmLabel="Cancel Job"
            cancelLabel="Keep Job"
            busy={busy}
            impact={
                <>
                    <span className="font-semibold text-slate-700">
                        {job.jobNumber}
                    </span>{" "}
                    will be marked as Cancelled. Cancelled jobs are
                    read-only.
                </>
            }
        />
    );
};