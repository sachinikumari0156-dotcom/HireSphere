import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth";

export default function SessionExpired() {
    const navigate = useNavigate();
    const { setSessionExpired, clearSession } = useAuth();

    const goLogin = () => {
        clearSession();
        setSessionExpired(false);
        navigate("/login");
    };

    return (
        <main style={{ padding: "3rem 1.5rem", maxWidth: 640, margin: "0 auto" }}>
            <h1>Session expired</h1>
            <p>Your session is no longer valid. Sign in again to continue.</p>
            <p>
                <button type="button" onClick={goLogin}>Sign in</button>
                {" · "}
                <Link to="/">Home</Link>
            </p>
        </main>
    );
}
