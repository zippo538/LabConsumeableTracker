import type { UserDto } from "../../types/user.types";

interface UserTableProps {
    users: UserDto[];
    isLoading: boolean;
    isError: boolean;
    onManage?: (user: UserDto) => void;
}

function initials(fullName: string): string {
    return fullName
        .split(" ")
        .filter(Boolean)
        .slice(0, 2)
        .map(
            (part) =>
                part[0]?.toUpperCase() ?? "",
        )
        .join("");
}

function RoleBadge({
    role,
}: {
    role: string;
}) {
    const isAdmin = role === "WarehouseAdmin";

    return (
        <span
            className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-medium ${
                isAdmin
                    ? "bg-blue-50 text-blue-700"
                    : "bg-gray-100 text-gray-600"
            }`}
        >
            {role}
        </span>
    );
}

function StatusBadge({
    isActive,
}: {
    isActive: boolean;
}) {
    return (
        <span
            className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-medium ${
                isActive
                    ? "bg-green-50 text-green-700"
                    : "bg-red-50 text-red-700"
            }`}
        >
            <span
                className={`h-1.5 w-1.5 rounded-full ${
                    isActive
                        ? "bg-green-500"
                        : "bg-red-500"
                }`}
            />

            {isActive ? "Active" : "Inactive"}
        </span>
    );
}

export function UserTable({
    users,
    isLoading,
    isError,
    onManage,
}: UserTableProps) {
    const showEmpty =
        !isLoading &&
        !isError &&
        users.length === 0;

    return (
        <div className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm">
            <div className="overflow-x-auto">
                <table className="w-full min-w-[700px] text-left text-sm">
                    <thead>
                        <tr className="border-b border-gray-200 text-xs uppercase tracking-wide text-gray-500">
                            <th className="px-6 py-3 font-semibold">
                                User
                            </th>

                            <th className="px-6 py-3 font-semibold">
                                Role
                            </th>

                            <th className="px-6 py-3 font-semibold">
                                Status
                            </th>

                            <th className="px-6 py-3 font-semibold">
                                Action
                            </th>
                        </tr>
                    </thead>

                    <tbody>
                        {isLoading && (
                            <tr>
                                <td
                                    colSpan={4}
                                    className="px-6 py-8 text-center text-gray-400"
                                >
                                    Loading users...
                                </td>
                            </tr>
                        )}

                        {isError && (
                            <tr>
                                <td
                                    colSpan={4}
                                    className="px-6 py-8 text-center text-red-500"
                                >
                                    Failed to load users.
                                </td>
                            </tr>
                        )}

                        {showEmpty && (
                            <tr>
                                <td
                                    colSpan={4}
                                    className="px-6 py-8 text-center text-gray-400"
                                >
                                    No users match your
                                    filters.
                                </td>
                            </tr>
                        )}

                        {!isLoading &&
                            !isError &&
                            users.map((user) => (
                                <tr
                                    key={user.id}
                                    className="border-b border-gray-100 last:border-0"
                                >
                                    <td className="px-6 py-4 ">
                                        <div className="flex items-center gap-3">
                                            <div className="flex h-9 w-9 items-center justify-center rounded-full bg-teal-100 text-xs font-semibold text-teal-700">
                                                {initials(
                                                    user.fullName,
                                                )}
                                            </div>

                                            <div>
                                                <div className=" text-gray-900 font-semibold">
                                                    {
                                                        user.fullName
                                                    }
                                                </div>

                                                <div className="text-xs text-gray-500">
                                                    {
                                                        user.username
                                                    }
                                                </div>

                                                <div className="text-xs text-gray-400">
                                                    {
                                                        user.email
                                                    }
                                                </div>
                                            </div>
                                        </div>
                                    </td>

                                    <td className="px-6 py-4">
                                        <div className="flex flex-wrap gap-2">
                                            {user.roles.length >
                                            0 ? (
                                                user.roles.map(
                                                    (role) => (
                                                        <RoleBadge
                                                            key={
                                                                role
                                                            }
                                                            role={
                                                                role
                                                            }
                                                        />
                                                    ),
                                                )
                                            ) : (
                                                <span className="text-xs text-gray-400">
                                                    No role
                                                </span>
                                            )}
                                        </div>
                                    </td>

                                    <td className="px-6 py-4">
                                        <StatusBadge
                                            isActive={
                                                user.isActive
                                            }
                                        />
                                    </td>

                                    <td className="px-6 py-4">
                                        <button
                                            type="button"
                                            onClick={() =>
                                                onManage?.(
                                                    user,
                                                )
                                            }
                                            className="rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 hover:bg-gray-50"
                                        >
                                            Manage
                                        </button>
                                    </td>
                                </tr>
                            ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}