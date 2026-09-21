export type AppRole = "Scientist" | "WarehouseAdmin";

export interface UserProfile {
  id: string;
  fullName: string;
  userName: string;
  email: string;
  isActive: boolean;
  roles: AppRole[];
}

export interface Session {
  token: string;
  tokenType: string;
  expiresAt?: string;
  user: UserProfile;
}

export interface PageResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface DashboardSummary {
  lowStockItemCount: number;
  expiringSoonLotCount: number;
  expiredLotCount: number;
  validLotCount: number;
  blockedLotCount: number;
  evaluatedAt: string;
}

export interface RegisterRequest {
  fullName: string;
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface CreateUserRequest extends RegisterRequest {
  roles?: AppRole[];
  isActive?: boolean;
}

export interface ApiErrorPayload {
  code?: string;
  message?: string;
  traceId?: string;
  fieldErrors?: Record<string, string[]>;
}
export interface LoginResponse {
  token: string;
  refreshToken?: string;
  id: string;
  username: string;
  fullName: string;
  role: AppRole;
}
