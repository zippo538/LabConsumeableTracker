import type { UserStatisticsDto } from "../../types/user.types";

interface UserStatisticsProps {
    statistics?: UserStatisticsDto;
}

interface StatCardProps {
    value: number;
    label: string;
    accentClassName: string;
}

function StatCard({
    value,
    label,
    accentClassName,
}: StatCardProps) {
    return (
        <div className="flex items-center gap-3 rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
            <span
                className={`h-10 w-1 rounded-full ${accentClassName}`}
            />

            <div>
                <div className="text-2xl font-bold text-gray-900">
                    {value}
                </div>

                <div className="text-sm text-gray-500">
                    {label}
                </div>
            </div>
        </div>
    );
}

export function UserStatistics({
    statistics,
}: UserStatisticsProps) {
    return (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <StatCard
                value={statistics?.totalCount ?? 0}
                label="Total users"
                accentClassName="bg-blue-500"
            />
 <StatCard
                value={statistics?.activeAdminCount ?? 0}
                label="Active admins"
                accentClassName="bg-teal-500"
            />
            <StatCard
                value={statistics?.activeCount ?? 0}
                label="Active accounts"
                accentClassName="bg-green-500"
            />

           

            <StatCard
                value={statistics?.deactivatedCount ?? 0}
                label="Deactivated accounts"
                accentClassName="bg-red-500"
            />
        </div>
    );
}