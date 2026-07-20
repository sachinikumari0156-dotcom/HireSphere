import { useEffect, useState } from "react";
import api from "../../api/axios";

export default function AdminStoragePage() {
    const [statuses, setStatuses] = useState([]);
    const [dryRun, setDryRun] = useState(null);
    const [error, setError] = useState(null);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/admin/storage/status");
                if (alive) setStatuses(response.data || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load storage status.");
            }
        })();
        return () => { alive = false; };
    }, []);

    async function runDryRun() {
        setError(null);
        try {
            const response = await api.post("/admin/storage/migrations/dry-run");
            setDryRun(response.data);
        } catch (err) {
            setError(err.response?.data?.message || "Dry-run failed.");
        }
    }

    if (!statuses.length && !error) return <main className="admin-page"><p>Loading storage…</p></main>;
    if (error && !statuses.length) return <main className="admin-page"><p className="admin-error">{error}</p></main>;

    return (
        <main className="admin-page">
            <h2>Storage providers</h2>
            <p className="admin-muted">Azure Blob cloud is Not Configured without verified credentials. Local development storage may be Healthy. Antivirus remains Not Configured when no scanner is installed.</p>
            {error && <p className="admin-error">{error}</p>}
            <ul>
                {statuses.map((s) => (
                    <li key={s.name}>
                        <strong>{s.name}</strong>: {s.status}
                        {s.detail && <span className="admin-muted"> — {s.detail}</span>}
                        {typeof s.quarantinedDocumentCount === "number" && (
                            <span> · quarantined: {s.quarantinedDocumentCount}</span>
                        )}
                    </li>
                ))}
            </ul>
            <button type="button" onClick={runDryRun}>Run migration dry-run</button>
            {dryRun && (
                <pre className="admin-muted">{JSON.stringify(dryRun, null, 2)}</pre>
            )}
        </main>
    );
}
