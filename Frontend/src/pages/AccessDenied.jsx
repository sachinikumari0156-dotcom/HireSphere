import { Link } from "react-router-dom";

export default function AccessDenied() {
    return (
        <main style={{ padding: "3rem 1.5rem", maxWidth: 640, margin: "0 auto" }}>
            <h1>Access denied</h1>
            <p>You do not have permission to view this area.</p>
            <p>
                <Link to="/">Return home</Link>
                {" · "}
                <Link to="/login">Sign in with another account</Link>
            </p>
        </main>
    );
}
