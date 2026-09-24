import { useEffect, useState } from "react";

import { useUpdateUser } from "../../hooks/useUser";

import type { UserDto } from "../../types/user.types";

import {
    FormModal,
    ModalField,
} from "../Modal";

interface EditUserModalProps {
    isOpen: boolean;
    user: UserDto;
    onClose: () => void;
    onSuccess: (message: string) => void;
}

interface EditUserForm {
    username: string;
    fullName: string;
    email: string;
}

type FieldErrors = Partial<
    Record<keyof EditUserForm, string[]>
>;

export function EditUserModal({
    isOpen,
    user,
    onClose,
    onSuccess,
}: EditUserModalProps) {
    const updateUserMutation = useUpdateUser();

    const [form, setForm] =
        useState<EditUserForm>({
            username: "",
            fullName: "",
            email: "",
        });

    const [error, setError] =
        useState<string | null>(null);

    const [fieldErrors, setFieldErrors] =
        useState<FieldErrors>({});

    useEffect(() => {
        if (!isOpen) return;

        setForm({
            username: user.username,
            fullName: user.fullName,
            email: user.email,
        });

        setError(null);
        setFieldErrors({});
    }, [isOpen, user]);

    const updateField = <
        K extends keyof EditUserForm,
    >(
        field: K,
        value: EditUserForm[K],
    ) => {
        setForm((current) => ({
            ...current,
            [field]: value,
        }));

        setFieldErrors((current) => ({
            ...current,
            [field]: undefined,
        }));

        setError(null);
    };

    const resetForm = () => {
        setForm({
            username: user.username,
            fullName: user.fullName,
            email: user.email,
        });

        setError(null);
        setFieldErrors({});
    };

    const handleClose = () => {
        if (updateUserMutation.isPending) {
            return;
        }

        resetForm();
        onClose();
    };

    const handleSubmit = async (
        event: React.FormEvent<HTMLFormElement>,
    ) => {
        event.preventDefault();

        setError(null);
        setFieldErrors({});

        try {
            const response =
                await updateUserMutation.mutateAsync({
                    id: user.id,
                    payload: {
                        username: form.username.trim(),
                        fullName: form.fullName.trim(),
                        email: form.email.trim(),
                    },
                });

            resetForm();

            onSuccess(
                response.message ||
                    "User updated successfully.",
            );

            onClose();
        } catch (error: any) {
            const response = error?.response;
            const data = response?.data;

            const errors = data?.errors;

            if (errors) {
                setFieldErrors({
                    username: errors.Username,
                    fullName: errors.FullName,
                    email: errors.Email,
                });
            }

            setError(
                data?.message ??
                    data?.title ??
                    "Failed to update user.",
            );
        }
    };

    return (
        <FormModal
            isOpen={isOpen}
            onClose={handleClose}
            title="Edit user"
            description="Update the user's account information."
            mode="update"
            submitLabel="Save changes"
            onSubmit={handleSubmit}
            busy={updateUserMutation.isPending}
            error={error}
        >
            <ModalField
                label="Username"
                htmlFor="edit-username"
                required
            >
                <input
                    id="edit-username"
                    name="username"
                    type="text"
                    value={form.username}
                    onChange={(event) =>
                        updateField(
                            "username",
                            event.target.value,
                        )
                    }
                    required
                    autoComplete="username"
                    className={`h-11 rounded-lg border px-3 text-sm outline-none transition focus:ring-2 focus:ring-teal-500/20 ${
                        fieldErrors.username?.length
                            ? "border-red-400 focus:border-red-500"
                            : "border-slate-200 focus:border-teal-500"
                    }`}
                    placeholder="john.doe"
                />

                {fieldErrors.username?.map(
                    (message) => (
                        <span
                            key={message}
                            className="text-xs font-normal text-red-600"
                        >
                            {message}
                        </span>
                    ),
                )}
            </ModalField>

            <ModalField
                label="Full name"
                htmlFor="edit-full-name"
                required
            >
                <input
                    id="edit-full-name"
                    name="fullName"
                    type="text"
                    value={form.fullName}
                    onChange={(event) =>
                        updateField(
                            "fullName",
                            event.target.value,
                        )
                    }
                    required
                    autoComplete="name"
                    className={`h-11 rounded-lg border px-3 text-sm outline-none transition focus:ring-2 focus:ring-teal-500/20 ${
                        fieldErrors.fullName?.length
                            ? "border-red-400 focus:border-red-500"
                            : "border-slate-200 focus:border-teal-500"
                    }`}
                    placeholder="John Doe"
                />

                {fieldErrors.fullName?.map(
                    (message) => (
                        <span
                            key={message}
                            className="text-xs font-normal text-red-600"
                        >
                            {message}
                        </span>
                    ),
                )}
            </ModalField>

            <ModalField
                label="Email"
                htmlFor="edit-email"
                required
            >
                <input
                    id="edit-email"
                    name="email"
                    type="email"
                    value={form.email}
                    onChange={(event) =>
                        updateField(
                            "email",
                            event.target.value,
                        )
                    }
                    required
                    autoComplete="email"
                    className={`h-11 rounded-lg border px-3 text-sm outline-none transition focus:ring-2 focus:ring-teal-500/20 ${
                        fieldErrors.email?.length
                            ? "border-red-400 focus:border-red-500"
                            : "border-slate-200 focus:border-teal-500"
                    }`}
                    placeholder="john@formulatrix.com"
                />

                {fieldErrors.email?.map(
                    (message) => (
                        <span
                            key={message}
                            className="text-xs font-normal text-red-600"
                        >
                            {message}
                        </span>
                    ),
                )}
            </ModalField>
        </FormModal>
    );
}