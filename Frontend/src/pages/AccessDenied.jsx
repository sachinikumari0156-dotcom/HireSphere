import { Link } from "react-router-dom";
import { ContentContainer, EmptyState } from "../components/ui/primitives";

export default function AccessDenied() {
    return (
        <ContentContainer>
            <EmptyState
                title="Access denied"
                action={
                    <p className="hs-inline" style={{ marginTop: "1rem", justifyContent: "center" }}>
                        <Link to="/" className="hs-btn hs-btn--primary">Return home</Link>
                        <Link to="/login" className="hs-btn hs-btn--secondary">Sign in with another account</Link>
                    </p>
                }
            >
                <p>You do not have permission to view this area. Role portals remain protected by server-side authorization.</p>
            </EmptyState>
        </ContentContainer>
    );
}
