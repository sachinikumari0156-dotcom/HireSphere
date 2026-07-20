import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import api from "../../api/axios";

export default function RecruiterAssessmentBuilderPage() {
    const { id } = useParams();
    const isNew = !id || id === "new";
    const navigate = useNavigate();
    const [title, setTitle] = useState("");
    const [passingScorePercent, setPassing] = useState(60);
    const [maxAttempts, setMaxAttempts] = useState(1);
    const [durationMinutes, setDuration] = useState(30);
    const [questions, setQuestions] = useState([]);
    const [qText, setQText] = useState("");
    const [qKey, setQKey] = useState("");
    const [error, setError] = useState(null);
    const [message, setMessage] = useState(null);
    const [loading, setLoading] = useState(!isNew);

    useEffect(() => {
        if (isNew) return undefined;
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/recruiter/assessments/${id}`);
                if (!alive) return;
                setTitle(response.data.title || "");
                setPassing(response.data.passingScorePercent ?? 60);
                setMaxAttempts(response.data.maxAttempts ?? 1);
                setDuration(response.data.durationMinutes ?? 30);
                setQuestions(response.data.questions || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load assessment.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id, isNew]);

    async function saveAssessment(e) {
        e.preventDefault();
        setError(null);
        if (!title.trim()) {
            setError("Title is required.");
            return;
        }
        if (Number(passingScorePercent) < 0 || Number(passingScorePercent) > 100) {
            setError("Passing score must be between 0 and 100.");
            return;
        }
        try {
            const payload = {
                title: title.trim(),
                passingScorePercent: Number(passingScorePercent),
                maxAttempts: Number(maxAttempts),
                durationMinutes: Number(durationMinutes) || null,
                revealResultsToCandidate: true
            };
            if (isNew) {
                const response = await api.post("/recruiter/assessments", payload);
                setMessage("Assessment created.");
                navigate(`/recruiter/assessments/${response.data.id}`);
            } else {
                await api.put(`/recruiter/assessments/${id}`, payload);
                setMessage("Assessment updated.");
            }
        } catch (err) {
            setError(err.response?.data?.message || "Save failed.");
        }
    }

    async function addQuestion(e) {
        e.preventDefault();
        setError(null);
        if (!qText.trim() || !qKey.trim()) {
            setError("Question text and correct answer key are required.");
            return;
        }
        try {
            const response = await api.post(`/recruiter/assessments/${id}/questions`, {
                questionText: qText.trim(),
                questionType: "ShortAnswer",
                points: 10,
                sortOrder: questions.length,
                correctAnswerKey: qKey.trim()
            });
            setQuestions((prev) => [...prev, response.data]);
            setQText("");
            setQKey("");
            setMessage("Question added.");
        } catch (err) {
            setError(err.response?.data?.message || "Could not add question.");
        }
    }

    if (loading) return <div className="rec-page"><p>Loading assessment builder…</p></div>;

    return (
        <div className="rec-page">
            <h2>{isNew ? "Create assessment" : "Assessment builder"}</h2>
            <p className="rec-muted">Answer keys are recruiter-only and never rendered in Candidate views.</p>
            {error && <p className="rec-error" role="alert">{error}</p>}
            {message && <p className="rec-success" role="status">{message}</p>}

            <form className="rec-form" onSubmit={saveAssessment}>
                <label>
                    Title
                    <input value={title} onChange={(e) => setTitle(e.target.value)} />
                </label>
                <div className="rec-form-grid">
                    <label>
                        Passing score %
                        <input type="number" value={passingScorePercent} onChange={(e) => setPassing(e.target.value)} />
                    </label>
                    <label>
                        Max attempts
                        <input type="number" min="1" value={maxAttempts} onChange={(e) => setMaxAttempts(e.target.value)} />
                    </label>
                    <label>
                        Duration (minutes)
                        <input type="number" value={durationMinutes} onChange={(e) => setDuration(e.target.value)} />
                    </label>
                </div>
                <button type="submit" className="rec-btn">{isNew ? "Create" : "Save"}</button>
            </form>

            {!isNew && (
                <section>
                    <h3>Questions</h3>
                    <ul className="rec-activity">
                        {questions.map((q) => (
                            <li key={q.id}>
                                {q.questionText}
                                <div className="rec-muted">Key (recruiter only): {q.correctAnswerKey}</div>
                            </li>
                        ))}
                    </ul>
                    <form className="rec-form" onSubmit={addQuestion}>
                        <label>
                            Question text
                            <input value={qText} onChange={(e) => setQText(e.target.value)} />
                        </label>
                        <label>
                            Correct answer key
                            <input value={qKey} onChange={(e) => setQKey(e.target.value)} />
                        </label>
                        <button type="submit" className="rec-btn secondary">Add question</button>
                    </form>
                </section>
            )}

            <div className="rec-actions">
                <Link className="rec-btn secondary" to="/recruiter/assessments">Back</Link>
            </div>
        </div>
    );
}
