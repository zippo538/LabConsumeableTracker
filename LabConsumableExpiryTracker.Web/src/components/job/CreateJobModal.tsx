import type { FormEvent } from "react";

import {
    FormModal,
    ModalField,
} from "../Modal";

interface CreateJobModalProps {
    isOpen: boolean;
    nextJobNumber: string;
    busy: boolean;
    error: string | null;
    onClose: () => void;
    onSubmit: (event: FormEvent<HTMLFormElement>) => void;
}

const inputClass =
    "w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-normal text-slate-900 placeholder:text-slate-400 focus:border-teal-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-teal-500 disabled:opacity-50";

export const CreateJobModal = ({
    isOpen,
    nextJobNumber,
    busy,
    error,
    onClose,
    onSubmit,
}: CreateJobModalProps) => {
    return (
        <FormModal
            isOpen={isOpen}
            onClose={onClose}
            mode="create"
            title="Create job"
            description="New jobs start as Draft. Start a job when you're ready to consume stock."
            submitLabel="Create job"
            busy={busy}
            error={error}
            onSubmit={onSubmit}
        >
            <ModalField
                label="Job number"
                required
                hint="Suggested automatically. You can change it."
            >
                <input
                    name="jobNumber"
                    required
                    defaultValue={nextJobNumber}
                    className={`${inputClass} h-11`}
                />
            </ModalField>

            <ModalField label="Description">
                <textarea
                    name="description"
                    rows={4}
                    className={`${inputClass} resize-y py-2.5`}
                />
            </ModalField>
        </FormModal>
    );
};