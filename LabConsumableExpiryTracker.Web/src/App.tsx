import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Provider as JotaiProvider, useAtomValue } from "jotai";
import {
  BrowserRouter,
  Routes,
  Route,
  Navigate,
} from "react-router";

import { LoginPage } from "./pages/LoginPage";
import { DashboardPage } from "./pages/DashboardPage";
import { ItemListPage } from "./pages/ItemPage";

import { isAuthenticatedAtom } from "./lib/auth/authAtom";
import { LotsPage } from './pages/LotPage';
import { LotDetailPage } from './pages/LotDetailPage';
import { ItemDetailPage } from "./pages/ItemDetailPage";

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
            <Route path="/login" element={<LoginPage />} />

            <Route
              path="/app/dashboard"
              element={
                <ProtectedRoute>
                  <DashboardPage />
                </ProtectedRoute>
              }
            />

            <Route
              path="/app/items"
              element={
                <ProtectedRoute>
                  <ItemListPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/items/:id"
              element={<ProtectedRoute>
                <ItemDetailPage />
              </ProtectedRoute>}
            />
            <Route
              path="/app/lots"
              element={
                <ProtectedRoute>
                  <LotsPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/app/lots/:id"
              element={
                <ProtectedRoute>
                  <LotDetailPage />
                </ProtectedRoute>
              }
            />

            <Route
              path="/"
              element={<Navigate to="/app/dashboard" replace />}
            />

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
