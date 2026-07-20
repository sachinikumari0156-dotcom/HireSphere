import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

export default function CandidateAssessmentDetailPage() {
    const { id } = useParams();
    const [assignment, setAssignment] = useState(null);
    const [attempt, setAttempt] = useState(null);
    const [answers, setAnswers] = useState({});
    const [error, setError] = useState(null);
    const [actionError, setActionError] = useState(null);
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);

    async function load() {
        const response = await api.get(`/candidate/assessments/${id}`);
        setAssignment(response.data);
        const attemptId = response.data.activeAttemptId || response.data.latestAttemptId;
        if (attemptId) {
            const attemptResponse = await api.get(`/candidate/assessments/attempts/${attemptId}`);
            setAttempt(attemptResponse.data);
            const map = {};
            (attemptResponse.data.answers || []).forEach((a) => {
                map[a.questionId] = a.answerValue;
            });
            setAnswers(map);
        } else {
            setAttempt(null);
            setAnswers({});
        }
    }

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                await load();
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load assessment.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [id]);

    async function start() {
        setActionError(null);
        setBusy(true);
        try {
            const response = await api.post(`/candidate/assessments/${id}/start`);
            setAttempt(response.data);
            await load();
        } catch (err) {
            setActionError(err.response?.data?.message || "Could not start assessment.");
        } finally {
            setBusy(false);
        }
    }

    async function saveAnswers() {
        if (!attempt) return;
        setActionError(null);
        setBusy(true);
        try {
            const payload = {
                answers: Object.entries(answers).map(([questionId, answerValue]) => ({
                    questionId: Number(questionId),
                    answerValue
                }))
            };
            const response = await api.put(
                `/candidate/assessments/attempts/${attempt.attemptId}/answers`,
                payload
            );
            setAttempt(response.data);
        } catch (err) {
            setActionError(err.response?.data?.message || "Could not save answers.");
        } finally {
            setBusy(false);
        }
    }

    async function submit() {
        if (!attempt) return;
        setActionError(null);
        setBusy(true);
        try {
            const payload = {
                answers: Object.entries(answers).map(([questionId, answerValue]) => ({
                    questionId: Number(questionId),
                    answerValue
                }))
            };
            await api.put(
                `/candidate/assessments/attempts/${attempt.attemptId}/answers`,
                payload
            );
            const response = await api.post(
                `/candidate/assessments/attempts/${attempt.attemptId}/submit`
            );
            setAttempt(response.data);
            await load();
        } catch (err) {
            setActionError(err.response?.data?.message || "Could not submit assessment.");
        } finally {
            setBusy(false);
        }
    }

    if (loading) {
        return <main className="dash-page"><p>Loading assessment…</p></main>;
    }

    if (error) {
        return <main className="dash-page"><p className="error">{error}</p></main>;
    }

    const inProgress = attempt?.status === "InProgress";
    const questions = attempt?.questions || assignment?.questions || [];

    return (
        <main className="dash-page">
            <header className="dash-header">
                <h1>{assignment.title}</h1>
                <p>
                    Status: {assignment.status}
                    {assignment.durationMinutes ? ` · ${assignment.durationMinutes} min` : ""}
                    {` · Attempts left: ${assignment.attemptsRemaining}`}
                </p>
                <nav className="dash-nav">
                    <Link to="/candidate/assessments">All assessments</Link>
                    {assignment.applicationId && (
                        <Link to={`/candidate/applications/${assignment.applicationId}`}>Application</Link>
                    )}
                </nav>
            </header>

            {assignment.description && (
                <section className="job-box">
                    <p>{assignment.description}</p>
                </section>
            )}

            {assignment.canStart && !inProgress && (
                <div className="wizard-actions">
                    <button type="button" onClick={start} disabled={busy}>
                        {busy ? "Starting…" : "Start assessment"}
                    </button>
                </div>
            )}

            {assignment.blockReason && !inProgress && (
                <p className="error">{assignment.blockReason}</p>
            )}

            {questions.length > 0 && (
                <section className="job-box">
                    <h2>{inProgress ? "Questions" : "Question preview"}</h2>
                    <ol>
                        {questions.map((q) => (
                            <li key={q.id} style={{ marginBottom: "1rem" }}>
                                <strong>{q.questionText}</strong>
                                <span> ({q.points} pts)</span>
                                {inProgress ? (
                                    q.options?.length > 0 ? (
                                        <div>
                                            {q.options.map((opt) => (
                                                <label key={opt} style={{ display: "block" }}>
                                                    <input
                                                        type="radio"
                                                        name={`q-${q.id}`}
                                                        value={opt}
                                                        checked={answers[q.id] === opt}
                                                        onChange={() =>
                                                            setAnswers((prev) => ({ ...prev, [q.id]: opt }))
                                                        }
                                                    />
                                                    {" "}{opt}
                                                </label>
                                            ))}
                                        </div>
                                    ) : (
                                        <input
                                            type="text"
                                            value={answers[q.id] || ""}
                                            onChange={(e) =>
                                                setAnswers((prev) => ({ ...prev, [q.id]: e.target.value }))
                                            }
                                            style={{ width: "100%", marginTop: "0.5rem" }}
                                        />
                                    )
                                ) : null}
                            </li>
                        ))}
                    </ol>
                </section>
            )}

            {inProgress && (
                <div className="wizard-actions">
                    <button type="button" onClick={saveAnswers} disabled={busy}>Save answers</button>
                    <button type="button" onClick={submit} disabled={busy}>
                        {busy ? "Submitting…" : "Submit assessment"}
                    </button>
                </div>
            )}

            {attempt?.resultsVisible && attempt.result && (
                <section className="job-box">
                    <h2>Result</h2>
                    <p>
                        Score: {attempt.result.score}/{attempt.result.maxScore}
                        {" "}({attempt.result.scorePercent}%) — {attempt.result.passed ? "Passed" : "Not passed"}
                    </p>
                    {attempt.result.feedback && <p>{attempt.result.feedback}</p>}
                </section>
            )}

            {attempt && !attempt.resultsVisible && attempt.status === "Completed" && (
                <p className="empty-state">Your attempt was submitted. Results are not visible for this assessment.</p>
            )}

            {actionError && <p className="error">{actionError}</p>}
        </main>
    );
}
