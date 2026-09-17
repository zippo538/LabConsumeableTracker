// Contoh di main.tsx
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Provider as JotaiProvider } from 'jotai';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router';
import { LoginPage } from './pages/LoginPage';

const queryClient = new QueryClient();

export default function App() {
  return (
    <JotaiProvider>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            {/* Route lain... */}
            <Route path="*" element={<Navigate to="/login" replace />} />
          </Routes>
        </BrowserRouter>
      </QueryClientProvider>
    </JotaiProvider>
  );
}