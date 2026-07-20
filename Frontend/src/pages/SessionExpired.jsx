import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth";
import { Button, ContentContainer, Alert } from "../components/ui/primitives";

export default function SessionExpired() {
    const navigate = useNavigate();
    const { setSessionExpired, clearSession } = useAuth();

    const goLogin = () => {
        clearSession();
        setSessionExpired(false);
        navigate("/login");
    };

    return (
        <ContentContainer>
            <h1>Session expired</h1>
            <Alert variant="warning">
                Your session is no longer valid. Sign in again to continue.
            </Alert>
            <div className="hs-inline" style={{ marginTop: "1rem" }}>
                <Button onClick={goLogin}>Sign in</Button>
                <Link to="/" className="hs-btn hs-btn--secondary">Home</Link>
            </div>
        </ContentContainer>
    );
}
