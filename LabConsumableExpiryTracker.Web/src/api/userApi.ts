import { apiClient } from "./authApi";
import type { ScientistUpdateProfileDto, UserProfileResponse } from "../types/user.types";

export const getMyProfile = async (): Promise<UserProfileResponse> => {
  const response = await apiClient.get("/users/me");

  return response.data;
};

export const updateMyProfile = async (
  payload: ScientistUpdateProfileDto
): Promise<UserProfileResponse> => {
  const response = await apiClient.patch("/users/me", payload);

  return response.data;
};