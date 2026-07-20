import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

export default function CandidateApplyPage() {
    const { id } = useParams();
    const navigate = useNavigate();
    const [options, setOptions] = useState(null);
    const [error, setError] = useState(null);
    const [submitError, setSubmitError] = useState(null);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [step, setStep] = useState(1);

    const [resumeId, setResumeId] = useState("");
    const [coverLetter, setCoverLetter] = useState("");
    const [answers, setAnswers] = useState({});
    const [termsAccepted, setTermsAccepted] = useState(false);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/candidate/jobs/${id}/apply-options`);
                if (!alive) return;
                setOptions(response.data);
                const primary = response.data.resumes?.find((r) => r.isPrimary)
                    || response.data.resumes?.[0];
                if (primary) setResumeId(String(primary.id));
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load application options.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    async function submit(event) {
        event.preventDefault();
        setSubmitError(null);
        setSubmitting(true);
        try {
            const screeningAnswers = (options.screeningQuestions || []).map((q) => ({
                screeningQuestionId: q.id,
                answerText: answers[q.id] || ""
            }));

            const response = await api.post("/candidate/applications", {
                jobId: Number(id),
                resumeId: resumeId ? Number(resumeId) : null,
                coverLetter,
                termsAccepted,
                screeningAnswers
            });

            navigate(`/candidate/applications/${response.data.id}`);
        } catch (err) {
            setSubmitError(err.response?.data?.message || "Could not submit application.");
        } finally {
            setSubmitting(false);
        }
    }

    if (loading) {
        return <main className="dash-page"><p>Loading application wizard…</p></main>;
    }

    if (error) {
        return (
            <main className="dash-page">
                <p className="error">{error}</p>
                <Link to={`/candidate/jobs/${id}`}>Back to job</Link>
            </main>
        );
    }

    if (!options.canApply) {
        return (
            <main className="dash-page">
                <h1>Apply — {options.jobTitle}</h1>
                <p className="error">{options.blockReason}</p>
                <Link to={`/candidate/jobs/${id}`}>Back to job</Link>
            </main>
        );
    }

    return (
        <main className="dash-page">
            <header className="dash-header">
                <h1>Apply — {options.jobTitle}</h1>
                <p>Step {step} of 3</p>
                <nav className="dash-nav">
                    <Link to={`/candidate/jobs/${id}`}>Cancel</Link>
                </nav>
            </header>

            <form className="profile-form" onSubmit={submit}>
                {step === 1 && (
                    <>
                        <label>
                            Resume
                            <select value={resumeId} onChange={(e) => setResumeId(e.target.value)}>
                                <option value="">No resume selected</option>
                                {(options.resumes || []).map((r) => (
                                    <option key={r.id} value={r.id}>
                                        {r.fileName}{r.isPrimary ? " (primary)" : ""}
                                    </option>
                                ))}
                            </select>
                        </label>
                        <label>
                            Cover letter
                            <textarea
                                rows={6}
                                value={coverLetter}
                                onChange={(e) => setCoverLetter(e.target.value)}
                                placeholder="Optional cover letter"
                            />
                        </label>
                        <button type="button" onClick={() => setStep(2)}>Continue</button>
                    </>
                )}

                {step === 2 && (
                    <>
                        {(options.screeningQuestions || []).length === 0 && (
                            <p className="empty-state">No screening questions for this role.</p>
                        )}
                        {(options.screeningQuestions || []).map((q) => (
                            <label key={q.id}>
                                {q.questionText}{q.isRequired ? " *" : ""}
                                <input
                                    value={answers[q.id] || ""}
                                    onChange={(e) => setAnswers((prev) => ({ ...prev, [q.id]: e.target.value }))}
                                    required={q.isRequired}
                                />
                            </label>
                        ))}
                        <div className="wizard-actions">
                            <button type="button" onClick={() => setStep(1)}>Back</button>
                            <button type="button" onClick={() => setStep(3)}>Continue</button>
                        </div>
                    </>
                )}

                {step === 3 && (
                    <>
                        <label className="checkbox-row">
                            <input
                                type="checkbox"
                                checked={termsAccepted}
                                onChange={(e) => setTermsAccepted(e.target.checked)}
                            />
                            I confirm my answers are accurate and accept the application terms.
                        </label>
                        {submitError && <p className="error">{submitError}</p>}
                        <div className="wizard-actions">
                            <button type="button" onClick={() => setStep(2)}>Back</button>
                            <button type="submit" disabled={submitting || !termsAccepted}>
                                {submitting ? "Submitting…" : "Submit application"}
                            </button>
                        </div>
                    </>
                )}
            </form>
        </main>
    );
}
