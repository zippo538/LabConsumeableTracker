// src/lib/auth/authAtom.ts
import { atom } from 'jotai';
import type { UserProfile } from '../../types/auth.types';

export const ACCESS_TOKEN_KEY = 'consumabletracker.token';
export const REFRESH_TOKEN_KEY = 'consumabletracker.refreshToken';
export const USER_STORAGE_KEY = 'consumabletracker.user';

export const getStoredAccessToken = (): string | null =>
  localStorage.getItem(ACCESS_TOKEN_KEY) ?? sessionStorage.getItem(ACCESS_TOKEN_KEY);

export const getStoredRefreshToken = (): string | null =>
  localStorage.getItem(REFRESH_TOKEN_KEY) ?? sessionStorage.getItem(REFRESH_TOKEN_KEY);

export const getStoredUser = (): UserProfile | null => {
  const raw = localStorage.getItem(USER_STORAGE_KEY) ?? sessionStorage.getItem(USER_STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as UserProfile;
  } catch {
    return null;
  }
};

export const tokenAtom = atom<string | null>(getStoredAccessToken());
export const refreshTokenAtom = atom<string | null>(getStoredRefreshToken());
export const userAtom = atom<UserProfile | null>(getStoredUser());
export const isAuthenticatedAtom = atom((get) => !!get(tokenAtom));

export const saveSession = (
  token: string,
  refreshToken: string,
  user: UserProfile,
  rememberMe: boolean = false
) => {
  const primary = rememberMe ? localStorage : sessionStorage;
  const secondary = rememberMe ? sessionStorage : localStorage;

  // Clear secondary to prevent stale conflicting data
  secondary.removeItem(ACCESS_TOKEN_KEY);
  secondary.removeItem(REFRESH_TOKEN_KEY);
  secondary.removeItem(USER_STORAGE_KEY);

  primary.setItem(ACCESS_TOKEN_KEY, token);
  primary.setItem(REFRESH_TOKEN_KEY, refreshToken);
  primary.setItem(USER_STORAGE_KEY, JSON.stringify(user));
};

export const clearSession = () => {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem(USER_STORAGE_KEY);

  sessionStorage.removeItem(ACCESS_TOKEN_KEY);
  sessionStorage.removeItem(REFRESH_TOKEN_KEY);
  sessionStorage.removeItem(USER_STORAGE_KEY);
};