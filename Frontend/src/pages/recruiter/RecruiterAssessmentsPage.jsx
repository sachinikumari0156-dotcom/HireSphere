import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";

export default function RecruiterAssessmentsPage() {
    const [items, setItems] = useState([]);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/recruiter/assessments");
                if (alive) setItems(response.data || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load assessments.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    return (
        <main className="rec-page">
            <h2>Assessments</h2>
            <div className="rec-actions">
                <Link className="rec-btn" to="/recruiter/assessments/new">Create assessment</Link>
            </div>
            {loading && <p>Loading…</p>}
            {error && <p className="rec-error">{error}</p>}
            {!loading && items.length === 0 && <p className="rec-muted">No assessments yet.</p>}
            <ul className="rec-activity">
                {items.map((item) => (
                    <li key={item.id}>
                        <Link to={`/recruiter/assessments/${item.id}`}>{item.title}</Link>
                        {" · "}
                        {item.questionCount} questions
                        {item.isArchived ? " · archived" : ""}
                    </li>
                ))}
            </ul>
        </main>
    );
}
