import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";

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

    return (
        <main className="admin-page">
            <h2>Users</h2>
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
                        <option>Candidate</option>
                        <option>Recruiter</option>
                        <option>HiringManager</option>
                        <option>Admin</option>
                    </select>
                </label>
                <label>
                    Status
                    <select value={status} onChange={(e) => setStatus(e.target.value)}>
                        <option value="">All</option>
                        <option>Active</option>
                        <option>Inactive</option>
                        <option>Suspended</option>
                        <option>PendingApproval</option>
                    </select>
                </label>
                <button type="submit" className="admin-btn">Filter</button>
            </form>
            {error && <p className="admin-error" role="alert">{error}</p>}
            {loading && <p>Loading users…</p>}
            {!loading && items.length === 0 && <p className="admin-muted">No users match filters.</p>}
            {!loading && items.length > 0 && (
                <table className="admin-table">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Email</th>
                            <th>Role</th>
                            <th>Status</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {items.map((u) => (
                            <tr key={u.userId}>
                                <td data-label="Name">{u.fullName}</td>
                                <td data-label="Email">{u.email}</td>
                                <td data-label="Role">{u.role}</td>
                                <td data-label="Status">{u.status}</td>
                                <td data-label="Actions">
                                    <Link to={`/admin/users/${u.userId}`}>Open</Link>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </main>
    );
}
