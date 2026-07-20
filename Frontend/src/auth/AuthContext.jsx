import { useCallback, useEffect, useMemo, useState } from "react";
import api from "../api/axios";
import { AuthContext } from "./auth-context";

const ROLE_HOME = {
    Candidate: "/candidate",
    Recruiter: "/recruiter",
    HiringManager: "/hiring-manager",
    Admin: "/admin"
};

export function AuthProvider({ children }) {
    const [user, setUser] = useState(null);
    const [token, setToken] = useState(() => localStorage.getItem("token"));
    const [loading, setLoading] = useState(true);
    const [sessionExpired, setSessionExpired] = useState(false);

    const clearSession = useCallback(() => {
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        setToken(null);
        setUser(null);
    }, []);

    const applyAuthResponse = useCallback((data) => {
        const nextToken = data.token;
        const nextUser = {
            userId: data.userId,
            fullName: data.fullName,
            email: data.email,
            role: data.role,
            organizationId: data.organizationId ?? null,
            departmentId: data.departmentId ?? null
        };

        localStorage.setItem("token", nextToken);
        localStorage.setItem("user", JSON.stringify(nextUser));
        setToken(nextToken);
        setUser(nextUser);
        setSessionExpired(false);
        return nextUser;
    }, []);

    useEffect(() => {
        let alive = true;

        (async () => {
            const stored = localStorage.getItem("token");
            if (!stored) {
                if (alive) setLoading(false);
                return;
            }

            try {
                const response = await api.get("/auth/me");
                if (!alive) return;
                const me = response.data;
                const nextUser = {
                    userId: me.userId,
                    fullName: me.fullName,
                    email: me.email,
                    role: me.role,
                    status: me.status,
                    organizationId: me.organizationId ?? null,
                    departmentId: me.departmentId ?? null,
                    permissions: me.permissions ?? []
                };
                localStorage.setItem("user", JSON.stringify(nextUser));
                setToken(stored);
                setUser(nextUser);
            } catch {
                if (!alive) return;
                localStorage.removeItem("token");
                localStorage.removeItem("user");
                setToken(null);
                setUser(null);
                setSessionExpired(true);
            } finally {
                if (alive) setLoading(false);
            }
        })();

        return () => {
            alive = false;
        };
    }, []);

    useEffect(() => {
        const onExpired = () => {
            clearSession();
            setSessionExpired(true);
        };
        window.addEventListener("hiresphere:session-expired", onExpired);
        return () => window.removeEventListener("hiresphere:session-expired", onExpired);
    }, [clearSession]);

    const login = useCallback(async (email, password) => {
        const response = await api.post("/auth/login", { email, password });
        return applyAuthResponse(response.data);
    }, [applyAuthResponse]);

    const registerCandidate = useCallback(async (payload) => {
        const response = await api.post("/auth/register/candidate", payload);
        return applyAuthResponse(response.data);
    }, [applyAuthResponse]);

    const logout = useCallback(async () => {
        try {
            if (token) {
                await api.post("/auth/logout");
            }
        } catch {
            // Client still clears local session.
        } finally {
            clearSession();
            setSessionExpired(false);
        }
    }, [clearSession, token]);

    const value = useMemo(() => ({
        user,
        token,
        loading,
        sessionExpired,
        isAuthenticated: Boolean(token && user),
        roleHome: (role) => ROLE_HOME[role] || "/",
        login,
        registerCandidate,
        logout,
        clearSession,
        setSessionExpired
    }), [
        user,
        token,
        loading,
        sessionExpired,
        login,
        registerCandidate,
        logout,
        clearSession
    ]);

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
}
