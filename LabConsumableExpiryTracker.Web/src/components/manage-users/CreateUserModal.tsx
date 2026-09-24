import { useState } from "react";

import { useCreateUser } from "../../hooks/useUser";

import type { UserRole } from "../../types/user.types";

import {
    FormModal,
    ModalField,
} from "../Modal";
import { Eye, EyeOff } from "lucide-react";

interface CreateUserModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSuccess: (message: string) => void;
}

interface CreateUserForm {
    username: string;
    fullName: string;
    email: string;
    password: string;
    role: UserRole;
}

type FieldErrors = Partial<
    Record<keyof CreateUserForm, string[]>
>;

const INITIAL_FORM: CreateUserForm = {
    username: "",
    fullName: "",
    email: "",
    password: "",
    role: "Scientist",
};

export function CreateUserModal({
    isOpen,
    onClose,
    onSuccess,
}: CreateUserModalProps) {
    const createUserMutation = useCreateUser();

    const [form, setForm] =
        useState<CreateUserForm>(INITIAL_FORM);

    const [error, setError] =
        useState<string | null>(null);

    const [fieldErrors, setFieldErrors] =
        useState<FieldErrors>({});

    const [showPassword, setShowPassword] = useState(false);

    const updateField = <
        K extends keyof CreateUserForm,
    >(
        field: K,
        value: CreateUserForm[K],
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
        setForm(INITIAL_FORM);
        setError(null);
        setFieldErrors({});
    };

    const handleClose = () => {
        if (createUserMutation.isPending) {
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
                await createUserMutation.mutateAsync({
                    username: form.username.trim(),
                    fullName: form.fullName.trim(),
                    email: form.email.trim(),
                    password: form.password,
                    role: form.role,
                });

            resetForm();
            onSuccess(
                response.message ||
                "User created successfully.",
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
                    password: errors.Password,
                    role: errors.Role,
                });
            }

            setError(
                data?.message ??
                data?.title ??
                "Failed to create user.",
            );
        }
    };

    return (
        <FormModal
            isOpen={isOpen}
            onClose={handleClose}
            title="Add user"
            description="Create a new user account and assign an initial role."
            mode="create"
            submitLabel="Create user"
            onSubmit={handleSubmit}
            busy={createUserMutation.isPending}
            error={error}
        >
            <ModalField
                label="Username"
                htmlFor="create-username"
                required
            >
                <input
                    id="create-username"
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
                    className="h-11 rounded-lg border border-slate-200 px-3 text-sm outline-none transition focus:border-teal-500 focus:ring-2 focus:ring-teal-500/20"
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
                htmlFor="create-full-name"
                required
            >
                <input
                    id="create-full-name"
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
                    className="h-11 rounded-lg border border-slate-200 px-3 text-sm outline-none transition focus:border-teal-500 focus:ring-2 focus:ring-teal-500/20"
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
                htmlFor="create-email"
                required
            >
                <input
                    id="create-email"
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
                    className={`h-11 rounded-lg border px-3 text-sm outline-none transition focus:ring-2 focus:ring-teal-500/20 ${fieldErrors.email?.length
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

            <ModalField
                label="Password"
                htmlFor="create-password"
                required
                hint="The password must satisfy the server's Identity password policy."
            >
                <div className="relative">
                    <input
                        id="create-password"
                        name="password"
                        type={showPassword ? "text" : "password"}
                        value={form.password}
                        onChange={(event) =>
                            updateField(
                                "password",
                                event.target.value,
                            )
                        }
                        required
                        autoComplete="new-password"
                        className={`h-11 w-full rounded-lg border px-3 pr-10 text-sm outline-none transition focus:ring-2 focus:ring-teal-500/20 ${fieldErrors.password?.length
                                ? "border-red-400 focus:border-red-500"
                                : "border-slate-200 focus:border-teal-500"
                            }`}
                        placeholder="••••••••"
                    />

                    <button
                        type="button"
                        onClick={() =>
                            setShowPassword((current) => !current)
                        }
                        className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 transition hover:text-slate-600"
                        aria-label={
                            showPassword
                                ? "Hide password"
                                : "Show password"
                        }
                    >
                        {showPassword ? (
                            <EyeOff className="h-4 w-4" />
                        ) : (
                            <Eye className="h-4 w-4" />
                        )}
                    </button>
                </div>

                {fieldErrors.password?.map(
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
                label="Role"
                htmlFor="create-role"
                required
            >
                <select
                    id="create-role"
                    name="role"
                    value={form.role}
                    onChange={(event) =>
                        updateField(
                            "role",
                            event.target.value as UserRole,
                        )
                    }
                    required
                    className="h-11 rounded-lg border border-slate-200 bg-white px-3 text-sm outline-none transition focus:border-teal-500 focus:ring-2 focus:ring-teal-500/20"
                >
                    <option value="Scientist">
                        Scientist
                    </option>

                    <option value="WarehouseAdmin">
                        Warehouse Admin
                    </option>
                </select>

                {fieldErrors.role?.map(
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