import type {
  ApiErrorPayload,
  CreateUserRequest,
  DashboardSummary,
  LoginRequest,
  LoginResponse,
  PageResult,
  RegisterRequest,
  UserProfile,
} from "../types/api";

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL ?? "/api").replace(
  /\/$/,
  "",
);

export class ApiError extends Error {
  readonly status: number;
  readonly code?: string;
  readonly fieldErrors?: Record<string, string[]>;
  constructor(status: number, payload: ApiErrorPayload) {
    super(payload.message ?? "Permintaan tidak dapat diproses.");
    this.name = "ApiError";
    this.status = status;
    this.code = payload.code;
    this.fieldErrors = payload.fieldErrors;
  }
}

type ApiEnvelope<T> = { data?: T; result?: T };
function unwrap<T>(value: T | ApiEnvelope<T>): T {
  if (
    typeof value === "object" &&
    value !== null &&
    ("data" in value || "result" in value)
  ) {
    const envelope = value as ApiEnvelope<T>;
    return (envelope.data ?? envelope.result) as T;
  }
  return value as T;
}

async function request<T>(
  path: string,
  init: RequestInit = {},
  token?: string,
): Promise<T> {
  const headers = new Headers(init.headers);
  headers.set("Accept", "application/json");
  if (init.body) headers.set("Content-Type", "application/json");
  if (token) headers.set("Authorization", `Bearer ${token}`);
  const response = await fetch(`${apiBaseUrl}${path}`, { ...init, headers });
  if (response.status === 204) return undefined as T;
  const contentType = response.headers.get("content-type") ?? "";
  const body: unknown = contentType.includes("application/json")
    ? await response.json()
    : await response.text();
  if (!response.ok) {
    const payload =
      typeof body === "object" && body !== null
        ? (body as ApiErrorPayload)
        : { message: typeof body === "string" ? body : undefined };
    throw new ApiError(response.status, payload);
  }
  return unwrap(body as T | ApiEnvelope<T>);
}

function queryString(
  query: Record<string, string | number | undefined>,
): string {
  const parameters = new URLSearchParams();
  Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== "") parameters.set(key, String(value));
  });
  const result = parameters.toString();
  return result ? `?${result}` : "";
}

export const api = {
  register: (payload: RegisterRequest) =>
    request<UserProfile>("/auth/register", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  login: (payload: LoginRequest) =>
    request<LoginResponse>("/auth/login", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  getMe: (token: string) => request<UserProfile>("/users/me", {}, token),
  updateMe: (fullName: string, token: string) =>
    request<UserProfile>(
      "/users/me",
      { method: "PATCH", body: JSON.stringify({ fullName }) },
      token,
    ),
  getDashboard: (token: string) =>
    request<DashboardSummary>("/dashboard/summary", {}, token),
  getUsers: (token: string, search: string, page: number) =>
    request<PageResult<UserProfile>>(
      `/users${queryString({ Search: search, Page: page, PageSize: 10 })}`,
      {},
      token,
    ),
  getUser: (id: string, token: string) =>
    request<UserProfile>(`/users/${id}`, {}, token),
  createUser: (payload: CreateUserRequest, token: string) =>
    request<UserProfile>(
      "/users",
      { method: "POST", body: JSON.stringify(payload) },
      token,
    ),
  updateUser: (id: string, fullName: string, token: string) =>
    request<UserProfile>(
      `/users/${id}`,
      { method: "PATCH", body: JSON.stringify({ fullName }) },
      token,
    ),
  updateUserStatus: (id: string, isActive: boolean, token: string) =>
    request<UserProfile>(
      `/users/${id}/status`,
      { method: "PATCH", body: JSON.stringify({ isActive }) },
      token,
    ),
  updateUserRoles: (id: string, roles: string[], token: string) =>
    request<UserProfile>(
      `/users/${id}/roles`,
      { method: "PATCH", body: JSON.stringify({ roles }) },
      token,
    ),
};
