import { Outlet } from "react-router-dom";
import RoleShell from "../../components/layout/RoleShell";
import "./RecruiterPortal.css";

const LINKS = [
    { to: "/recruiter", label: "Dashboard", end: true },
    { to: "/recruiter/jobs", label: "Jobs" },
    { to: "/recruiter/jobs/new", label: "Create job" },
    { to: "/recruiter/screening", label: "Screening" },
    { to: "/recruiter/assessments", label: "Assessments" },
    { to: "/recruiter/interviews", label: "Interviews" },
    { to: "/recruiter/reports", label: "Reports" },
    { to: "/recruiter/compare", label: "Compare" }
];

export default function RecruiterLayout() {
    return (
        <RoleShell title="Recruiter portal" navLabel="Recruiter" links={LINKS}>
            <Outlet />
        </RoleShell>
    );
}
