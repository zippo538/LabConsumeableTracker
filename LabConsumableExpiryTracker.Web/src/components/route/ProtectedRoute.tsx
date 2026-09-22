import {useAtomValue} from "jotai";
import {Navigate} from "react-router";
import {
    isAuthenticatedAtom,
    userAtom,
} from "../../lib/auth/authAtom";

interface ProtectedRouteProps {
    children: React.ReactNode;
    allowedRoles?: string[];
}

export const ProtectedRoute = ({
                                   children,
                                   allowedRoles,
                               }: ProtectedRouteProps) => {
    const isAuthenticated = useAtomValue(isAuthenticatedAtom);
    const user = useAtomValue(userAtom);

    if (!isAuthenticated) {
        return <Navigate to="/login" replace/>;
    }

    if (allowedRoles) {
        const hasAccess = user?.roles?.some((role) =>
            allowedRoles.includes(role)
        );

        if (!hasAccess) {
            return <Navigate to="/app/dashboard" replace/>;
        }
    }

    return <>{children}</>;
};