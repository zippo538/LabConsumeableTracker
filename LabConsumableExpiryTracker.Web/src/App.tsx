import {QueryClient, QueryClientProvider} from "@tanstack/react-query";
import {Provider as JotaiProvider} from "jotai";
import {
    BrowserRouter,
    Routes,
    Route,
    Navigate,
} from "react-router";

import {LoginPage} from "./pages/LoginPage";
import {DashboardPage} from "./pages/DashboardPage";
import {ItemListPage} from "./pages/ItemPage";

import {LotsPage} from './pages/LotPage';
import {ItemDetailPage} from "./pages/ItemDetailPage";
import {ProtectedRoute} from "./components/route/ProtectedRoute";

const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            retry: 1,
            refetchOnWindowFocus: false,
        },
    },
});


export default function App() {
    return (
        <JotaiProvider>
            <QueryClientProvider client={queryClient}>
                <BrowserRouter>
                    <Routes>
                        <Route path="/login" element={<LoginPage/>}/>

                        <Route
                            path="/app/dashboard"
                            element={
                                <ProtectedRoute
                                    allowedRoles={[
                                        "WarehouseAdmin",
                                        "Scientist",
                                    ]}
                                >
                                    <DashboardPage/>
                                </ProtectedRoute>
                            }
                        />

                        <Route
                            path="/app/items"
                            element={
                                <ProtectedRoute
                                    allowedRoles={["WarehouseAdmin"]}
                                >
                                    <ItemListPage/>
                                </ProtectedRoute>
                            }
                        />

                        <Route
                            path="/items/:id"
                            element={
                                <ProtectedRoute
                                    allowedRoles={["WarehouseAdmin"]}
                                >
                                    <ItemDetailPage/>
                                </ProtectedRoute>
                            }
                        />

                        <Route
                            path="/app/lots"
                            element={
                                <ProtectedRoute allowedRoles={["WarehouseAdmin"]}>
                                    <LotsPage/>
                                </ProtectedRoute>
                            }
                        />

                        <Route
                            path="/"
                            element={<Navigate to="/app/dashboard" replace/>}
                        />

                        <Route
                            path="*"
                            element={<Navigate to="/app/dashboard" replace/>}
                        />
                    </Routes>
                </BrowserRouter>
            </QueryClientProvider>
        </JotaiProvider>
    );
}
