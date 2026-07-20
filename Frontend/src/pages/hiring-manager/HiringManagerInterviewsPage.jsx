import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";

export default function HiringManagerInterviewsPage() {
    const [items, setItems] = useState([]);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/hiring-manager/interviews");
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
        <div className="hm-page">
            <h2>Interviews</h2>
            <p className="hm-muted">Interviews where you are assigned Hiring Manager or participant.</p>
            {error && <p className="hm-error" role="alert">{error}</p>}
            {loading && <p>Loading interviews…</p>}
            {!loading && items.length === 0 && <p className="hm-muted">No interviews assigned.</p>}
            <ul>
                {items.map((i) => (
                    <li key={i.id}>
                        <Link to={`/hiring-manager/interviews/${i.id}`}>
                            {i.candidateName} · {i.jobTitle} · {i.interviewDateUtc} ({i.timeZoneId})
                        </Link>
                    </li>
                ))}
            </ul>
        </div>
    );
}
