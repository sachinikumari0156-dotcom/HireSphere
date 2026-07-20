import { NavLink, Outlet } from "react-router-dom";
import "./RecruiterPortal.css";

export default function RecruiterLayout() {
    return (
        <div className="rec-shell">
            <header className="rec-top">
                <h1 className="rec-brand">Recruiter portal</h1>
                <nav className="rec-nav" aria-label="Recruiter">
                    <NavLink to="/recruiter" end>Dashboard</NavLink>
                    <NavLink to="/recruiter/jobs">Jobs</NavLink>
                    <NavLink to="/recruiter/jobs/new">Create job</NavLink>
                    <NavLink to="/recruiter/screening">Screening</NavLink>
                    <NavLink to="/recruiter/assessments">Assessments</NavLink>
                    <NavLink to="/recruiter/interviews">Interviews</NavLink>
                    <NavLink to="/recruiter/reports">Reports</NavLink>
                    <NavLink to="/recruiter/compare">Compare</NavLink>
                </nav>
            </header>
            <Outlet />
        </div>
    );
}
