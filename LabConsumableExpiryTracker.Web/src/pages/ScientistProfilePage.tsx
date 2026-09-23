import { useEffect, useState } from "react";

import { AppLayout } from "../components/layout/AppLayout";
import ProfileAccessCard from "../components/scientist-profile/ProfileAccessCard";
import ProfileForm from "../components/scientist-profile/ProfileForm";
import ProfileHeader from "../components/scientist-profile/ProfileHeader";
import {
    useMyProfile,
    useUpdateMyProfile,
} from "../hooks/useUser";

export default function ScientistProfilePage() {
    const { data, isLoading, isError } = useMyProfile();
    const profile = data?.data;

    const updateProfile = useUpdateMyProfile();

    const [fullName, setFullName] = useState("");

    useEffect(() => {
        if (profile?.fullName) {
            setFullName(profile.fullName);
        }
    }, [profile?.fullName]);

    const handleSave = () => {
        const trimmed = fullName.trim();

        if (!trimmed || trimmed === profile?.fullName) {
            return;
        }

        updateProfile.mutate({
            fullName: trimmed,
        });
    };

    const handleFullNameChange = (value: string) => {
        setFullName(value);

        if (
            updateProfile.isSuccess ||
            updateProfile.isError
        ) {
            updateProfile.reset();
        }
    };

    const isUnchanged =
        fullName.trim() === profile?.fullName;

    const isSaveDisabled =
        !fullName.trim() ||
        isUnchanged ||
        updateProfile.isPending;

    if (isLoading) {
        return (
            <AppLayout
                title="Profile"
                description="View your account details. Only Full name can be updated here."
            >
                <div className="space-y-6">
                    <div>
                        <h1 className="text-[26px] font-bold tracking-tight text-slate-900">
                            My Profile
                        </h1>

                        <p className="mt-1 text-sm text-slate-500">
                            View your account details. Only Full name can be
                            updated here.
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
                description="View your account details. Only Full name can be updated here."
            >
                <div className="space-y-6">
                    <div>
                        <h1 className="text-[26px] font-bold tracking-tight text-slate-900">
                            My Profile
                        </h1>

                        <p className="mt-1 text-sm text-slate-500">
                            View your account details. Only Full name can be
                            updated here.
                        </p>
                    </div>

                    <div className="rounded-xl border border-red-200 bg-red-50 p-6">
                        <p className="text-sm font-semibold text-red-700">
                            Failed to load profile
                        </p>

                        <p className="mt-1 text-sm text-red-600">
                            Please check your connection and try again.
                        </p>
                    </div>
                </div>
            </AppLayout>
        );
    }

    return (
        <AppLayout
            title="Profile"
            description="View your account details. Only Full name can be updated here."
        >
            <div className="space-y-6">
                <div>
                    <h1 className="text-[26px] font-bold tracking-tight text-slate-900">
                        My Profile
                    </h1>

                    <p className="mt-1 text-sm text-slate-500">
                        Review your account information and update your full
                        name.
                    </p>
                </div>

                <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
                    <div className="rounded-xl border border-slate-200 bg-white p-6 lg:col-span-2">
                        <ProfileHeader profile={profile} />

                        <ProfileForm
                            profile={profile}
                            fullName={fullName}
                            isPending={updateProfile.isPending}
                            isError={updateProfile.isError}
                            isSuccess={updateProfile.isSuccess}
                            isSaveDisabled={isSaveDisabled}
                            onFullNameChange={handleFullNameChange}
                            onSave={handleSave}
                            onResetStatus={updateProfile.reset}
                        />
                    </div>

                    <ProfileAccessCard profile={profile} />
                </div>
            </div>
        </AppLayout>
    );
}