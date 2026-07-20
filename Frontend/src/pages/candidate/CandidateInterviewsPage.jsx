import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

export default function CandidateInterviewsPage() {
    const [items, setItems] = useState([]);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/candidate/interviews");
                if (alive) setItems(response.data || []);
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load interviews.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    if (loading) {
        return <div className="dash-page"><p>Loading interviews…</p></div>;
    }

    if (error) {
        return <div className="dash-page"><p className="error">{error}</p></div>;
    }

    return (
        <div className="dash-page">
            <header className="dash-header">
                <h1>Interviews</h1>
                <p>Scheduled interviews for your applications.</p>
                <nav className="dash-nav">
                    <Link to="/candidate">Dashboard</Link>
                    <Link to="/candidate/applications">Applications</Link>
                </nav>
            </header>

            {items.length === 0 ? (
                <p className="empty-state">No interviews scheduled yet.</p>
            ) : (
                <ul className="job-list">
                    {items.map((i) => (
                        <li key={i.id} className="job-box">
                            <h2>
                                <Link to={`/candidate/interviews/${i.id}`}>{i.jobTitle}</Link>
                            </h2>
                            <p>
                                {new Date(i.interviewDateUtc).toLocaleString()} ({i.timeZoneId})
                            </p>
                            <p>
                                {i.interviewType} · {i.status} · Response: {i.candidateResponse}
                            </p>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
