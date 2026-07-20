import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";

export default function RecruiterScreeningPage() {
    const [items, setItems] = useState([]);
    const [error, setError] = useState(null);
    const [message, setMessage] = useState(null);
    const [confirm, setConfirm] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/recruiter/screening-queue");
                if (alive) setItems(response.data || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load screening queue.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    async function applyDecision() {
        if (!confirm) return;
        setError(null);
        setMessage(null);
        try {
            await api.post(`/recruiter/applications/${confirm.id}/screening-decision`, {
                status: confirm.status,
                reason: confirm.reason
            });
            setMessage(`Decision recorded: ${confirm.status}`);
            setConfirm(null);
            const response = await api.get("/recruiter/screening-queue");
            setItems(response.data || []);
        } catch (err) {
            setError(err.response?.data?.message || "Decision failed.");
        }
    }

    return (
        <div className="rec-page">
            <h2>Screening queue</h2>
            <p className="rec-muted">Review answers and record Pass / Fail / ManualReview / Shortlist / Reject with a reason.</p>
            {loading && <p>Loading…</p>}
            {error && <p className="rec-error" role="alert">{error}</p>}
            {message && <p className="rec-success" role="status">{message}</p>}

            {confirm && (
                <div className="rec-notice" role="dialog" aria-label="Confirm screening decision">
                    <p>Confirm {confirm.status} for application #{confirm.id}?</p>
                    <label>
                        Reason
                        <input
                            value={confirm.reason}
                            onChange={(e) => setConfirm({ ...confirm, reason: e.target.value })}
                            required
                        />
                    </label>
                    <div className="rec-actions">
                        <button type="button" className="rec-btn" onClick={applyDecision}>Confirm decision</button>
                        <button type="button" className="rec-btn secondary" onClick={() => setConfirm(null)}>Cancel</button>
                    </div>
                </div>
            )}

            {!loading && items.length === 0 && <p className="rec-muted">Screening queue is empty.</p>}
            <ul className="rec-activity">
                {items.map((item) => (
                    <li key={item.applicationId}>
                        <strong>{item.candidateName}</strong> · {item.jobTitle} · {item.status}
                        <div className="rec-muted">
                            Required answers {item.requiredAnswersCompleted}/{item.requiredAnswersTotal}
                        </div>
                        <div className="rec-actions">
                            <Link to={`/recruiter/applications/${item.applicationId}`}>Open</Link>
                            <button
                                type="button"
                                className="rec-btn secondary"
                                onClick={() => setConfirm({
                                    id: item.applicationId,
                                    status: "Shortlisted",
                                    reason: "Screening passed"
                                })}
                            >
                                Shortlist
                            </button>
                            <button
                                type="button"
                                className="rec-btn danger"
                                onClick={() => setConfirm({
                                    id: item.applicationId,
                                    status: "Rejected",
                                    reason: "Screening failed"
                                })}
                            >
                                Reject
                            </button>
                        </div>
                    </li>
                ))}
            </ul>
        </div>
    );
}
