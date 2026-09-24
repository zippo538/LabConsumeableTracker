import { useEffect, useState } from "react";

import { AppLayout } from "../components/layout/AppLayout";
import ProfileHeader from "../components/scientist-profile/ProfileHeader";

import { ChangePasswordModal } from "../components/manage-users/ChangePasswordModal";
import { AdminProfileForm } from "../components/admin-profile/AdminProfileForm";
import { AccountSecurityCard } from "../components/admin-profile/AccountSecurityCard";

import {
    useMyProfile,
    useUpdateUser,
} from "../hooks/useUser";

interface FieldErrors {
    username?: string;
    fullName?: string;
    email?: string;
}

export default function AdminProfilePage() {
    const { data, isLoading, isError } = useMyProfile();
    const profile = data?.data;

    const updateUser = useUpdateUser();

    const [username, setUsername] = useState("");
    const [fullName, setFullName] = useState("");
    const [email, setEmail] = useState("");

    const [fieldErrors, setFieldErrors] =
        useState<FieldErrors>({});

    const [isPasswordModalOpen, setIsPasswordModalOpen] =
        useState(false);

    const [successMessage, setSuccessMessage] = useState("");
    const [errorMessage, setErrorMessage] = useState("");

    useEffect(() => {
        if (!profile) return;

        setUsername(profile.username);
        setFullName(profile.fullName);
        setEmail(profile.email);
        setFieldErrors({});
    }, [profile]);

    const clearMessages = () => {
        setSuccessMessage("");
        setErrorMessage("");
    };

    const clearFieldError = (
        field: keyof FieldErrors,
    ) => {
        setFieldErrors((current) => ({
            ...current,
            [field]: undefined,
        }));
    };

    const handleUsernameChange = (value: string) => {
        setUsername(value);
        clearFieldError("username");
        clearMessages();
    };

    const handleFullNameChange = (value: string) => {
        setFullName(value);
        clearFieldError("fullName");
        clearMessages();
    };

    const handleEmailChange = (value: string) => {
        setEmail(value);
        clearFieldError("email");
        clearMessages();
    };

    const handleSave = async () => {
        if (!profile) return;

        const payload = {
            username: username.trim(),
            fullName: fullName.trim(),
            email: email.trim(),
        };

        setFieldErrors({});
        clearMessages();

        if (
            !payload.username ||
            !payload.fullName ||
            !payload.email
        ) {
            setErrorMessage(
                "Username, full name, and email are required.",
            );
            return;
        }

        const isUnchanged =
            payload.username === profile.username &&
            payload.fullName === profile.fullName &&
            payload.email === profile.email;

        if (isUnchanged) {
            return;
        }

        try {
            const response =
                await updateUser.mutateAsync({
                    id: profile.id,
                    payload,
                });

            setSuccessMessage(
                response.message ||
                    "Profile updated successfully.",
            );
        } catch (error: any) {
            const data = error?.response?.data;
            const backendErrors = data?.errors;

            if (backendErrors) {
                setFieldErrors({
                    username:
                        backendErrors.Username?.[0],
                    fullName:
                        backendErrors.FullName?.[0],
                    email:
                        backendErrors.Email?.[0],
                });

                return;
            }

            setErrorMessage(
                data?.message ??
                    data?.title ??
                    "Failed to update profile.",
            );
        }
    };

    const handleChangePassword = () => {
        clearMessages();
        setIsPasswordModalOpen(true);
    };

    const isUnchanged =
        username.trim() === profile?.username &&
        fullName.trim() === profile?.fullName &&
        email.trim() === profile?.email;

    const isSaveDisabled =
        !username.trim() ||
        !fullName.trim() ||
        !email.trim() ||
        isUnchanged ||
        updateUser.isPending;

    if (isLoading) {
        return (
            <AppLayout
                title="Profile"
                description="Manage your account information and security."
            >
                <div className="space-y-6">
                    <div>
                        <h1 className="text-[26px] font-bold tracking-tight text-slate-900">
                            My Profile
                        </h1>

                        <p className="mt-1 text-sm text-slate-500">
                            Manage your account information and
                            security.
                        </p>
                    </div>

                    <div className="rounded-xl border border-slate-200 bg-white p-6">
                        <div className="animate-pulse space-y-6">
                            <div className="flex items-center gap-4">
                                <div className="h-16 w-16 rounded-full bg-slate-200" />

                                <div className="space-y-2">
                                    <div className="h-5 w-40 rounded bg-slate-200" />
                                    <div className="h-4 w-28 rounded bg-slate-200" />
                                </div>
                            </div>

                            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                                <div className="h-20 rounded-lg bg-slate-100" />
                                <div className="h-20 rounded-lg bg-slate-100" />
                                <div className="h-20 rounded-lg bg-slate-100" />
                            </div>
                        </div>
                    </div>
                </div>
            </AppLayout>
        );
    }

    if (isError || !profile) {
        return (
            <AppLayout
                title="Profile"
                description="Manage your account information and security."
            >
                <div className="space-y-6">
                    <div>
                        <h1 className="text-[26px] font-bold tracking-tight text-slate-900">
                            My Profile
                        </h1>

                        <p className="mt-1 text-sm text-slate-500">
                            Manage your account information and
                            security.
                        </p>
                    </div>

                    <div className="rounded-xl border border-red-200 bg-red-50 p-6">
                        <p className="text-sm font-semibold text-red-700">
                            Failed to load profile
                        </p>

                        <p className="mt-1 text-sm text-red-600">
                            Please check your connection and try
                            again.
                        </p>
                    </div>
                </div>
            </AppLayout>
        );
    }

    return (
        <AppLayout
            title="Profile"
            description="Manage your account information and security."
        >
            <div className="space-y-6">
                <div>
                    <h1 className="text-[26px] font-bold tracking-tight text-slate-900">
                        My Profile
                    </h1>

                    <p className="mt-1 text-sm text-slate-500">
                        Manage your account information and security.
                    </p>
                </div>

                {successMessage && (
                    <div className="rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
                        {successMessage}
                    </div>
                )}

                {errorMessage && (
                    <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                        {errorMessage}
                    </div>
                )}

                <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
                    <div className="space-y-6 lg:col-span-2">
                        <div className="rounded-xl border border-slate-200 bg-white p-6">
                            <ProfileHeader profile={profile} />

                            <AdminProfileForm
                                username={username}
                                fullName={fullName}
                                email={email}
                                fieldErrors={fieldErrors}
                                isPending={
                                    updateUser.isPending
                                }
                                onUsernameChange={
                                    handleUsernameChange
                                }
                                onFullNameChange={
                                    handleFullNameChange
                                }
                                onEmailChange={
                                    handleEmailChange
                                }
                                onSave={handleSave}
                                isSaveDisabled={
                                    isSaveDisabled
                                }
                            />
                        </div>

                        <AccountSecurityCard
                            onChangePassword={
                                handleChangePassword
                            }
                        />
                    </div>

                   
                </div>

                <ChangePasswordModal
                    isOpen={isPasswordModalOpen}
                    userId={profile.id}
                    onClose={() =>
                        setIsPasswordModalOpen(false)
                    }
                    onSuccess={(message) => {
                        setSuccessMessage(message);
                        setErrorMessage("");
                    }}
                />
            </div>
        </AppLayout>
    );
}