import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

export default function CandidateApplicationDetailPage() {
    const { id } = useParams();
    const [app, setApp] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);
    const [actionError, setActionError] = useState(null);
    const [withdrawing, setWithdrawing] = useState(false);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/candidate/applications/${id}`);
                if (alive) setApp(response.data);
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load application.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    async function withdraw() {
        setActionError(null);
        setWithdrawing(true);
        try {
            const response = await api.post(`/candidate/applications/${id}/withdraw`);
            setApp(response.data);
        } catch (err) {
            setActionError(err.response?.data?.message || "Could not withdraw application.");
        } finally {
            setWithdrawing(false);
        }
    }

    if (loading) {
        return <div className="dash-page"><p>Loading application…</p></div>;
    }

    if (error) {
        return <div className="dash-page"><p className="error">{error}</p></div>;
    }

    return (
        <div className="dash-page">
            <header className="dash-header">
                <h1>{app.jobTitle}</h1>
                <p>Status: {app.status} · Submitted {new Date(app.submittedAtUtc).toLocaleString()}</p>
                <nav className="dash-nav">
                    <Link to="/candidate/applications">All applications</Link>
                    <Link to={`/candidate/jobs/${app.jobId}`}>View job</Link>
                </nav>
            </header>

            <section className="job-box">
                <h2>Tracking</h2>
                <p>Next action: {app.nextAction || "—"}</p>
                {app.latestUpdateAtUtc && (
                    <p>
                        Latest update: {new Date(app.latestUpdateAtUtc).toLocaleString()}
                        {app.latestUpdateNotes ? ` — ${app.latestUpdateNotes}` : ""}
                    </p>
                )}
            </section>

            <section className="job-box">
                <h2>Cover letter</h2>
                <p style={{ whiteSpace: "pre-wrap" }}>{app.coverLetter || "—"}</p>
                {app.resumeFileName && <p>Resume: {app.resumeFileName}</p>}
            </section>

            {app.answers?.length > 0 && (
                <section className="job-box">
                    <h2>Screening answers</h2>
                    <ul>
                        {app.answers.map((a) => (
                            <li key={a.id}>
                                <strong>{a.questionText}</strong>: {a.answerText}
                            </li>
                        ))}
                    </ul>
                </section>
            )}

            <section className="job-box">
                <h2>Status timeline</h2>
                {(app.statusHistory || []).length === 0 ? (
                    <p className="empty-state">No status history yet.</p>
                ) : (
                    <ul>
                        {(app.statusHistory || []).map((h, index) => (
                            <li key={`${h.status}-${h.changedAtUtc}-${index}`}>
                                {h.status} — {new Date(h.changedAtUtc).toLocaleString()}
                                {h.notes ? ` (${h.notes})` : ""}
                            </li>
                        ))}
                    </ul>
                )}
            </section>

            {app.interviews?.length > 0 && (
                <section className="job-box">
                    <h2>Interviews</h2>
                    <ul>
                        {app.interviews.map((i) => (
                            <li key={i.interviewId}>
                                <Link to={`/candidate/interviews/${i.interviewId}`}>
                                    {new Date(i.interviewDateUtc).toLocaleString()} ({i.timeZoneId})
                                </Link>
                                {" — "}{i.status} / {i.candidateResponse}
                            </li>
                        ))}
                    </ul>
                </section>
            )}

            {app.assessments?.length > 0 && (
                <section className="job-box">
                    <h2>Assessments</h2>
                    <ul>
                        {app.assessments.map((a) => (
                            <li key={a.assignmentId}>
                                <Link to={`/candidate/assessments/${a.assignmentId}`}>{a.title}</Link>
                                {" — "}{a.status} · {a.attemptsRemaining} attempts left
                            </li>
                        ))}
                    </ul>
                </section>
            )}

            {app.canWithdraw && (
                <div className="wizard-actions">
                    <button type="button" onClick={withdraw} disabled={withdrawing}>
                        {withdrawing ? "Withdrawing…" : "Withdraw application"}
                    </button>
                </div>
            )}
            {actionError && <p className="error">{actionError}</p>}
        </div>
    );
}
