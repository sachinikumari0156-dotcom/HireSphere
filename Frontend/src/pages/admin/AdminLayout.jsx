import { Outlet } from "react-router-dom";
import RoleShell from "../../components/layout/RoleShell";
import "./AdminPortal.css";

const LINKS = [
    { to: "/admin", label: "Dashboard", end: true },
    { to: "/admin/users", label: "Users" },
    { to: "/admin/recruiter-requests", label: "Recruiter requests" },
    { to: "/admin/organizations", label: "Organizations" },
    { to: "/admin/departments", label: "Departments" },
    { to: "/admin/roles", label: "Roles" },
    { to: "/admin/hiring-managers", label: "Hiring Manager assignment" },
    { to: "/admin/audit", label: "Audit" },
    { to: "/admin/monitoring", label: "Monitoring" },
    { to: "/admin/integrations", label: "Integrations" },
    { to: "/admin/storage", label: "Storage" },
    { to: "/admin/analytics", label: "Analytics" },
    { to: "/admin/final-decisions", label: "Final decisions" }
];

export default function AdminLayout() {
    return (
        <RoleShell title="Administrator portal" navLabel="Administrator" links={LINKS}>
            <Outlet />
        </RoleShell>
    );
}
