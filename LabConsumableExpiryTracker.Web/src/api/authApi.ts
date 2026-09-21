import axios from 'axios';
import type { AxiosRequestConfig } from 'axios';
import type {
  LoginRequest,
  LoginResponse,
  RefreshTokenRequest,
  ServiceResult,
} from '../types/auth.types';
import {
  ACCESS_TOKEN_KEY,
  REFRESH_TOKEN_KEY,
  clearSession,
  getStoredAccessToken,
  getStoredRefreshToken,
} from '../lib/auth/authAtom';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5041/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use(
  (config) => {
    const token = getStoredAccessToken();
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };
    
    const url = originalRequest.url || '';
    if (
      url.includes('/auth/login') ||
      url.includes('/auth/refresh') ||
      url.includes('/auth/revoke')
    ) {
      return Promise.reject(error);
    }

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            if (originalRequest.headers && token) {
              originalRequest.headers.Authorization = `Bearer ${token}`;
            }
            return apiClient(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const currentRefreshToken = getStoredRefreshToken();
      if (!currentRefreshToken) {
        clearSession();
        window.location.href = '/login';
        return Promise.reject(error);
      }

      try {
        const refreshResponse = await axios.post<ServiceResult<LoginResponse>>(
          `${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5041/api'}/auth/refresh`,
          { refreshToken: currentRefreshToken }
        );

        if (refreshResponse.data?.success && refreshResponse.data.data) {
          const { token, refreshToken } = refreshResponse.data.data;
          const isRemember = !!localStorage.getItem(ACCESS_TOKEN_KEY);
          const storage = isRemember ? localStorage : sessionStorage;
          storage.setItem(ACCESS_TOKEN_KEY, token);
          storage.setItem(REFRESH_TOKEN_KEY, refreshToken);

          processQueue(null, token);

          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${token}`;
          }
          return apiClient(originalRequest);
        } else {
          throw new Error('Refresh token invalid');
        }
      } catch (refreshErr) {
        processQueue(refreshErr, null);
        clearSession();
        window.location.href = '/login';
        return Promise.reject(refreshErr);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);


export const loginApi = async (data: LoginRequest): Promise<LoginResponse> => {
  console.log('[AuthApi] Sending login request:', { username: data.username });
  const response = await apiClient.post<ServiceResult<LoginResponse>>('/auth/login', data);
  console.log('[AuthApi] Login response received:', response.data);
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Login failed');
  }
  return response.data.data;
};

export const refreshTokenApi = async (data: RefreshTokenRequest): Promise<LoginResponse> => {
  console.log('[AuthApi] Sending refresh-token request');
  const response = await apiClient.post<ServiceResult<LoginResponse>>('/auth/refresh', data);
  console.log('[AuthApi] Refresh-token response received:', response.data);
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Refresh token failed');
  }
  return response.data.data;
};

export const revokeTokenApi = async (data: RefreshTokenRequest): Promise<boolean> => {
  console.log('[AuthApi] Sending revoke request');
  const response = await apiClient.post<ServiceResult<boolean>>('/auth/revoke', data);
  console.log('[AuthApi] Revoke response received:', response.data);
  if (!response.data.success) {
    throw new Error(response.data.message || 'Revoke token failed');
  }
  return response.data.data ?? true;
};
