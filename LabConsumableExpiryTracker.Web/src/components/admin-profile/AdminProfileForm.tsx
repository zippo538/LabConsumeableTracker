import { Mail, UserRound } from "lucide-react";

interface AdminProfileFormProps {
    username: string;
    fullName: string;
    email: string;

    fieldErrors: {
        username?: string;
        fullName?: string;
        email?: string;
    };

    isPending: boolean;

    onUsernameChange: (value: string) => void;
    onFullNameChange: (value: string) => void;
    onEmailChange: (value: string) => void;

    onSave: () => void;
    isSaveDisabled: boolean;
}

interface ProfileInputProps {
    label: string;
    value: string;
    type?: string;
    error?: string;
    disabled: boolean;
    icon: React.ReactNode;
    onChange: (value: string) => void;
}

function ProfileInput({
    label,
    value,
    type = "text",
    error,
    disabled,
    icon,
    onChange,
}: ProfileInputProps) {
    return (
        <div>
            <label className="mb-2 block text-xs font-semibold text-slate-700">
                {label}
            </label>

            <div className="relative">
                {icon}

                <input
                    type={type}
                    value={value}
                    onChange={(event) =>
                        onChange(event.target.value)
                    }
                    disabled={disabled}
                    className={`h-11 w-full rounded-lg border bg-white pl-10 pr-3 text-sm text-slate-700 outline-none transition placeholder:text-slate-400 disabled:cursor-not-allowed disabled:bg-slate-50 ${
                        error
                            ? "border-red-300 focus:border-red-400 focus:ring-2 focus:ring-red-100"
                            : "border-slate-200 focus:border-teal-400 focus:ring-2 focus:ring-teal-100"
                    }`}
                />
            </div>

            {error && (
                <p className="mt-1.5 text-xs text-red-600">
                    {error}
                </p>
            )}
        </div>
    );
}

export function AdminProfileForm({
    username,
    fullName,
    email,
    fieldErrors,
    isPending,
    onUsernameChange,
    onFullNameChange,
    onEmailChange,
    onSave,
    isSaveDisabled,
}: AdminProfileFormProps) {
    return (
        <div className="mt-6 border-t border-slate-100 pt-6">
            <div className="mb-5">
                <h2 className="text-sm font-semibold text-slate-900">
                    Profile Information
                </h2>

                <p className="mt-1 text-xs text-slate-500">
                    Update the information associated with
                    your account.
                </p>
            </div>

            <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                <ProfileInput
                    label="Username"
                    value={username}
                    error={fieldErrors.username}
                    disabled={isPending}
                    onChange={onUsernameChange}
                    icon={
                        <UserRound className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                    }
                />

                <ProfileInput
                    label="Full Name"
                    value={fullName}
                    error={fieldErrors.fullName}
                    disabled={isPending}
                    onChange={onFullNameChange}
                    icon={
                        <UserRound className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                    }
                />

                <div className="md:col-span-2">
                    <ProfileInput
                        label="Email"
                        type="email"
                        value={email}
                        error={fieldErrors.email}
                        disabled={isPending}
                        onChange={onEmailChange}
                        icon={
                            <Mail className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                        }
                    />
                </div>
            </div>

            <div className="mt-6 flex justify-end">
                <button
                    type="button"
                    onClick={onSave}
                    disabled={isSaveDisabled}
                    className="rounded-lg bg-[#13a9a7] px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-[#0f9694] disabled:cursor-not-allowed disabled:opacity-50"
                >
                    {isPending ? "Saving..." : "Save Changes"}
                </button>
            </div>
        </div>
    );
}