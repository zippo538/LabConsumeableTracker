import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { api } from '../../services/apiClient'
import type { LoginRequest, Session } from '../../types/api'

const sessionKey = 'lab-consumable-expiry-tracker.session'
type AuthContextValue = { session: Session | null; isRestoring: boolean; login: (payload: LoginRequest) => Promise<void>; logout: () => void; refreshProfile: () => Promise<void>; updateSession: (session: Session) => void }
const AuthContext = createContext<AuthContextValue | undefined>(undefined)
function readSession(): Session | null { try { const value = sessionStorage.getItem(sessionKey); return value ? JSON.parse(value) as Session : null } catch { sessionStorage.removeItem(sessionKey); return null } }

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(() => readSession())
  const [isRestoring, setIsRestoring] = useState(() => session !== null)
  const updateSession = useCallback((nextSession: Session) => { sessionStorage.setItem(sessionKey, JSON.stringify(nextSession)); setSession(nextSession) }, [])
  const logout = useCallback(() => { sessionStorage.removeItem(sessionKey); setSession(null); setIsRestoring(false) }, [])
  const refreshProfile = useCallback(async () => { if (!session) return; const user = await api.getMe(session.token); updateSession({ ...session, user }) }, [session, updateSession])
  useEffect(() => { if (!session) return; void refreshProfile().catch(logout).finally(() => setIsRestoring(false)) }, [logout, refreshProfile, session])
  const login = useCallback(async (payload: LoginRequest) => { const response = await api.login(payload); updateSession({ token: response.token, tokenType: response.tokenType ?? 'Bearer', expiresAt: response.expiresAt, user: response.user }); setIsRestoring(false) }, [updateSession])
  const value = useMemo(() => ({ session, isRestoring, login, logout, refreshProfile, updateSession }), [session, isRestoring, login, logout, refreshProfile, updateSession])
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
export function useAuth(): AuthContextValue { const context = useContext(AuthContext); if (!context) throw new Error('useAuth must be used inside AuthProvider.'); return context }
