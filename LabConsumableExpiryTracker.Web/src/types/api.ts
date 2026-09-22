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
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  code?: string;
  message?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
  fieldErrors?: Record<string, string[]>;
}

export const getApiErrorMessage = (payload: ApiErrorPayload): string =>
  Object.values(payload.errors ?? {})
    .flat()
    .filter(Boolean)
    .join(" ") ||
  payload.message ||
  payload.detail ||
  payload.title ||
  "The request could not be completed.";

export class ApiRequestError extends Error {
  readonly payload: ApiErrorPayload;
  readonly fieldErrors: Record<string, string[]>;

  constructor(message: string, payload: ApiErrorPayload = {}) {
    super(message);
    this.name = "ApiRequestError";
    this.payload = payload;
    this.fieldErrors = payload.errors ?? {};
  }
}
export interface LoginResponse {
  token: string;
  refreshToken?: string;
  id: string;
  username: string;
  fullName: string;
  role: AppRole;
}
