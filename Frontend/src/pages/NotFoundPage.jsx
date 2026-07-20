import { Link } from "react-router-dom";
import { ContentContainer, EmptyState } from "../components/ui/primitives";

export default function NotFoundPage() {
    return (
        <ContentContainer>
            <EmptyState
                title="Page not found"
                action={
                    <Link to="/" className="hs-btn hs-btn--primary" style={{ marginTop: "1rem", display: "inline-flex" }}>
                        Return home
                    </Link>
                }
            >
                <p>The page you requested does not exist or is no longer available.</p>
            </EmptyState>
        </ContentContainer>
    );
}
