import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { useAuth } from "../../auth/useAuth";
import api from "../../api/axios";

export default function AdminUserDetailPage() {
    const { id } = useParams();
    const { user: me } = useAuth();
    const [detail, setDetail] = useState(null);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);
    const [roleName, setRoleName] = useState("HiringManager");
    const [orgId, setOrgId] = useState("");
    const [deptId, setDeptId] = useState("");

    async function load() {
        const response = await api.get(`/admin/users/${id}`);
        setDetail(response.data);
    }

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                await load();
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load user.");
            }
        })();
        return () => { alive = false; };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [id]);

    async function changeStatus(status) {
        setError(null);
        setSuccess(null);
        if (Number(id) === me?.userId && (status === "Inactive" || status === "Suspended")) {
            setError("Administrators cannot disable their own account through this endpoint.");
            return;
        }
        if (!window.confirm(`Set status to ${status}?`)) return;
        try {
            await api.patch(`/admin/users/${id}/status`, { status });
            setSuccess(`Status updated to ${status}.`);
            await load();
        } catch (err) {
            setError(err.response?.data?.message || "Could not update status.");
        }
    }

    async function assignRole(e) {
        e.preventDefault();
        setError(null);
        try {
            await api.post(`/admin/users/${id}/roles`, { roleName });
            setSuccess(`Role ${roleName} assigned.`);
            await load();
        } catch (err) {
            setError(err.response?.data?.message || "Could not assign role.");
        }
    }

    async function assignOrg(e) {
        e.preventDefault();
        setError(null);
        try {
            await api.put(`/admin/users/${id}/organization`, {
                organizationId: orgId ? Number(orgId) : null,
                departmentId: deptId ? Number(deptId) : null
            });
            setSuccess("Organization/department updated.");
            await load();
        } catch (err) {
            setError(err.response?.data?.message || "Could not assign organization.");
        }
    }

    if (!detail && !error) return <div className="admin-page"><p>Loading user…</p></div>;
    if (error && !detail) return <div className="admin-page"><p className="admin-error" role="alert">{error}</p></div>;

    const isSelf = Number(id) === me?.userId;

    return (
        <div className="admin-page">
            <h2>User detail</h2>
            <p className="admin-muted">{detail.fullName} · {detail.email} · {detail.role} · {detail.status}</p>
            {error && <p className="admin-error" role="alert">{error}</p>}
            {success && <p className="admin-success" role="status">{success}</p>}
            {isSelf && (
                <p className="admin-muted" role="note">
                    Self-disable is blocked. Last-Administrator protection also applies when disabling the final active Admin.
                </p>
            )}
            <div className="admin-filters">
                <button type="button" className="admin-btn" onClick={() => changeStatus("Active")}>Activate</button>
                <button type="button" className="admin-btn secondary" onClick={() => changeStatus("Inactive")}>Disable</button>
                <button type="button" className="admin-btn secondary" onClick={() => changeStatus("Suspended")}>Suspend</button>
            </div>
            <h3>Roles</h3>
            <ul>
                {(detail.roles || []).map((r) => (
                    <li key={r.roleId}>{r.roleName}</li>
                ))}
            </ul>
            <form className="admin-form" onSubmit={assignRole}>
                <label>
                    Assign role
                    <select value={roleName} onChange={(e) => setRoleName(e.target.value)}>
                        <option>Candidate</option>
                        <option>Recruiter</option>
                        <option>HiringManager</option>
                        <option>Admin</option>
                    </select>
                </label>
                <button type="submit" className="admin-btn">Assign role</button>
            </form>
            <form className="admin-form" onSubmit={assignOrg}>
                <label>
                    Organization ID
                    <input value={orgId} onChange={(e) => setOrgId(e.target.value)} />
                </label>
                <label>
                    Department ID
                    <input value={deptId} onChange={(e) => setDeptId(e.target.value)} />
                </label>
                <button type="submit" className="admin-btn secondary">Assign organization</button>
            </form>
            <Link className="admin-btn secondary" to="/admin/users">Back</Link>
        </div>
    );
}
