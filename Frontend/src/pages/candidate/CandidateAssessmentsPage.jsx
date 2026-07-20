import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

export default function CandidateAssessmentsPage() {
    const [items, setItems] = useState([]);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/candidate/assessments");
                if (alive) setItems(response.data || []);
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load assessments.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    if (loading) {
        return <div className="dash-page"><p>Loading assessments…</p></div>;
    }

    if (error) {
        return <div className="dash-page"><p className="error">{error}</p></div>;
    }

    return (
        <div className="dash-page">
            <header className="dash-header">
                <h1>Skill assessments</h1>
                <p>Assigned assessments for your applications.</p>
                <nav className="dash-nav">
                    <Link to="/candidate">Dashboard</Link>
                    <Link to="/candidate/applications">Applications</Link>
                </nav>
            </header>

            {items.length === 0 ? (
                <p className="empty-state">No assessments assigned yet.</p>
            ) : (
                <ul className="job-list">
                    {items.map((a) => (
                        <li key={a.assignmentId} className="job-box">
                            <h2>
                                <Link to={`/candidate/assessments/${a.assignmentId}`}>{a.title}</Link>
                            </h2>
                            <p>
                                Status: {a.status}
                                {a.jobTitle ? ` · ${a.jobTitle}` : ""}
                            </p>
                            <p>
                                Attempts: {a.attemptsUsed}/{a.maxAttempts}
                                {a.expiresAtUtc
                                    ? ` · Expires ${new Date(a.expiresAtUtc).toLocaleString()}`
                                    : ""}
                            </p>
                            {a.blockReason && <p className="error">{a.blockReason}</p>}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
