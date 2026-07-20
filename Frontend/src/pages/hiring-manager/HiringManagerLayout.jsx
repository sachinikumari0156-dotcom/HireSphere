import { NavLink, Outlet } from "react-router-dom";
import "./HiringManagerPortal.css";

export default function HiringManagerLayout() {
    return (
        <div className="hm-shell">
            <header className="hm-top">
                <h1 className="hm-brand">Hiring Manager portal</h1>
                <nav className="hm-nav" aria-label="Hiring Manager">
                    <NavLink to="/hiring-manager" end>Dashboard</NavLink>
                    <NavLink to="/hiring-manager/jobs">Assigned vacancies</NavLink>
                    <NavLink to="/hiring-manager/interviews">Interviews</NavLink>
                    <NavLink to="/hiring-manager/compare">Compare</NavLink>
                </nav>
            </header>
            <Outlet />
        </div>
    );
}
