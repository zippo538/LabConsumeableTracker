
import { useMutation } from '@tanstack/react-query';

import { useSetAtom } from 'jotai';
import { loginApi, refreshTokenApi, revokeTokenApi } from '../api/authApi';
import type { LoginRequest, LoginResponse, RefreshTokenRequest } from '../types/auth.types';
import type { AppRole, UserProfile } from '../types/api';
import {
  clearSession,
  getStoredRefreshToken,
  refreshTokenAtom,
  saveSession,
  tokenAtom,
  userAtom,
} from "../lib/auth/authAtom";

export type LoginPayload = LoginRequest & {
  rememberMe?: boolean;
};

export const useLogin = () => {
  const setToken = useSetAtom(tokenAtom);
  const setRefreshToken = useSetAtom(refreshTokenAtom);
  const setUser = useSetAtom(userAtom);

  return useMutation<LoginResponse, Error, LoginPayload>({
    mutationFn: ({ rememberMe: _, ...credentials }) => loginApi(credentials),
    onSuccess: (data, variables) => {
      const userProfile: UserProfile = {
        id: data.id,
        fullName: data.fullName,
        userName: data.username,
        email: '',
        isActive: true,
        roles: data.role ? [data.role as AppRole] : [],
      };

      setToken(data.token);
      setRefreshToken(data.refreshToken);
      setUser(userProfile);

      saveSession(
        data.token,
        data.refreshToken,
        userProfile,
        variables.rememberMe ?? false,
      );
    },
  });
};

export const useRefreshToken = () => {
  const setToken = useSetAtom(tokenAtom);
  const setRefreshToken = useSetAtom(refreshTokenAtom);
  const setUser = useSetAtom(userAtom);

  return useMutation<LoginResponse, Error, RefreshTokenRequest | void>({
    mutationFn: (variables) => {
      const token = variables?.refreshToken ?? getStoredRefreshToken();
      if (!token) {
        throw new Error("No refresh token available");
      }
      return refreshTokenApi({ refreshToken: token });
    },
    onSuccess: (data) => {
      const userProfile: UserProfile = {
        id: data.id,
        fullName: data.fullName,
        userName: data.username,
        email: '',
        isActive: true,
        roles: data.role ? [data.role as AppRole] : [],
      };

      setToken(data.token);
      setRefreshToken(data.refreshToken);
      setUser(userProfile);

      const isRemember = !!localStorage.getItem("consumabletracker.token");
      saveSession(data.token, data.refreshToken, userProfile, isRemember);
    },
  });
};

export const useLogout = () => {
  const setToken = useSetAtom(tokenAtom);
  const setRefreshToken = useSetAtom(refreshTokenAtom);
  const setUser = useSetAtom(userAtom);

  return useMutation<boolean, Error, void>({
    mutationFn: async () => {
      const token = getStoredRefreshToken();
      if (token) {
        try {
          await revokeTokenApi({ refreshToken: token });
        } catch (e) {
          console.warn("Failed to revoke token on backend:", e);
        }
      }
      return true;
    },
    onSettled: () => {
      clearSession();
      setToken(null);
      setRefreshToken(null);
      setUser(null);
    },
  });
};
