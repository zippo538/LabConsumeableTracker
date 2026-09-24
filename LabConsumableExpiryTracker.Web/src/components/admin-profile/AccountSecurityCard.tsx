import {
    LockKeyhole,
    ShieldCheck,
} from "lucide-react";

interface AccountSecurityCardProps {
    onChangePassword: () => void;
}

export function AccountSecurityCard({
    onChangePassword,
}: AccountSecurityCardProps) {
    return (
        <div className="rounded-xl border border-slate-200 bg-white p-6">
            <div className="flex items-start justify-between gap-4">
                <div className="flex items-start gap-3">
                    <div className="grid h-10 w-10 shrink-0 place-items-center rounded-lg bg-teal-50 text-teal-700">
                        <LockKeyhole className="h-5 w-5" />
                    </div>

                    <div>
                        <h2 className="text-sm font-semibold text-slate-900">
                            Account Security
                        </h2>

                        <p className="mt-1 text-xs leading-5 text-slate-500">
                            Change your account password to
                            keep your account secure.
                        </p>
                    </div>
                </div>

                <ShieldCheck className="h-5 w-5 shrink-0 text-slate-300" />
            </div>

            <div className="mt-5 flex items-center justify-between rounded-lg border border-slate-100 bg-slate-50 px-4 py-3">
                <div>
                    <p className="text-sm font-medium text-slate-700">
                        Password
                    </p>

                    <p className="mt-0.5 text-xs text-slate-500">
                        Set a new password for your account.
                    </p>
                </div>

                <button
                    type="button"
                    onClick={onChangePassword}
                    className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-xs font-semibold text-slate-700 transition hover:border-teal-300 hover:text-teal-700"
                >
                    Change Password
                </button>
            </div>
        </div>
    );
}