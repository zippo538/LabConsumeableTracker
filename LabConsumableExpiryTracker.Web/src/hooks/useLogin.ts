// src/hooks/useLogin.ts
import { useMutation } from '@tanstack/react-query';
import { useSetAtom } from 'jotai';
import { loginApi } from '../api/authApi';
import type { LoginRequest, LoginResponse } from '../types/auth.types';
import { TOKEN_STORAGE_KEY, tokenAtom, userAtom } from '../lib/auth/authAtom';

export type LoginPayload = LoginRequest & { rememberMe: boolean };

export const useLogin = () => {
  const setToken = useSetAtom(tokenAtom);
  const setUser = useSetAtom(userAtom);

  return useMutation<LoginResponse, Error, LoginPayload>({
    mutationFn: ({ rememberMe, ...credentials }) => loginApi(credentials),
    onSuccess: (data, variables) => {
      setToken(data.token);
      setUser(data.profile);
      // UI-17 / D-11: "Ingat sesi masuk" => localStorage, selain itu sessionStorage
      const storage = variables.rememberMe ? localStorage : sessionStorage;
      storage.setItem(TOKEN_STORAGE_KEY, data.token);
    },
  });
};