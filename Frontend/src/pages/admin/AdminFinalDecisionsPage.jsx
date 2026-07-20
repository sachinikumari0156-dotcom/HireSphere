import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

export default function AdminFinalDecisionsPage() {
    const [items, setItems] = useState([]);
    const [error, setError] = useState(null);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/admin/final-decisions/pending");
                if (alive) setItems(response.data || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load pending decisions.");
            }
        })();
        return () => { alive = false; };
    }, []);

    return (
        <div className="admin-page">
            <h2>Pending final decisions</h2>
            {error && <p className="admin-error" role="alert">{error}</p>}
            {items.length === 0 && <p className="admin-muted">No pending recommendations awaiting final decision.</p>}
            <ul>
                {items.map((i) => (
                    <li key={i.applicationId}>
                        {i.candidateName} · {i.jobTitle} · {i.latestRecommendation}
                        {" "}
                        <Link to={`/admin/final-decisions/${i.applicationId}`}>Review</Link>
                    </li>
                ))}
            </ul>
        </div>
    );
}

export function AdminFinalDecisionDetailPage() {
    const { applicationId } = useParams();
    const [detail, setDetail] = useState(null);
    const [reason, setReason] = useState("");
    const [decisionType, setDecisionType] = useState("FinalHire");
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/admin/final-decisions/${applicationId}`);
                if (alive) setDetail(response.data);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load.");
            }
        })();
        return () => { alive = false; };
    }, [applicationId]);

    async function submit(e) {
        e.preventDefault();
        if (!window.confirm(`Record ${decisionType}?`)) return;
        setError(null);
        setSuccess(null);
        try {
            const response = await api.post(`/admin/final-decisions/${applicationId}`, {
                decisionType,
                reason
            });
            setSuccess(`Recorded ${response.data.decisionType}. Final=${response.data.isFinal}`);
            const refreshed = await api.get(`/admin/final-decisions/${applicationId}`);
            setDetail(refreshed.data);
        } catch (err) {
            setError(err.response?.data?.message || "Decision failed.");
        }
    }

    if (!detail && !error) return <div className="admin-page"><p>Loading…</p></div>;

    return (
        <div className="admin-page">
            <h2>Final decision review</h2>
            {detail && (
                <>
                    <p>{detail.candidateName} · {detail.jobTitle} · {detail.applicationStatus}</p>
                    <p>Recommendation: {detail.latestRecommendation || "—"}</p>
                    <p>{detail.recommendationReason}</p>
                    {(detail.warnings || []).map((w) => (
                        <p key={w} className="admin-error">{w}</p>
                    ))}
                </>
            )}
            {error && <p className="admin-error" role="alert">{error}</p>}
            {success && <p className="admin-success" role="status">{success}</p>}
            <form className="admin-form" onSubmit={submit}>
                <label>
                    Decision type
                    <select value={decisionType} onChange={(e) => setDecisionType(e.target.value)}>
                        <option>FinalHire</option>
                        <option>FinalReject</option>
                        <option>Hold</option>
                        <option>RequestAdditionalInterview</option>
                        <option>RequestAdditionalAssessment</option>
                    </select>
                </label>
                <label>
                    Reason
                    <textarea required value={reason} onChange={(e) => setReason(e.target.value)} />
                </label>
                <button type="submit" className="admin-btn">Record decision</button>
            </form>
            <Link className="admin-btn secondary" to="/admin/final-decisions">Back</Link>
        </div>
    );
}
