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
                    ? "Summary Item Stock"
                    : "Your laboratory workspace"
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