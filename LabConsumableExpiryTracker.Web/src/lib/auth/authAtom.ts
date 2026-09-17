// src/lib/auth/authAtom.ts
import { atom } from 'jotai';
import type { UserProfile } from '../../types/auth.types';

export const TOKEN_STORAGE_KEY = 'consumabletracker.token';

const readStoredToken = (): string | null =>
  localStorage.getItem(TOKEN_STORAGE_KEY) ?? sessionStorage.getItem(TOKEN_STORAGE_KEY);

export const tokenAtom = atom<string | null>(readStoredToken());
export const userAtom = atom<UserProfile | null>(null);
export const isAuthenticatedAtom = atom((get) => !!get(tokenAtom));