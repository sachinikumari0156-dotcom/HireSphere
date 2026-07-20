import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

const empty = {
    requiredSkillsAlignment: 70,
    preferredSkillsAlignment: 70,
    relevantExperience: 70,
    educationRequirement: 70,
    assessmentPerformance: 70,
    interviewPerformance: 70,
    communication: 70,
    problemSolving: 70,
    roleReadiness: 70,
    strengths: "",
    weaknesses: "",
    documentedRisks: "",
    justification: "",
    recommendation: "",
    submit: false
};

export default function HiringManagerEvaluationPage() {
    const { id } = useParams();
    const [form, setForm] = useState(empty);
    const [status, setStatus] = useState("Draft");
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);
    const [loading, setLoading] = useState(true);
    const [dirty, setDirty] = useState(false);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/hiring-manager/applications/${id}/evaluation`);
                if (alive && response.data) {
                    setForm({ ...empty, ...response.data });
                    setStatus(response.data.submissionStatus || "Draft");
                }
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load evaluation.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    useEffect(() => {
        const handler = (e) => {
            if (dirty) {
                e.preventDefault();
                e.returnValue = "";
            }
        };
        window.addEventListener("beforeunload", handler);
        return () => window.removeEventListener("beforeunload", handler);
    }, [dirty]);

    function update(field, value) {
        setDirty(true);
        setForm((prev) => ({ ...prev, [field]: value }));
    }

    async function save(submit) {
        setError(null);
        setSuccess(null);
        if (submit && !window.confirm("Submit this evaluation?")) return;
        try {
            const payload = { ...form, submit };
            const method = form.id ? "put" : "post";
            const response = await api[method](`/hiring-manager/applications/${id}/evaluation`, payload);
            setForm({ ...empty, ...response.data });
            setStatus(response.data.submissionStatus);
            setSuccess(submit ? "Evaluation submitted." : "Draft saved.");
            setDirty(false);
        } catch (err) {
            setError(err.response?.data?.message || "Could not save evaluation.");
        }
    }

    if (loading) return <main className="hm-page"><p>Loading evaluation…</p></main>;

    return (
        <main className="hm-page">
            <h2>Candidate evaluation</h2>
            <p className="hm-muted">Status: {status}. Scores are advisory — they do not auto-decide hiring.</p>
            {error && <p className="hm-error" role="alert">{error}</p>}
            {success && <p className="hm-success" role="status">{success}</p>}
            <form
                className="hm-form"
                onSubmit={(e) => {
                    e.preventDefault();
                    save(false);
                }}
            >
                {[
                    ["requiredSkillsAlignment", "Required skills"],
                    ["preferredSkillsAlignment", "Preferred skills"],
                    ["relevantExperience", "Relevant experience"],
                    ["educationRequirement", "Education"],
                    ["assessmentPerformance", "Assessment"],
                    ["interviewPerformance", "Interview"],
                    ["communication", "Communication"],
                    ["problemSolving", "Problem solving"],
                    ["roleReadiness", "Role readiness"]
                ].map(([key, label]) => (
                    <label key={key}>
                        {label} (0–100)
                        <input
                            type="number"
                            min="0"
                            max="100"
                            value={form[key]}
                            onChange={(e) => update(key, Number(e.target.value))}
                        />
                    </label>
                ))}
                <label>
                    Justification
                    <textarea value={form.justification || ""} onChange={(e) => update("justification", e.target.value)} />
                </label>
                <label>
                    Recommendation
                    <input value={form.recommendation || ""} onChange={(e) => update("recommendation", e.target.value)} />
                </label>
                <button type="submit" className="hm-btn secondary">Save draft</button>
                <button type="button" className="hm-btn" onClick={() => save(true)}>Submit evaluation</button>
            </form>
            <Link className="hm-btn secondary" to={`/hiring-manager/applications/${id}`}>Back to review</Link>
        </main>
    );
}
