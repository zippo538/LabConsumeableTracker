import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Provider as JotaiProvider, useAtomValue } from "jotai";
import { BrowserRouter, Routes, Route, Navigate } from "react-router";
import { LoginPage } from "./pages/LoginPage";
import { DashboardPage } from "./pages/DashboardPage";
import { isAuthenticatedAtom } from "./lib/auth/authAtom";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAtomValue(isAuthenticatedAtom);
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return <>{children}</>;
}

export default function App() {
  return (
    <JotaiProvider>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <Routes>
            {/* Public Routes */}
            <Route path="/login" element={<LoginPage />} />

            {/* Protected Routes - Sesuai SRS §8.1 */}
            <Route
              path="/app/dashboard"
              element={
                <ProtectedRoute>
                  <DashboardPage />
                </ProtectedRoute>
              }
            />

            {/* Redirects */}
            {/* Root redirect ke dashboard jika terautentikasi */}
            <Route
              path="/"
              element={<Navigate to="/app/dashboard" replace />}
            />

            {/* Fallback untuk route tidak dikenal */}
            <Route
              path="*"
              element={<Navigate to="/app/dashboard" replace />}
            />
          </Routes>
        </BrowserRouter>
      </QueryClientProvider>
    </JotaiProvider>
  );
}
