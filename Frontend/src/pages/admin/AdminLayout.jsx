import { NavLink, Outlet } from "react-router-dom";
import "./AdminPortal.css";

export default function AdminLayout() {
    return (
        <div className="admin-shell">
            <header className="admin-top">
                <h1 className="admin-brand">Administrator portal</h1>
                <nav className="admin-nav" aria-label="Administrator">
                    <NavLink to="/admin" end>Dashboard</NavLink>
                    <NavLink to="/admin/users">Users</NavLink>
                    <NavLink to="/admin/recruiter-requests">Recruiter requests</NavLink>
                    <NavLink to="/admin/organizations">Organizations</NavLink>
                    <NavLink to="/admin/departments">Departments</NavLink>
                    <NavLink to="/admin/roles">Roles</NavLink>
                    <NavLink to="/admin/hiring-managers">HM assignment</NavLink>
                </nav>
            </header>
            <Outlet />
        </div>
    );
}
