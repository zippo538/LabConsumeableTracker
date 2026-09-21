import { useAtomValue } from "jotai";
import { userAtom } from "../lib/auth/authAtom";
import { AppLayout } from "../components/layout/AppLayout";
import { ScientistDashboard } from "./ScientistDashboard";
import { WarehouseAdminDashboard } from "./WarehouseAdminDashboard";

export const DashboardPage = () => {
    const user = useAtomValue(userAtom);

    const isWarehouseAdmin =
        user?.roles.includes("WarehouseAdmin") ?? false;

    return (
        <AppLayout
            title="Dashboard"
            description={
                isWarehouseAdmin
                    ? "Ringkasan stok gudang laboratorium"
                    : "Ruang kerja laboratorium Anda"
            }
        >
            {isWarehouseAdmin ? (
                <WarehouseAdminDashboard user={user} />
            ) : (
                <ScientistDashboard user={user} />
            )}
        </AppLayout>
    );
};