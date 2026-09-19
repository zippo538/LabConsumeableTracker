import type {AppRole} from "./api"; // ← import dari api.ts

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  tokenType: string; // ✅ sesuai FR-13-02
  expiresAt: string; // ✅ sesuai FR-13-02
  id: string;
  username: string;
  fullName: string;
  role: string; // ✅ singular, karena backend LoginResponseDto hanya kirim 1 role
}

export interface ServiceResult<T> {
  success: boolean;
  message: string;
  data?: T;
  errors: string[];
}
export type { AppRole};
