import { useAuth } from "../auth/useAuth";

function RoleShell({ title, description }) {
    const { user } = useAuth();

    return (
        <div style={{ padding: "2rem 1.5rem", maxWidth: 960, margin: "0 auto" }}>
            <h1>{title}</h1>
            <p>{description}</p>
            <p>
                Signed in as <strong>{user?.fullName}</strong> ({user?.role}).
            </p>
            <p style={{ opacity: 0.8 }}>
                Portal features for this role will be expanded in later phases.
                This shell confirms authorized access only.
            </p>
        </div>
    );
}

export function AdminDashboard() {
    return (
        <RoleShell
            title="Administrator workspace"
            description="Manage users, recruiter access requests, and system scope."
        />
    );
}
