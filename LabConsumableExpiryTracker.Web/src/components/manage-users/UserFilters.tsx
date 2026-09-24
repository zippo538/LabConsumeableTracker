import type { UserRole } from "../../types/user.types";

interface UserFiltersProps {
    search: string;
    roleFilter: "all" | UserRole;
    statusFilter: "all" | "active" | "inactive";

    onSearchChange: (value: string) => void;
    onRoleChange: (value: "all" | UserRole) => void;
    onStatusChange: (
        value: "all" | "active" | "inactive",
    ) => void;

    onAddUser: () => void;
}

const ROLE_OPTIONS: UserRole[] = [
    "WarehouseAdmin",
    "Scientist",
];

export function UserFilters({
    search,
    roleFilter,
    statusFilter,
    onSearchChange,
    onRoleChange,
    onStatusChange,
    onAddUser,
}: UserFiltersProps) {
    return (
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <input
                type="text"
                value={search}
                onChange={(event) =>
                    onSearchChange(event.target.value)
                }
                placeholder="Search name, username, or email..."
                className="w-full rounded-lg border border-gray-300 px-4 py-2 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 sm:max-w-xs"
            />

            <div className="flex flex-wrap gap-3">
                <select
                    value={roleFilter}
                    onChange={(event) =>
                        onRoleChange(
                            event.target.value as
                                | "all"
                                | UserRole,
                        )
                    }
                    className="rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-blue-500"
                >
                    <option value="all">
                        All roles
                    </option>

                    {ROLE_OPTIONS.map((role) => (
                        <option
                            key={role}
                            value={role}
                        >
                            {role === "WarehouseAdmin"
                                ? "Warehouse Admin"
                                : role}
                        </option>
                    ))}
                </select>

                <select
                    value={statusFilter}
                    onChange={(event) =>
                        onStatusChange(
                            event.target.value as
                                | "all"
                                | "active"
                                | "inactive",
                        )
                    }
                    className="rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-blue-500"
                >
                    <option value="all">
                        All statuses
                    </option>

                    <option value="active">
                        Active
                    </option>

                    <option value="inactive">
                        Inactive
                    </option>
                </select>

                <button
                    type="button"
                    onClick={onAddUser}
                    className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700"
                >
                    + Add user
                </button>
            </div>
        </div>
    );
}