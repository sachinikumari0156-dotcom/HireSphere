import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

export default function CandidateNotificationsPage() {
    const [data, setData] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);

    async function load() {
        const response = await api.get("/candidate/notifications");
        setData(response.data);
    }

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                await load();
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load notifications.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    async function markRead(id) {
        setBusy(true);
        try {
            await api.post(`/candidate/notifications/${id}/read`);
            await load();
        } finally {
            setBusy(false);
        }
    }

    async function markAll() {
        setBusy(true);
        try {
            const response = await api.post("/candidate/notifications/read-all");
            setData(response.data);
        } finally {
            setBusy(false);
        }
    }

    if (loading) {
        return <div className="dash-page"><p>Loading notifications…</p></div>;
    }

    if (error) {
        return <div className="dash-page"><p className="error">{error}</p></div>;
    }

    const items = data?.items || [];

    return (
        <div className="dash-page">
            <header className="dash-header">
                <h1>Notifications</h1>
                <p>{data?.unreadCount ?? 0} unread</p>
                <nav className="dash-nav">
                    <Link to="/candidate">Dashboard</Link>
                </nav>
            </header>

            {items.length > 0 && (
                <div className="wizard-actions">
                    <button type="button" onClick={markAll} disabled={busy || data.unreadCount === 0}>
                        Mark all read
                    </button>
                </div>
            )}

            {items.length === 0 ? (
                <p className="empty-state">No notifications yet.</p>
            ) : (
                <ul className="job-list">
                    {items.map((n) => (
                        <li key={n.id} className="job-box" style={{ opacity: n.isRead ? 0.75 : 1 }}>
                            <h2>{n.title}</h2>
                            <p>{n.message}</p>
                            <p>
                                {n.category} · {new Date(n.createdAtUtc).toLocaleString()}
                                {n.isRead ? " · Read" : " · Unread"}
                            </p>
                            <div className="wizard-actions">
                                {n.linkPath && <Link to={n.linkPath}>Open</Link>}
                                {!n.isRead && (
                                    <button type="button" onClick={() => markRead(n.id)} disabled={busy}>
                                        Mark read
                                    </button>
                                )}
                            </div>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
