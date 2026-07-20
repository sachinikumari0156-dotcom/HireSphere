import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

export default function RecruiterRankingPage() {
    const { id } = useParams();
    const [ranking, setRanking] = useState(null);
    const [error, setError] = useState(null);
    const [notes, setNotes] = useState("");
    const [decision, setDecision] = useState("Proceed");
    const [message, setMessage] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/recruiter/applications/${id}/ranking`);
                if (alive) setRanking(response.data);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load ranking.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    async function saveReview(e) {
        e.preventDefault();
        setError(null);
        setMessage(null);
        try {
            await api.post(`/recruiter/applications/${id}/ranking/review`, {
                decision,
                notes,
                overrideScore: null
            });
            setMessage("Human review saved.");
            const response = await api.get(`/recruiter/applications/${id}/ranking`);
            setRanking(response.data);
        } catch (err) {
            setError(err.response?.data?.message || "Could not save review.");
        }
    }

    if (loading) return <main className="rec-page"><p>Loading ranking…</p></main>;
    if (error && !ranking) return <main className="rec-page"><p className="rec-error">{error}</p></main>;

    return (
        <main className="rec-page">
            <h2>Candidate ranking</h2>
            <p className="rec-notice">{ranking?.humanReviewNotice}</p>
            {message && <p className="rec-success" role="status">{message}</p>}
            {error && <p className="rec-error" role="alert">{error}</p>}

            <section>
                <p><strong>Total score:</strong> {ranking?.totalScore}</p>
                <p><strong>Confidence:</strong> {ranking?.confidence}</p>
                <p><strong>Provider:</strong> {ranking?.providerName} ({ranking?.modelVersion})</p>
                <p data-testid="ranking-explanation">{ranking?.explanation}</p>
                <p>Matched required: {(ranking?.matchedRequiredSkills || []).join(", ") || "—"}</p>
                <p>Missing required: {(ranking?.missingRequiredSkills || []).join(", ") || "None"}</p>
            </section>

            <form className="rec-form" onSubmit={saveReview}>
                <label>
                    Human review decision
                    <input value={decision} onChange={(e) => setDecision(e.target.value)} required />
                </label>
                <label>
                    Review notes
                    <textarea rows={3} value={notes} onChange={(e) => setNotes(e.target.value)} required />
                </label>
                <button type="submit" className="rec-btn">Save human review</button>
            </form>

            <div className="rec-actions">
                <Link className="rec-btn secondary" to={`/recruiter/applications/${id}`}>Back to application</Link>
            </div>
        </main>
    );
}
