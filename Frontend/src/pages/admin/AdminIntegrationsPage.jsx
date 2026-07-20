import { useEffect, useState } from "react";
import api from "../../api/axios";
import { friendlyStatus } from "../../utils/statusLabels";

export default function AdminIntegrationsPage() {
    const [statuses, setStatuses] = useState([]);
    const [failed, setFailed] = useState([]);
    const [error, setError] = useState(null);
    const [message, setMessage] = useState(null);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const [s, f] = await Promise.all([
                    api.get("/admin/integrations/status"),
                    api.get("/admin/notifications/failed")
                ]);
                if (!alive) return;
                setStatuses(s.data || []);
                setFailed(f.data || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load integrations.");
            }
        })();
        return () => { alive = false; };
    }, []);

    async function reload() {
        setError(null);
        try {
            const [s, f] = await Promise.all([
                api.get("/admin/integrations/status"),
                api.get("/admin/notifications/failed")
            ]);
            setStatuses(s.data || []);
            setFailed(f.data || []);
        } catch (err) {
            setError(err.response?.data?.message || "Could not load integrations.");
        }
    }

    async function healthCheck(name) {
        setMessage(null);
        try {
            const key = encodeURIComponent(name.split(" ")[0]);
            const response = await api.post(`/admin/integrations/${key}/health-check`);
            setMessage(`${response.data.name}: ${response.data.status}`);
            await reload();
        } catch (err) {
            setError(err.response?.data?.message || "Health check failed.");
        }
    }

    async function retry(id) {
        setMessage(null);
        try {
            await api.post(`/admin/notifications/${id}/retry`);
            setMessage(`Retry queued for delivery #${id}`);
            await reload();
        } catch (err) {
            setError(err.response?.data?.message || "Retry failed.");
        }
    }

    if (!statuses.length && !error) return <div className="admin-page"><p>Loading integrations…</p></div>;
    if (error && !statuses.length) return <div className="admin-page"><p className="admin-error">{error}</p></div>;

    return (
        <div className="admin-page">
            <h2>Integration providers</h2>
            <p className="admin-muted">Statuses are truthful. Secrets are never shown. External providers remain Not Configured until verified with real credentials.</p>
            <p><span className="portal-provider-chip">Provider Not Configured</span> appears when credentials are absent.</p>
            {message && <p className="admin-ok">{message}</p>}
            {error && <p className="admin-error">{error}</p>}
            <ul className="admin-integration-list">
                {statuses.map((s) => (
                    <li key={s.name}>
                        <strong>{s.name}</strong>: {friendlyStatus(s.status)}
                        {s.detail && <span className="admin-muted"> — {s.detail}</span>}
                        <button type="button" onClick={() => healthCheck(s.name)}>Health check</button>
                    </li>
                ))}
            </ul>

            <h3>Failed deliveries</h3>
            {failed.length === 0 && <p className="admin-muted">No failed deliveries.</p>}
            <ul>
                {failed.map((d) => (
                    <li key={d.id}>
                        #{d.id} {d.notificationType} — {d.channel} — {d.status}
                        {d.safeFailureCode && <> ({d.safeFailureCode})</>}
                        <button type="button" onClick={() => retry(d.id)}>Retry</button>
                    </li>
                ))}
            </ul>
        </div>
    );
}
