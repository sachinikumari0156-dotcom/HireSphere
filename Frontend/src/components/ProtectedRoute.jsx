import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../auth/useAuth";

export default function ProtectedRoute({ children, roles }) {
    const { isAuthenticated, loading, user, sessionExpired } = useAuth();
    const location = useLocation();

    if (loading) {
        return <div className="auth-loading">Loading session…</div>;
    }

    if (sessionExpired) {
        return <Navigate to="/session-expired" replace state={{ from: location }} />;
    }

    if (!isAuthenticated) {
        return <Navigate to="/login" replace state={{ from: location }} />;
    }

    if (roles?.length && !roles.includes(user?.role)) {
        return <Navigate to="/access-denied" replace />;
    }

    return children;
}
