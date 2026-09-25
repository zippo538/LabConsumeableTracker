import type { FormEvent } from "react";

import {
    FormModal,
    ModalField,
} from "../Modal";

import type { JobDto } from "../../types/job.types";
import { getStatusConfig } from "../../helpers/jobStatus";

interface EditJobModalProps {
    job: JobDto;
    isOpen: boolean;
    busy: boolean;
    error: string | null;
    onClose: () => void;
    onSubmit: (event: FormEvent<HTMLFormElement>) => void;
}

const inputClass =
    "w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-normal text-slate-900 placeholder:text-slate-400 focus:border-teal-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-teal-500 disabled:opacity-50";

export const EditJobModal = ({
    job,
    isOpen,
    busy,
    error,
    onClose,
    onSubmit,
}: EditJobModalProps) => {
    const cfg = getStatusConfig(job.status);

    return (
        <FormModal
            isOpen={isOpen}
            onClose={onClose}
            mode="update"
            title="Edit job"
            description="Update the job number or description."
            submitLabel="Save changes"
            busy={busy}
            error={error}
            identity={
                <div className="flex items-center justify-between text-sm">
                    <span className="font-medium text-slate-500">
                        Status
                    </span>

                    <span className="font-semibold text-slate-800">
                        {cfg.label}
                    </span>
                </div>
            }
            onSubmit={onSubmit}
        >
            <ModalField label="Job number" required>
                <input
                    name="jobNumber"
                    required
                    defaultValue={job.jobNumber}
                    className={`${inputClass} h-11`}
                />
            </ModalField>

            <ModalField label="Description">
                <textarea
                    name="description"
                    rows={4}
                    defaultValue={job.description ?? ""}
                    className={`${inputClass} resize-y py-2.5`}
                />
            </ModalField>
        </FormModal>
    );
};