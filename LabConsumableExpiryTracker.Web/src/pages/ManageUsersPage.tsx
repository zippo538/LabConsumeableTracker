import { useState } from "react";

import {
    useUserStatistics,
    useUsers,
} from "../hooks/useUser";

import type {
    UserDto,
    UserRole,
} from "../types/user.types";

import { AppLayout } from "../components/layout/AppLayout";
import { Pagination } from "../components/Pagination";

import { UserFilters } from "../components/manage-users/UserFilters";
import { UserTable } from "../components/manage-users/UserTable";
import { UserStatistics } from "../components/manage-users/UserStaitistic";
import { CreateUserModal } from "../components/manage-users/CreateUserModal";
import { useNavigate } from "react-router";

export default function ManageUsersPage() {
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    const [search, setSearch] = useState("");

    const [createSuccess, setCreateSuccess] = useState("");

    const [roleFilter, setRoleFilter] =
        useState<"all" | UserRole>("all");

    const [statusFilter, setStatusFilter] =
        useState<
            "all" | "active" | "inactive"
        >("all");

    const [isCreateModalOpen, setIsCreateModalOpen] =
        useState(false);

    const navigate = useNavigate();
    const {
        data,
        isLoading,
        isError,
    } = useUsers({
        page,
        pageSize,
        search: search.trim() || undefined,
        role:
            roleFilter === "all"
                ? undefined
                : roleFilter,
        status:
            statusFilter === "all"
                ? undefined
                : statusFilter,
    });

    const {
        data: statisticsData,
    } = useUserStatistics();

    const users: UserDto[] =
        data?.data.items ?? [];

    const statistics =
        statisticsData?.data;

    const handleSearchChange = (
        value: string,
    ) => {
        setSearch(value);
        setPage(1);
    };

    const handleRoleChange = (
        value: "all" | UserRole,
    ) => {
        setRoleFilter(value);
        setPage(1);
    };

    const handleStatusChange = (
        value:
            | "all"
            | "active"
            | "inactive",
    ) => {
        setStatusFilter(value);
        setPage(1);
    };

    const handlePageSizeChange = (
        value: number,
    ) => {
        setPageSize(value);
        setPage(1);
    };

    const handleManageUser = (
        user: UserDto,
    ) => {
        console.log("Manage user:", user);
        navigate(`/app/manage-users/${user.id}`);
    };

    return (
        <AppLayout
            title="User Management"
            description="Manage users and access."
        >
            <div className="space-y-6">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">
                        User management
                    </h1>

                    <p className="mt-1 text-sm text-gray-500">
                        Review accounts, change roles,
                        and deactivate access safely.
                    </p>
                </div>

                <UserStatistics
                    statistics={statistics}
                />

                <div className="rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
                    The system always retains at
                    least one active Warehouse Admin.
                </div>


                <UserFilters
                    search={search}
                    roleFilter={roleFilter}
                    statusFilter={statusFilter}
                    onSearchChange={
                        handleSearchChange
                    }
                    onRoleChange={
                        handleRoleChange
                    }
                    onStatusChange={
                        handleStatusChange
                    }
                    onAddUser={() =>
                        setIsCreateModalOpen(true)
                    }
                />

                {createSuccess && (
                    <div className="rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
                        {createSuccess}
                    </div>
                )}


                <UserTable
                    users={users}
                    isLoading={isLoading}
                    isError={isError}
                    onManage={
                        handleManageUser
                    }
                />

                <Pagination
                    page={page}
                    onPageChange={setPage}
                    totalItems={
                        statistics?.totalCount ?? 0
                    }
                    pageSize={pageSize}
                    onPageSizeChange={
                        handlePageSizeChange
                    }
                    pageSizeOptions={[
                        10,
                        20,
                        50,
                        100,
                    ]}
                />

                <CreateUserModal
                    isOpen={isCreateModalOpen}
                    onClose={() =>
                        setIsCreateModalOpen(false)
                    }
                    onSuccess={(message) => {
                        setCreateSuccess(message);
                    }}
                />
            </div>
        </AppLayout>
    );
}