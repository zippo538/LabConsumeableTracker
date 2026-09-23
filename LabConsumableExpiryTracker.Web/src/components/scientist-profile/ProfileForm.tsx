import { Mail, User } from "lucide-react";

import type { UserProfileDto } from "../../types/user.types";

interface ProfileFormProps {
    profile: UserProfileDto;
    fullName: string;
    isPending: boolean;
    isError: boolean;
    isSuccess: boolean;
    isSaveDisabled: boolean;
    onFullNameChange: (value: string) => void;
    onSave: () => void;
    onResetStatus: () => void;
}

export default function ProfileForm({
    profile,
    fullName,
    isPending,
    isError,
    isSuccess,
    isSaveDisabled,
    onFullNameChange,
    onSave,
    onResetStatus,
}: ProfileFormProps) {
    const handleFullNameChange = (value: string) => {
        onFullNameChange(value);

        if (isSuccess || isError) {
            onResetStatus();
        }
    };

    return (
        <div className="space-y-4 pt-6">
            {isError && (
                <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                    Failed to save full name. Please try again.
                </div>
            )}

            {isSuccess && (
                <div className="rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
                    Full name updated successfully.
                </div>
            )}
            <div>
                <label
                    htmlFor="fullName"
                    className="text-xs font-medium text-slate-500"
                >
                    Full name
                </label>

                <input
                    id="fullName"
                    type="text"
                    value={fullName}
                    onChange={(event) =>
                        handleFullNameChange(event.target.value)
                    }
                    className="mt-1.5 w-full rounded-lg border border-slate-200 px-3 py-2.5 text-sm text-slate-900 focus:border-teal-500 focus:outline-none focus:ring-1 focus:ring-teal-500"
                    placeholder="Enter your full name"
                />
            </div>

            <div>
                <div className="flex items-center justify-between">
                    <label className="text-xs font-medium text-slate-500">
                        Username
                    </label>

                    <span className="text-xs text-slate-400">
                        Read only
                    </span>
                </div>

                <div className="mt-1.5 flex items-center gap-2 rounded-lg border border-slate-100 bg-slate-50 px-3 py-2.5">
                    <User className="h-4 w-4 shrink-0 text-slate-400" />

                    <span className="text-sm text-slate-500">
                        {profile.username}
                    </span>
                </div>
            </div>
            <div>
                <div className="flex items-center justify-between">
                    <label className="text-xs font-medium text-slate-500">
                        Email
                    </label>

                    <span className="text-xs text-slate-400">
                        Read only
                    </span>
                </div>

                <div className="mt-1.5 flex items-center gap-2 rounded-lg border border-slate-100 bg-slate-50 px-3 py-2.5">
                    <Mail className="h-4 w-4 shrink-0 text-slate-400" />

                    <span className="break-all text-sm text-slate-500">
                        {profile.email}
                    </span>
                </div>
            </div>

            <div>
                <label className="text-xs font-medium text-slate-500">
                    Role
                </label>

                <div className="mt-1.5 flex flex-wrap gap-2">
                    {profile.roles.map((role) => (
                        <span
                            key={role}
                            className="rounded-full bg-teal-50 px-2.5 py-1 text-xs font-semibold text-teal-700"
                        >
                            {role}
                        </span>
                    ))}
                </div>
            </div>


            <div className="flex justify-end pt-2">
                <button
                    type="button"
                    onClick={onSave}
                    disabled={isSaveDisabled}
                    className="rounded-lg bg-emerald-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-50"
                >
                    {isPending ? "Saving..." : "Save Full Name"}
                </button>
            </div>
        </div>
    );
}