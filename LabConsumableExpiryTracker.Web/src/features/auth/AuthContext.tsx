import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { loginApi } from "../../api/authApi";
import { apiClient } from "../../api/authApi";
import type {
  AppRole,
  LoginRequest,
  LoginResponse,
  ServiceResult,
  UserProfile,
} from "../../types/auth.types";
import type { Session } from "../../types/api";

const sessionKey = "lab-consumable-expiry-tracker.session";

type AuthContextValue = {
  session: Session | null;
  isRestoring: boolean;
  login: (payload: LoginRequest) => Promise<void>;
  logout: () => void;
  refreshProfile: () => Promise<void>;
  updateSession: (session: Session) => void;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function readSession(): Session | null {
  try {
    const value = sessionStorage.getItem(sessionKey);
    return value ? (JSON.parse(value) as Session) : null;
  } catch {
    sessionStorage.removeItem(sessionKey);
    return null;
  }
}

// Helper: decode JWT untuk mendapatkan expiresAt
function decodeJwtExpiry(token: string): string | undefined {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload.exp ? new Date(payload.exp * 1000).toISOString() : undefined;
  } catch {
    return undefined;
  }
}

// Helper: fetch profil user dari /api/users/me
async function fetchUserProfile(token: string): Promise<UserProfile> {
  const response = await apiClient.get<ServiceResult<UserProfile>>(
    "/users/me",
    {
      headers: { Authorization: `Bearer ${token}` },
    },
  );
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || "Gagal memuat profil.");
  }
  return response.data.data;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(() => readSession());
  const [isRestoring, setIsRestoring] = useState(() => session !== null);

  const updateSession = useCallback((nextSession: Session) => {
    sessionStorage.setItem(sessionKey, JSON.stringify(nextSession));
    setSession(nextSession);
  }, []);

  const logout = useCallback(() => {
    sessionStorage.removeItem(sessionKey);
    setSession(null);
    setIsRestoring(false);
  }, []);

  const refreshProfile = useCallback(async () => {
    if (!session) return;
    try {
      const user = await fetchUserProfile(session.token);
      updateSession({ ...session, user });
    } catch (error) {
      // Jika gagal fetch profil (misal 401), logout
      logout();
    }
  }, [session, updateSession, logout]);

  useEffect(() => {
    if (!session) return;
    void refreshProfile()
      .catch(logout)
      .finally(() => setIsRestoring(false));
  }, [logout, refreshProfile, session]);

  const login = useCallback(
    async (payload: LoginRequest) => {
      const response: LoginResponse = await loginApi(payload);
      const expiresAt = decodeJwtExpiry(response.token);

      let user: UserProfile;
      try {
        user = await fetchUserProfile(response.token);
      } catch {
        // Fallback: buat profil minimal dari response login
        user = {
          id: response.id,
          fullName: response.fullName,
          userName: response.username,
          email: "", // ✅ PERBAIKAN: LoginResponse tidak punya email, kosongkan dulu
          isActive: true,
          roles: [response.role as AppRole], // ✅ Masukkan ke array
        };
      }

      updateSession({
        token: response.token,
        tokenType: "Bearer",
        expiresAt,
        user,
      });
      setIsRestoring(false);
    },
    [updateSession],
  );

  const value = useMemo(
    () => ({
      session,
      isRestoring,
      login,
      logout,
      refreshProfile,
      updateSession,
    }),
    [session, isRestoring, login, logout, refreshProfile, updateSession],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }
  return context;
}
