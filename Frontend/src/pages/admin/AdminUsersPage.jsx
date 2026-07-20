import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";
import FilterDrawer from "../../components/ui/FilterDrawer";
import { StatusBadge } from "../../components/ui/primitives";
import { friendlyStatus } from "../../utils/statusLabels";

function roleLabel(role) {
    if (role === "Admin") return "Administrator";
    if (role === "HiringManager") return "Hiring Manager";
    return role;
}

export default function AdminUsersPage() {
    const [items, setItems] = useState([]);
    const [keyword, setKeyword] = useState("");
    const [role, setRole] = useState("");
    const [status, setStatus] = useState("");
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    async function load(params = {}) {
        setLoading(true);
        setError(null);
        try {
            const response = await api.get("/admin/users", { params });
            setItems(response.data.items || []);
        } catch (err) {
            setError(err.response?.data?.message || "Could not load users.");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        let alive = true;
        (async () => {
            setLoading(true);
            setError(null);
            try {
                const response = await api.get("/admin/users");
                if (alive) setItems(response.data.items || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load users.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    const filterForm = (
        <form
            className="admin-filters"
            onSubmit={(e) => {
                e.preventDefault();
                load({ keyword, role: role || undefined, status: status || undefined });
            }}
        >
            <label>
                Search
                <input value={keyword} onChange={(e) => setKeyword(e.target.value)} />
            </label>
            <label>
                Role
                <select value={role} onChange={(e) => setRole(e.target.value)}>
                    <option value="">All</option>
                    <option value="Candidate">Candidate</option>
                    <option value="Recruiter">Recruiter</option>
                    <option value="HiringManager">Hiring Manager</option>
                    <option value="Admin">Administrator</option>
                </select>
            </label>
            <label>
                Status
                <select value={status} onChange={(e) => setStatus(e.target.value)}>
                    <option value="">All</option>
                    <option value="Active">Active</option>
                    <option value="Inactive">Inactive</option>
                    <option value="Suspended">Suspended</option>
                    <option value="PendingApproval">Pending approval</option>
                </select>
            </label>
            <button type="submit" className="admin-btn">Filter</button>
        </form>
    );

    return (
        <div className="admin-page portal-page">
            <h2>Users</h2>
            <FilterDrawer title="user filters">{filterForm}</FilterDrawer>
            {error && <p className="admin-error" role="alert">{error}</p>}
            {loading && <p>Loading users…</p>}
            {!loading && items.length === 0 && <p className="admin-muted">No users match filters.</p>}
            {!loading && items.length > 0 && (
                <div className="portal-table-wrap" tabIndex={0} aria-label="Users list">
                    <table className="admin-table">
                        <caption className="hs-sr-only">Administrator user directory</caption>
                        <thead>
                            <tr>
                                <th scope="col">Name</th>
                                <th scope="col">Email</th>
                                <th scope="col">Role</th>
                                <th scope="col">Status</th>
                                <th scope="col">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {items.map((u) => (
                                <tr key={u.userId}>
                                    <td data-label="Name">{u.fullName}</td>
                                    <td data-label="Email">{u.email}</td>
                                    <td data-label="Role">{roleLabel(u.role)}</td>
                                    <td data-label="Status"><StatusBadge label={friendlyStatus(u.status)} /></td>
                                    <td data-label="Actions">
                                        <Link to={`/admin/users/${u.userId}`}>Open</Link>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}
