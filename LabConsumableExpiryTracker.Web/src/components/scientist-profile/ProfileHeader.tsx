import { User } from "lucide-react";

import type { UserDto } from "../../types/user.types";

interface ProfileHeaderProps {
    profile: UserDto;
}

function getInitials(name: string): string {
    if (!name) return "";

    return name
        .trim()
        .split(/\s+/)
        .slice(0, 2)
        .map((part) => part[0]?.toUpperCase())
        .join("");
}

export default function ProfileHeader({
    profile,
}: ProfileHeaderProps) {
    return (
        <div className="flex flex-col gap-4 border-b border-slate-100 pb-6 sm:flex-row sm:items-center">
            <div className="grid h-16 w-16 shrink-0 place-items-center rounded-full bg-[#DBF7F7] text-lg font-semibold text-[#0EA5A5]">
                {getInitials(profile.fullName) || (
                    <User className="h-7 w-7" />
                )}
            </div>

            <div>
                <h2 className="text-xl font-bold text-slate-900">
                    {profile.fullName}
                </h2>

                <p className="mt-1 text-sm text-slate-500">
                    @{profile.username}
                </p>
            </div>

            <div className="sm:ml-auto">
                <span
                    className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1.5 text-xs font-semibold ${
                        profile.isActive
                            ? "bg-emerald-50 text-emerald-700"
                            : "bg-red-50 text-red-700"
                    }`}
                >
                    <span
                        className={`h-1.5 w-1.5 rounded-full ${
                            profile.isActive
                                ? "bg-emerald-500"
                                : "bg-red-500"
                        }`}
                    />

                    {profile.isActive
                        ? "Active account"
                        : "Inactive account"}
                </span>
            </div>
        </div>
    );
}