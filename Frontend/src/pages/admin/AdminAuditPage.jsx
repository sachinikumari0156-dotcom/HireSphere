import { useEffect, useState } from "react";
import api from "../../api/axios";

export default function AdminAuditPage() {
    const [items, setItems] = useState([]);
    const [action, setAction] = useState("");
    const [error, setError] = useState(null);

    async function load(params = {}) {
        try {
            const response = await api.get("/admin/audit-logs", { params });
            setItems(response.data.items || []);
        } catch (err) {
            setError(err.response?.data?.message || "Could not load audit logs.");
        }
    }

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/admin/audit-logs");
                if (alive) setItems(response.data.items || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load audit logs.");
            }
        })();
        return () => { alive = false; };
    }, []);

    async function exportCsv() {
        const response = await api.get("/admin/audit-logs/export", { responseType: "blob" });
        const url = URL.createObjectURL(response.data);
        const a = document.createElement("a");
        a.href = url;
        a.download = "admin-audit-logs.csv";
        a.click();
        URL.revokeObjectURL(url);
    }

    return (
        <main className="admin-page">
            <h2>Audit logs</h2>
            {error && <p className="admin-error" role="alert">{error}</p>}
            <form
                className="admin-filters"
                onSubmit={(e) => {
                    e.preventDefault();
                    load({ action: action || undefined });
                }}
            >
                <label>
                    Action contains
                    <input value={action} onChange={(e) => setAction(e.target.value)} />
                </label>
                <button type="submit" className="admin-btn">Filter</button>
                <button type="button" className="admin-btn secondary" onClick={exportCsv}>Export CSV</button>
            </form>
            {items.length === 0 && <p className="admin-muted">No audit events.</p>}
            <ul>
                {items.map((a) => (
                    <li key={a.id}>{a.createdAtUtc} · {a.action} · {a.entityType} · {a.details}</li>
                ))}
            </ul>
        </main>
    );
}
