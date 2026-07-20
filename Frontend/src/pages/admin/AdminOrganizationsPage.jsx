import { useEffect, useState } from "react";
import api from "../../api/axios";

export default function AdminOrganizationsPage() {
    const [items, setItems] = useState([]);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);
    const [form, setForm] = useState({ name: "", code: "", description: "" });

    async function load() {
        const response = await api.get("/admin/organizations");
        setItems(response.data || []);
    }

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/admin/organizations");
                if (alive) setItems(response.data || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load organizations.");
            }
        })();
        return () => { alive = false; };
    }, []);

    async function create(e) {
        e.preventDefault();
        setError(null);
        if (!form.name.trim() || !form.code.trim()) {
            setError("Name and code are required.");
            return;
        }
        try {
            await api.post("/admin/organizations", form);
            setSuccess("Organization created.");
            setForm({ name: "", code: "", description: "" });
            await load();
        } catch (err) {
            setError(err.response?.data?.message || "Create failed.");
        }
    }

    return (
        <div className="admin-page">
            <h2>Organizations</h2>
            {error && <p className="admin-error" role="alert">{error}</p>}
            {success && <p className="admin-success" role="status">{success}</p>}
            <form className="admin-form" onSubmit={create}>
                <label>
                    Name
                    <input required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
                </label>
                <label>
                    Code
                    <input required value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value })} />
                </label>
                <label>
                    Description
                    <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
                </label>
                <button type="submit" className="admin-btn">Create organization</button>
            </form>
            <ul>
                {items.map((o) => (
                    <li key={o.id}>{o.name} ({o.code}) · {o.status} · depts {o.departmentCount}</li>
                ))}
            </ul>
        </div>
    );
}
