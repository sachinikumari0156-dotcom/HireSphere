import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";

export default function AdminHome() {
    const [data, setData] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/admin/dashboard");
                if (alive) setData(response.data);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load dashboard.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    if (loading) return <div className="admin-page"><p>Loading administrator dashboard…</p></div>;
    if (error) return <div className="admin-page"><p className="admin-error" role="alert">{error}</p></div>;

    return (
        <div className="admin-page">
            <h2>Dashboard</h2>
            <p className="admin-muted">Live LocalDB metrics for governance scope.</p>
            <section className="admin-stats" aria-label="Administrator metrics">
                <article><h3>Active users</h3><p>{data.activeUsers}</p></article>
                <article><h3>Disabled users</h3><p>{data.disabledUsers}</p></article>
                <article><h3>Pending recruiter requests</h3><p>{data.pendingRecruiterRequests}</p></article>
                <article><h3>Candidates</h3><p>{data.candidates}</p></article>
                <article><h3>Recruiters</h3><p>{data.recruiters}</p></article>
                <article><h3>Hiring Managers</h3><p>{data.hiringManagers}</p></article>
                <article><h3>Administrators</h3><p>{data.administrators}</p></article>
                <article><h3>Organizations</h3><p>{data.organizations}</p></article>
                <article><h3>Departments</h3><p>{data.departments}</p></article>
                <article><h3>Active jobs</h3><p>{data.activeJobs}</p></article>
                <article><h3>Applications</h3><p>{data.applications}</p></article>
                <article><h3>Pending final decisions</h3><p>{data.pendingFinalDecisions}</p></article>
                <article><h3>Upcoming interviews</h3><p>{data.upcomingInterviews}</p></article>
            </section>
            <h3>Recent audit</h3>
            {(!data.recentAuditEvents || data.recentAuditEvents.length === 0) && (
                <p className="admin-muted">No recent audit events.</p>
            )}
            <ul>
                {(data.recentAuditEvents || []).map((a) => (
                    <li key={a.id}>{a.action} · {a.entityType} · {a.createdAtUtc}</li>
                ))}
            </ul>
            <Link className="admin-btn" to="/admin/users">Manage users</Link>
        </div>
    );
}
