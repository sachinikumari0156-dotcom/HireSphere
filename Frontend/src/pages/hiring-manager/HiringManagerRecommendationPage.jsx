import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

export default function HiringManagerRecommendationPage() {
    const { id } = useParams();
    const [decisionType, setDecisionType] = useState("RecommendHire");
    const [reason, setReason] = useState("");
    const [notes, setNotes] = useState("");
    const [history, setHistory] = useState([]);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/hiring-manager/applications/${id}/decision-history`);
                if (alive) setHistory(response.data || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load decision history.");
            }
        })();
        return () => { alive = false; };
    }, [id]);

    async function reloadHistory() {
        const response = await api.get(`/hiring-manager/applications/${id}/decision-history`);
        setHistory(response.data || []);
    }
    async function submit(e) {
        e.preventDefault();
        if (!window.confirm(`Submit ${decisionType}?`)) return;
        setError(null);
        setSuccess(null);
        try {
            const response = await api.post(`/hiring-manager/applications/${id}/recommendation`, {
                decisionType,
                reason,
                notes
            });
            setSuccess(`Recorded ${response.data.decisionType}. Final=${response.data.isFinal}`);
            setReason("");
            await reloadHistory();
        } catch (err) {
            setError(err.response?.data?.message || "Could not submit recommendation.");
        }
    }

    return (
        <main className="hm-page">
            <h2>Recommendation</h2>
            <p className="hm-muted">
                Hiring Managers submit recommendations. FinalHire/FinalReject require Recruiter or Administrator.
            </p>
            {error && <p className="hm-error" role="alert">{error}</p>}
            {success && <p className="hm-success" role="status">{success}</p>}
            <form className="hm-form" onSubmit={submit}>
                <label>
                    Decision type
                    <select value={decisionType} onChange={(e) => setDecisionType(e.target.value)}>
                        <option>RecommendHire</option>
                        <option>RecommendReject</option>
                        <option>Hold</option>
                        <option>RequestAdditionalInterview</option>
                        <option>RequestAdditionalAssessment</option>
                    </select>
                </label>
                <label>
                    Reason
                    <textarea required value={reason} onChange={(e) => setReason(e.target.value)} />
                </label>
                <label>
                    Notes
                    <textarea value={notes} onChange={(e) => setNotes(e.target.value)} />
                </label>
                <button type="submit" className="hm-btn">Submit recommendation</button>
            </form>
            <h3>Decision history</h3>
            {history.length === 0 && <p className="hm-muted">No decisions yet.</p>}
            <ul>
                {history.map((h) => (
                    <li key={h.id}>
                        {h.decisionType} · final={String(h.isFinal)} · {h.reason} · {h.decisionDateUtc}
                    </li>
                ))}
            </ul>
            <Link className="hm-btn secondary" to={`/hiring-manager/applications/${id}`}>Back</Link>
        </main>
    );
}
