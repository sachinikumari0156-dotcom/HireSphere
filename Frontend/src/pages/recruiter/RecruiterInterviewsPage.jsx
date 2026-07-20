import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";

export default function RecruiterInterviewsPage() {
    const [items, setItems] = useState([]);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/recruiter/interviews");
                if (alive) setItems(response.data || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load interviews.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    return (
        <main className="rec-page">
            <h2>Interviews</h2>
            <p className="rec-muted">Calendar sync: Not Configured (Google/Outlook deferred to Phase 8).</p>
            <div className="rec-actions">
                <Link className="rec-btn" to="/recruiter/interviews/schedule">Schedule interview</Link>
            </div>
            {loading && <p>Loading…</p>}
            {error && <p className="rec-error">{error}</p>}
            {!loading && items.length === 0 && <p className="rec-muted">No interviews scheduled.</p>}
            <ul className="rec-activity">
                {items.map((item) => (
                    <li key={item.id}>
                        <Link to={`/recruiter/interviews/${item.id}`}>
                            {item.candidateName} · {item.jobTitle}
                        </Link>
                        <div className="rec-muted">
                            {new Date(item.startAtUtc).toLocaleString()} ({item.timeZoneId}) · {item.status}
                            {" · "}Candidate: {item.candidateResponse}
                        </div>
                    </li>
                ))}
            </ul>
        </main>
    );
}
