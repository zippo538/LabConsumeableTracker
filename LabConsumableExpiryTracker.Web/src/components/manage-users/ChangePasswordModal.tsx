import { useEffect, useState } from "react";
import { Eye, EyeOff, LockKeyhole } from "lucide-react";

import { FormModal,ModalField } from "../Modal";
import { useChangeUserPassword } from "../../hooks/useUser";

interface ChangePasswordModalProps {
    isOpen: boolean;
    userId: string;
    onClose: () => void;
    onSuccess: (message: string) => void;
}

export function ChangePasswordModal({
    isOpen,
    userId,
    onClose,
    onSuccess,
}: ChangePasswordModalProps) {
    const changePasswordMutation =
        useChangeUserPassword();

    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] =
        useState("");

    const [showPassword, setShowPassword] =
        useState(false);

    const [showConfirmPassword, setShowConfirmPassword] =
        useState(false);

    const [error, setError] = useState("");

    useEffect(() => {
        if (!isOpen) return;

        setNewPassword("");
        setConfirmPassword("");
        setError("");
        setShowPassword(false);
        setShowConfirmPassword(false);
    }, [isOpen]);

    const handleClose = () => {
        if (changePasswordMutation.isPending) return;

        setNewPassword("");
        setConfirmPassword("");
        setError("");
        setShowPassword(false);
        setShowConfirmPassword(false);

        onClose();
    };

  const handleSubmit = async (
    event: React.FormEvent<HTMLFormElement>,
) => {
    event.preventDefault();

    setError("");

    if (!newPassword.trim()) {
        setError("New password is required.");
        return;
    }

    if (!confirmPassword.trim()) {
        setError("Please confirm the new password.");
        return;
    }

    if (newPassword !== confirmPassword) {
        setError("Passwords do not match.");
        return;
    }

    try {
        const response =
            await changePasswordMutation.mutateAsync({
                id: userId,
                payload: {
                    newPassword,
                },
            });

        if (!response.success) {
            setError(
                response.message ||
                    "Failed to change password.",
            );
            return;
        }

        onSuccess(
            response.message ||
                "User password changed successfully.",
        );

        onClose();
    } catch (error: any) {
        const data = error?.response?.data;

        const backendErrors = data?.errors;

        if (
            Array.isArray(backendErrors) &&
            backendErrors.length > 0
        ) {
            setError(backendErrors.join(" "));
            return;
        }

        setError(
            data?.message ??
                data?.title ??
                "Failed to change password.",
        );
    }
};

    return (
        <FormModal
            isOpen={isOpen}
            onClose={handleClose}
            title="Change Password"
            description="Set a new password for this user account."
            mode="update"
            submitLabel="Change Password"
            onSubmit={handleSubmit}
            busy={changePasswordMutation.isPending}
            error={error}
        >
            <div className="space-y-5">
                <div className="flex items-center gap-3 rounded-lg border border-slate-200 bg-slate-50 px-4 py-3">
                    <div className="grid h-9 w-9 place-items-center rounded-lg bg-teal-100 text-teal-700">
                        <LockKeyhole className="h-4 w-4" />
                    </div>

                    <div>
                        <p className="text-sm font-semibold text-slate-800">
                            Account Password
                        </p>

                        <p className="text-xs text-slate-500">
                            The current password will be replaced.
                        </p>
                    </div>
                </div>

                <ModalField
                    label="New Password"
                    required
                >
                    <div className="relative">
                        <input
                            type={
                                showPassword
                                    ? "text"
                                    : "password"
                            }
                            value={newPassword}
                            onChange={(event) =>
                                setNewPassword(
                                    event.target.value,
                                )
                            }
                            placeholder="Enter new password"
                            className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 pr-10 text-sm text-slate-700 outline-none transition placeholder:text-slate-400 focus:border-teal-400 focus:ring-2 focus:ring-teal-100"
                        />

                        <button
                            type="button"
                            onClick={() =>
                                setShowPassword(
                                    (current) => !current,
                                )
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
                </ModalField>

                <ModalField
                    label="Confirm New Password"
                    required
                >
                    <div className="relative">
                        <input
                            type={
                                showConfirmPassword
                                    ? "text"
                                    : "password"
                            }
                            value={confirmPassword}
                            onChange={(event) =>
                                setConfirmPassword(
                                    event.target.value,
                                )
                            }
                            placeholder="Confirm new password"
                            className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 pr-10 text-sm text-slate-700 outline-none transition placeholder:text-slate-400 focus:border-teal-400 focus:ring-2 focus:ring-teal-100"
                        />

                        <button
                            type="button"
                            onClick={() =>
                                setShowConfirmPassword(
                                    (current) => !current,
                                )
                            }
                            className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 transition hover:text-slate-600"
                            aria-label={
                                showConfirmPassword
                                    ? "Hide password"
                                    : "Show password"
                            }
                        >
                            {showConfirmPassword ? (
                                <EyeOff className="h-4 w-4" />
                            ) : (
                                <Eye className="h-4 w-4" />
                            )}
                        </button>
                    </div>
                </ModalField>

                <div className="rounded-lg border border-slate-200 bg-slate-50 px-4 py-3">
                    <p className="text-xs leading-5 text-slate-500">
                        Password requirements are validated by
                        the server.
                    </p>
                </div>
            </div>
        </FormModal>
    );
}