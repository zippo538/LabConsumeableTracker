import { useState } from "react";
import { useNavigate, useParams } from "react-router";
import {
    ArrowLeft,
    CheckCircle2,
    Edit,
    LockKeyhole,
    Mail,
    ShieldCheck,
    User,
    UserRoundX,
} from "lucide-react";

import { AppLayout } from "../components/layout/AppLayout";
import { useActivateUser, useDeactivateUser, useUser } from "../hooks/useUser";
import { EditUserModal } from "../components/manage-users/EditUserModal";
import { ChangePasswordModal } from "../components/manage-users/ChangePasswordModal";

export default function UserDetailPage() {
    const { id } = useParams();
    const navigate = useNavigate();

    const { data, isLoading, isError } = useUser(id);
    const activateUserMutation = useActivateUser();
    const deactivateUserMutation = useDeactivateUser();

    const [isEditModalOpen, setIsEditModalOpen] = useState(false);

    const [successMessage, setSuccessMessage] = useState("");


    const [statusError, setStatusError] =
        useState("");
    const isStatusUpdating =
        activateUserMutation.isPending ||
        deactivateUserMutation.isPending;

    const [isChangePasswordModalOpen, setIsChangePasswordModalOpen] =
        useState(false);

    const user = data?.data;

    const handleBack = () => {
        navigate("/app/manage-users");
    };

    const handleToggleStatus = async () => {
        if (!user) return;

        setSuccessMessage("");
        setStatusError("");

        try {
            const response = user.isActive
                ? await deactivateUserMutation.mutateAsync(user.id)
                : await activateUserMutation.mutateAsync(user.id);

            setSuccessMessage(
                response.message ||
                (user.isActive
                    ? "User deactivated successfully."
                    : "User activated successfully.")
            );
        } catch (error: any) {
            const message =
                error?.response?.data?.message ??
                error?.response?.data?.title ??
                "Failed to update user status.";

            setStatusError(message);
        }
    };

    if (isLoading) {
        return (
            <AppLayout
                title="User Detail"
                description="View user account details."
            >
                <div className="space-y-6">
                    <div>
                        <button
                            type="button"
                            onClick={handleBack}
                            className="inline-flex items-center gap-2 text-sm font-medium text-slate-500 transition hover:text-teal-600"
                        >
                            <ArrowLeft className="h-4 w-4" />
                            Back to users
                        </button>
                    </div>

                    <div className="animate-pulse rounded-xl border border-slate-200 bg-white p-6">
                        <div className="flex items-center gap-4">
                            <div className="h-16 w-16 rounded-full bg-slate-200" />

                            <div className="space-y-2">
                                <div className="h-5 w-40 rounded bg-slate-200" />
                                <div className="h-4 w-28 rounded bg-slate-200" />
                            </div>
                        </div>

                        <div className="mt-8 grid grid-cols-1 gap-4 md:grid-cols-2">
                            <div className="h-20 rounded-lg bg-slate-100" />
                            <div className="h-20 rounded-lg bg-slate-100" />
                            <div className="h-20 rounded-lg bg-slate-100" />
                            <div className="h-20 rounded-lg bg-slate-100" />
                        </div>
                    </div>
                </div>
            </AppLayout>
        );
    }

    if (isError || !user) {
        return (
            <AppLayout
                title="User Detail"
                description="View user account details."
            >
                <div className="space-y-6">
                    <button
                        type="button"
                        onClick={handleBack}
                        className="inline-flex items-center gap-2 text-sm font-medium text-slate-500 transition hover:text-teal-600"
                    >
                        <ArrowLeft className="h-4 w-4" />
                        Back to users
                    </button>

                    <div className="rounded-xl border border-red-200 bg-red-50 p-6">
                        <p className="text-sm font-semibold text-red-700">
                            Failed to load user
                        </p>

                        <p className="mt-1 text-sm text-red-600">
                            The user could not be found or an error
                            occurred while loading the data.
                        </p>
                    </div>
                </div>
            </AppLayout>
        );
    }

    const initials = user.fullName
        .trim()
        .split(/\s+/)
        .slice(0, 2)
        .map((part) => part[0]?.toUpperCase())
        .join("");

    return (
        <AppLayout
            title="User Detail"
            description="View and manage user account details."
        >
            <div className="space-y-6">
                {/* Header */}
                <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                    <div>
                        <button
                            type="button"
                            onClick={handleBack}
                            className="mb-3 inline-flex items-center gap-2 text-sm font-medium text-slate-500 transition hover:text-teal-600"
                        >
                            <ArrowLeft className="h-4 w-4" />
                            Back to users
                        </button>

                        <h1 className="text-2xl font-bold text-gray-900">
                            User detail
                        </h1>

                        <p className="mt-1 text-sm text-gray-500">
                            View account information and manage user access.
                        </p>
                    </div>



                    <div className="flex items-center gap-2">
                        <button
                            type="button"
                            onClick={() => {
                                setSuccessMessage("");
                                setIsChangePasswordModalOpen(true);
                            }}
                            className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 transition hover:border-teal-300 hover:text-teal-700"
                        >
                            <LockKeyhole className="h-4 w-4" />
                            Change Password
                        </button>
                        <button
                            type="button"
                            onClick={() => {
                                setSuccessMessage("");
                                setIsEditModalOpen(true);
                            }}
                            className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 transition hover:border-teal-300 hover:text-teal-700"
                        >
                            <Edit className="h-4 w-4" />
                            Edit
                        </button>

                        <button
                            type="button"
                            onClick={handleToggleStatus}
                            disabled={isStatusUpdating}
                            className={`inline-flex items-center gap-2 rounded-lg px-4 py-2.5 text-sm font-semibold transition disabled:cursor-not-allowed disabled:opacity-50 ${user.isActive
                                ? "border border-red-200 bg-red-50 text-red-700 hover:bg-red-100"
                                : "bg-emerald-600 text-white hover:bg-emerald-700"
                                }`}
                        >
                            {user.isActive ? (
                                <>
                                    <UserRoundX className="h-4 w-4" />
                                    {deactivateUserMutation.isPending
                                        ? "Deactivating..."
                                        : "Deactivate"}
                                </>
                            ) : (
                                <>
                                    <CheckCircle2 className="h-4 w-4" />
                                    {activateUserMutation.isPending
                                        ? "Activating..."
                                        : "Activate"}
                                </>
                            )}
                        </button>
                    </div>
                </div>
                {successMessage && (
                    <div className="rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
                        {successMessage}
                    </div>
                )}
                {statusError && (
                    <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                        {statusError}
                    </div>
                )}

                {/* Profile Card */}
                <div className="rounded-xl border border-slate-200 bg-white p-6">
                    <div className="flex flex-col gap-5 sm:flex-row sm:items-center">
                        <div className="grid h-16 w-16 shrink-0 place-items-center rounded-full bg-teal-100 text-lg font-semibold text-teal-700">
                            {initials || (
                                <User className="h-7 w-7" />
                            )}
                        </div>

                        <div className="min-w-0">
                            <h2 className="text-xl font-bold text-slate-900">
                                {user.fullName}
                            </h2>

                            <p className="mt-1 text-sm text-slate-500">
                                @{user.username}
                            </p>
                        </div>

                        <div className="sm:ml-auto">
                            <span
                                className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1.5 text-xs font-semibold ${user.isActive
                                    ? "bg-emerald-50 text-emerald-700"
                                    : "bg-red-50 text-red-700"
                                    }`}
                            >
                                <span
                                    className={`h-1.5 w-1.5 rounded-full ${user.isActive
                                        ? "bg-emerald-500"
                                        : "bg-red-500"
                                        }`}
                                />

                                {user.isActive
                                    ? "Active account"
                                    : "Inactive account"}
                            </span>
                        </div>
                    </div>
                </div>

                {/* Account Information */}
                <div className="rounded-xl border border-slate-200 bg-white p-6">
                    <div className="mb-6">
                        <h2 className="text-base font-semibold text-slate-900">
                            Account information
                        </h2>

                        <p className="mt-1 text-sm text-slate-500">
                            Basic information associated with this account.
                        </p>
                    </div>

                    <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                        <div className="rounded-lg border border-slate-100 bg-slate-50/70 p-4">
                            <div className="flex items-center gap-3">
                                <div className="grid h-9 w-9 place-items-center rounded-lg bg-white text-slate-500">
                                    <User className="h-4 w-4" />
                                </div>

                                <div>
                                    <p className="text-xs font-medium text-slate-400">
                                        Username
                                    </p>

                                    <p className="mt-0.5 text-sm font-semibold text-slate-800">
                                        {user.username}
                                    </p>
                                </div>
                            </div>
                        </div>

                        <div className="rounded-lg border border-slate-100 bg-slate-50/70 p-4">
                            <div className="flex items-center gap-3">
                                <div className="grid h-9 w-9 place-items-center rounded-lg bg-white text-slate-500">
                                    <Mail className="h-4 w-4" />
                                </div>

                                <div className="min-w-0">
                                    <p className="text-xs font-medium text-slate-400">
                                        Email
                                    </p>

                                    <p className="mt-0.5 break-all text-sm font-semibold text-slate-800">
                                        {user.email}
                                    </p>
                                </div>
                            </div>
                        </div>

                        <div className="rounded-lg border border-slate-100 bg-slate-50/70 p-4">
                            <div className="flex items-center gap-3">
                                <div className="grid h-9 w-9 place-items-center rounded-lg bg-white text-slate-500">
                                    <ShieldCheck className="h-4 w-4" />
                                </div>

                                <div>
                                    <p className="text-xs font-medium text-slate-400">
                                        Role
                                    </p>

                                    <div className="mt-1 flex flex-wrap gap-2">
                                        {user.roles.map((role) => (
                                            <span
                                                key={role}
                                                className="rounded-full bg-teal-50 px-2.5 py-1 text-xs font-semibold text-teal-700"
                                            >
                                                {role}
                                            </span>
                                        ))}
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="rounded-lg border border-slate-100 bg-slate-50/70 p-4">
                            <div className="flex items-center gap-3">
                                <div
                                    className={`grid h-9 w-9 place-items-center rounded-lg bg-white ${user.isActive
                                        ? "text-emerald-600"
                                        : "text-red-600"
                                        }`}
                                >
                                    {user.isActive ? (
                                        <CheckCircle2 className="h-4 w-4" />
                                    ) : (
                                        <UserRoundX className="h-4 w-4" />
                                    )}
                                </div>

                                <div>
                                    <p className="text-xs font-medium text-slate-400">
                                        Account status
                                    </p>

                                    <p
                                        className={`mt-0.5 text-sm font-semibold ${user.isActive
                                            ? "text-emerald-700"
                                            : "text-red-700"
                                            }`}
                                    >
                                        {user.isActive
                                            ? "Active"
                                            : "Inactive"}
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Access Information */}
                <div className="rounded-xl border border-cyan-100 bg-cyan-50/50 p-6">
                    <div className="flex items-start gap-3">
                        <div className="grid h-9 w-9 shrink-0 place-items-center rounded-lg bg-teal-600 text-white">
                            <ShieldCheck className="h-4 w-4" />
                        </div>

                        <div>
                            <h3 className="text-sm font-semibold text-slate-900">
                                Access management
                            </h3>

                            <p className="mt-1 text-sm leading-6 text-slate-500">
                                User roles and account status determine
                                which areas of the application this user
                                can access. Changes to these settings
                                should be made carefully.
                            </p>
                        </div>
                    </div>
                </div>

            </div>
            <EditUserModal
                isOpen={isEditModalOpen}
                user={user}
                onClose={() =>
                    setIsEditModalOpen(false)
                }
                onSuccess={(message) => {
                    setSuccessMessage(message);
                }}
            />
            <ChangePasswordModal
                isOpen={isChangePasswordModalOpen}
                userId={user.id}
                onClose={() =>
                    setIsChangePasswordModalOpen(false)
                }
                onSuccess={(message) => {
                    setSuccessMessage(message);
                }}
            />
        </AppLayout>
    );
}