import { apiClient } from "./authApi";
import type { ChangeUserPasswordPayload, CreateUserPayload, GetUsersParams, UpdateMyProfilePayload, UpdateUserPayload, UserListResponse, UserProfileResponse, UserStatisticsResponse } from "../types/user.types";

export const getMyProfile = async (): Promise<UserProfileResponse> => {
  const response = await apiClient.get("/users/me");

  return response.data;
};

export const updateMyProfile = async (
  payload: UpdateMyProfilePayload
): Promise<UserProfileResponse> => {
  const response = await apiClient.patch("/users/me", payload);

  return response.data;
};

export const getUsers = async (
    params?: GetUsersParams
): Promise<UserListResponse> => {
    const response = await apiClient.get("/users", {
        params,
    });

    return response.data;
};

export const getUserStatistics =
    async (): Promise<UserStatisticsResponse> => {
        const response = await apiClient.get("/users/stats");

        return response.data;
    };

  export const getUserById = async (
    id: string
): Promise<UserProfileResponse> => {
    const response = await apiClient.get(`/users/${id}`);

    return response.data;
};

export const createUser = async (
    payload: CreateUserPayload
): Promise<UserProfileResponse> => {
    const response = await apiClient.post(
        "/users",
        payload
    );

    return response.data;
};
export const updateUser = async (
    id: string,
    payload: UpdateUserPayload,
): Promise<UserProfileResponse> => {
    const response = await apiClient.put(
        `/users/${id}`,
        payload,
    );

    console.log("UPDATE USER RESPONSE:", response.data);

    return response.data;
};

export const activateUser = async (
    id: string,
): Promise<UserProfileResponse> => {
    const response = await apiClient.put(
        `/users/${id}/activate`,
    );

    return response.data;
};

export const deactivateUser = async (
    id: string,
): Promise<UserProfileResponse> => {
    const response = await apiClient.put(
        `/users/${id}/deactivate`,
    );

    return response.data;
};

export const changeUserPassword = async (
    id: string,
    payload: ChangeUserPasswordPayload,
): Promise<UserProfileResponse> => {
    const response = await apiClient.put(
        `/users/${id}/password`,
        payload,
    );

    return response.data;
};