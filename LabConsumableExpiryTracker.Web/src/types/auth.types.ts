

export type AppRole = 'Scientist' | 'WarehouseAdmin' | string;

export interface UserProfile {
  id: string;
  fullName: string;
  userName: string;
  role: string;
}

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
  id: string;
  username: string;
  fullName: string;
  role: string;
}

export interface ServiceResult<T> {
  success: boolean;
  message: string;
  data?: T;
  errors: string[];
}
