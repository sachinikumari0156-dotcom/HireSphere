import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

export default function CandidateApplicationsPage() {
    const [items, setItems] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/candidate/applications");
                if (alive) setItems(response.data);
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load applications.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    if (loading) {
        return <div className="dash-page"><p>Loading applications…</p></div>;
    }

    if (error) {
        return <div className="dash-page"><p className="error">{error}</p></div>;
    }

    return (
        <div className="dash-page">
            <header className="dash-header">
                <h1>My applications</h1>
                <nav className="dash-nav">
                    <Link to="/candidate">Dashboard</Link>
                    <Link to="/candidate/jobs">Browse jobs</Link>
                </nav>
            </header>

            {items?.length === 0 && (
                <p className="empty-state">You have not submitted any applications yet.</p>
            )}

            <section className="job-list">
                {(items || []).map((app) => (
                    <article key={app.id} className="job-box">
                        <h2>
                            <Link to={`/candidate/applications/${app.id}`}>{app.jobTitle}</Link>
                        </h2>
                        <p>
                            {app.jobLocation} · Status: {app.status}
                        </p>
                        <p>
                            Submitted: {new Date(app.submittedAtUtc || app.appliedDate).toLocaleString()}
                        </p>
                    </article>
                ))}
            </section>
        </div>
    );
}
