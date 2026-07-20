import { Outlet } from "react-router-dom";
import RoleShell from "../../components/layout/RoleShell";
import "./HiringManagerPortal.css";

const LINKS = [
    { to: "/hiring-manager", label: "Dashboard", end: true },
    { to: "/hiring-manager/jobs", label: "Assigned vacancies" },
    { to: "/hiring-manager/interviews", label: "Interviews" },
    { to: "/hiring-manager/compare", label: "Compare" }
];

export default function HiringManagerLayout() {
    return (
        <RoleShell title="Hiring Manager portal" navLabel="Hiring Manager" links={LINKS}>
            <Outlet />
        </RoleShell>
    );
}
