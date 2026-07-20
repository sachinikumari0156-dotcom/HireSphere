import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

export default function RecruiterApplicationPage() {
    const { id } = useParams();
    const [detail, setDetail] = useState(null);
    const [note, setNote] = useState("");
    const [error, setError] = useState(null);
    const [message, setMessage] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/recruiter/applications/${id}`);
                if (alive) setDetail(response.data);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load application.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    async function reload() {
        setError(null);
        try {
            const response = await api.get(`/recruiter/applications/${id}`);
            setDetail(response.data);
        } catch (err) {
            setError(err.response?.data?.message || "Could not load application.");
        }
    }

    async function addNote(e) {
        e.preventDefault();
        setError(null);
        setMessage(null);
        try {
            await api.post(`/recruiter/applications/${id}/notes`, { content: note });
            setNote("");
            setMessage("Internal note saved.");
            await reload();
        } catch (err) {
            setError(err.response?.data?.message || "Could not save note.");
        }
    }

    if (loading) return <main className="rec-page"><p>Loading application…</p></main>;
    if (error && !detail) return <main className="rec-page"><p className="rec-error">{error}</p></main>;
    if (!detail) return null;

    return (
        <main className="rec-page">
            <h2>{detail.candidateName}</h2>
            <p className="rec-muted">
                {detail.jobTitle} · {detail.status} · applied {new Date(detail.appliedAtUtc).toLocaleString()}
            </p>
            {message && <p className="rec-success" role="status">{message}</p>}
            {error && <p className="rec-error" role="alert">{error}</p>}

            <section>
                <h3>Profile</h3>
                <p>{detail.professionalSummary || "No summary provided."}</p>
                <p>Experience: {detail.yearsOfExperience ?? "—"} years</p>
                <p>Match score: {detail.matchScore ?? "—"}</p>
                <p>Resume: {detail.resumeFileName || "Not attached"}</p>
            </section>

            <section>
                <h3>Skills</h3>
                <p>{(detail.skills || []).join(", ") || "None listed"}</p>
                <p className="rec-muted">
                    Missing required: {(detail.missingRequiredSkills || []).join(", ") || "None"}
                </p>
            </section>

            <section>
                <h3>Screening answers</h3>
                {(detail.screeningAnswers?.length ?? 0) === 0 ? (
                    <p className="rec-muted">No answers.</p>
                ) : (
                    <ul>
                        {detail.screeningAnswers.map((a) => (
                            <li key={a.questionId}>
                                <strong>{a.questionText}</strong>
                                <div>{a.answerText || "—"}</div>
                            </li>
                        ))}
                    </ul>
                )}
            </section>

            <section>
                <h3>Status history</h3>
                <ul className="rec-activity">
                    {(detail.statusHistory || []).map((h, index) => (
                        <li key={`${h.status}-${h.changedAtUtc}-${index}`}>
                            {h.status} · {new Date(h.changedAtUtc).toLocaleString()}
                            {h.notes ? ` — ${h.notes}` : ""}
                        </li>
                    ))}
                </ul>
            </section>

            <section>
                <h3>Internal notes</h3>
                <p className="rec-muted">Not visible to candidates.</p>
                <ul className="rec-activity">
                    {(detail.internalNotes || []).map((n) => (
                        <li key={n.id}>
                            <strong>{n.authorName}</strong>: {n.content}
                            <div className="rec-muted">{new Date(n.createdAtUtc).toLocaleString()}</div>
                        </li>
                    ))}
                </ul>
                <form className="rec-form" onSubmit={addNote}>
                    <label>
                        Add note
                        <textarea
                            rows={3}
                            value={note}
                            onChange={(e) => setNote(e.target.value)}
                            required
                        />
                    </label>
                    <button type="submit" className="rec-btn">Save note</button>
                </form>
            </section>

            <div className="rec-actions">
                <Link className="rec-btn secondary" to={`/recruiter/jobs/${detail.jobId}/applicants`}>
                    Back to pipeline
                </Link>
            </div>
        </main>
    );
}
