export type UserRole =
    | "WarehouseAdmin"
    | "Scientist";

export type UserStatus =
    | "active"
    | "inactive";

export interface UserDto {
    id: string;
    username: string;
    email: string;
    fullName: string;
    roles: string[];
    isActive: boolean;
}

export interface UserProfileResponse {
    success: boolean;
    message: string;
    data: UserDto;
    errors: string[];
}

export interface UpdateMyProfilePayload {
    fullName: string;
}

export interface CreateUserPayload {
    username: string;
    fullName: string;
    email: string;
    password: string;
    role: UserRole;
}

export interface GetUsersParams {
    page?: number;
    pageSize?: number;
    search?: string;
    role?: UserRole;
    status?: UserStatus;
}

export interface UserListResponse {
    success: boolean;
    message: string;
    data: {
        items: UserDto[];
        totalCount: number;
        page: number;
        pageSize: number;
    };
    errors: string[];
}

export interface UserStatisticsDto {
    totalCount: number;
    activeCount: number;
    activeAdminCount: number;
    deactivatedCount: number;
}

export interface UserStatisticsResponse {
    success: boolean;
    message: string;
    data: UserStatisticsDto;
    errors: string[];
}

export interface UpdateUserPayload {
    username: string;
    fullName: string;
    email: string;
}

export interface ChangeUserPasswordPayload {
    newPassword: string;
}