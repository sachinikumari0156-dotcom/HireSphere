import { useEffect, useState } from "react";
import api from "../../api/axios";

export default function AdminMonitoringPage() {
    const [data, setData] = useState(null);
    const [error, setError] = useState(null);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/admin/monitoring/summary");
                if (alive) setData(response.data);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load monitoring.");
            }
        })();
        return () => { alive = false; };
    }, []);

    if (!data && !error) return <main className="admin-page"><p>Loading monitoring…</p></main>;
    if (error) return <main className="admin-page"><p className="admin-error">{error}</p></main>;

    return (
        <main className="admin-page">
            <h2>Monitoring</h2>
            <section className="admin-stats" aria-label="Monitoring summary">
                <article><h3>API health</h3><p>{data.apiHealth}</p></article>
                <article><h3>Database</h3><p>{data.databaseConnectivity}</p></article>
                <article><h3>Pending recruiter requests</h3><p>{data.pendingRecruiterRequests}</p></article>
                <article><h3>Disabled accounts</h3><p>{data.disabledAccounts}</p></article>
                <article><h3>Pending assessments</h3><p>{data.pendingAssessments}</p></article>
                <article><h3>Upcoming interviews</h3><p>{data.upcomingInterviews}</p></article>
                <article><h3>Pending final decisions</h3><p>{data.pendingFinalDecisions}</p></article>
            </section>
            <h3>Phase 8 providers</h3>
            <ul>
                <li>Email: {data.emailProviderStatus}</li>
                <li>SMS: {data.smsProviderStatus}</li>
                <li>Calendar: {data.calendarProviderStatus}</li>
                <li>Storage: {data.storageProviderStatus}</li>
            </ul>
            <p className="admin-muted">{data.providerNotes}</p>
        </main>
    );
}
