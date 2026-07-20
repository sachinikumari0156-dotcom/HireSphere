import { useEffect, useState } from "react";
import api from "../../api/axios";

export default function AdminDepartmentsPage() {
    const [items, setItems] = useState([]);
    const [orgs, setOrgs] = useState([]);
    const [error, setError] = useState(null);
    const [form, setForm] = useState({ organizationId: "", name: "", code: "" });

    async function load() {
        const [d, o] = await Promise.all([
            api.get("/admin/departments"),
            api.get("/admin/organizations")
        ]);
        setItems(d.data || []);
        setOrgs(o.data || []);
    }

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const [d, o] = await Promise.all([
                    api.get("/admin/departments"),
                    api.get("/admin/organizations")
                ]);
                if (!alive) return;
                setItems(d.data || []);
                setOrgs(o.data || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load departments.");
            }
        })();
        return () => { alive = false; };
    }, []);

    async function create(e) {
        e.preventDefault();
        setError(null);
        if (!form.organizationId || !form.name.trim()) {
            setError("Organization and name are required.");
            return;
        }
        try {
            await api.post("/admin/departments", {
                organizationId: Number(form.organizationId),
                name: form.name,
                code: form.code || null
            });
            setForm({ organizationId: form.organizationId, name: "", code: "" });
            await load();
        } catch (err) {
            setError(err.response?.data?.message || "Create failed.");
        }
    }

    async function archive(id) {
        try {
            await api.patch(`/admin/departments/${id}/status`, { status: "Archived" });
            await load();
        } catch (err) {
            setError(err.response?.data?.message || "Archive failed.");
        }
    }

    return (
        <div className="admin-page">
            <h2>Departments</h2>
            {error && <p className="admin-error" role="alert">{error}</p>}
            <form className="admin-form" onSubmit={create}>
                <label>
                    Organization
                    <select
                        required
                        value={form.organizationId}
                        onChange={(e) => setForm({ ...form, organizationId: e.target.value })}
                    >
                        <option value="">Select…</option>
                        {orgs.map((o) => (
                            <option key={o.id} value={o.id}>{o.name}</option>
                        ))}
                    </select>
                </label>
                <label>
                    Name
                    <input required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
                </label>
                <label>
                    Code
                    <input value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value })} />
                </label>
                <button type="submit" className="admin-btn">Create department</button>
            </form>
            <ul>
                {items.map((d) => (
                    <li key={d.id}>
                        {d.name} · {d.organizationName} · {d.status} · users {d.userCount}
                        {d.status !== "Archived" && (
                            <>
                                {" "}
                                <button type="button" className="admin-btn secondary" onClick={() => archive(d.id)}>
                                    Archive
                                </button>
                            </>
                        )}
                    </li>
                ))}
            </ul>
        </div>
    );
}
