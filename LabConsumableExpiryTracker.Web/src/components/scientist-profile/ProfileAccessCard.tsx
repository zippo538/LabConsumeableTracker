import { UserRoundCheck } from "lucide-react";

import type { UserDto } from "../../types/user.types";

interface ProfileAccessCardProps {
    profile: UserDto;
}

export default function ProfileAccessCard({
    profile,
}: ProfileAccessCardProps) {
    return (
        <div className="h-fit rounded-xl border border-cyan-100 bg-cyan-50/50 p-6">
            <h3 className="text-sm font-semibold text-slate-900">
                Profile Access
            </h3>

            <p className="mt-2 text-sm text-slate-500">
                Role and account status are managed by the Warehouse Admin.
                You cannot change them from this profile.
            </p>

            <div className="mt-4 flex items-start gap-3 rounded-lg bg-white p-3">
                <div className="grid h-8 w-8 shrink-0 place-items-center rounded-md bg-teal-600 text-white">
                    <UserRoundCheck className="h-4 w-4" />
                </div>

                <div>
                    <p className="text-sm font-semibold text-slate-900">
                        {profile.isActive
                            ? "Active account"
                            : "Inactive account"}
                    </p>

                    <p className="mt-0.5 text-xs text-slate-500">
                        Your account status is controlled by the server.
                    </p>
                </div>
            </div>
        </div>
    );
}