import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

export default function CandidateResumeAnalysisPage() {
    const { id } = useParams();
    const [analysis, setAnalysis] = useState(null);
    const [status, setStatus] = useState(null);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);
    const [selected, setSelected] = useState([]);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const [a, s] = await Promise.all([
                    api.get(`/candidate/resumes/${id}/analysis`).catch(() => ({ data: null })),
                    api.get("/candidate/ai/status")
                ]);
                if (!alive) return;
                setAnalysis(a.data);
                setStatus(s.data);
                if (a.data?.skills) {
                    setSelected(a.data.skills.filter((x) => x.status === "Pending").map((x) => x.id));
                }
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load analysis.");
            }
        })();
        return () => { alive = false; };
    }, [id]);

    async function parse() {
        setError(null);
        setSuccess(null);
        try {
            const response = await api.post(`/candidate/resumes/${id}/parse`);
            setAnalysis(response.data);
            setSelected((response.data.skills || []).filter((x) => x.status === "Pending").map((x) => x.id));
            setSuccess("Parsing completed. Review extracted skills before confirming.");
        } catch (err) {
            setError(err.response?.data?.message || "Parse failed.");
        }
    }

    async function toggleConsent(checked) {
        const response = await api.put("/candidate/ai/consent", { allowExternalAiProcessing: checked });
        setStatus(response.data);
    }

    async function confirm() {
        const pending = (analysis?.skills || []).filter((s) => s.status === "Pending");
        const acceptSkillIds = selected;
        const rejectSkillIds = pending.map((s) => s.id).filter((id) => !selected.includes(id));
        try {
            const response = await api.post(`/candidate/resumes/${id}/analysis/confirm`, {
                acceptSkillIds,
                rejectSkillIds
            });
            setAnalysis(response.data);
            setSuccess("Accepted skills were added to your profile. Rejected skills were not applied.");
        } catch (err) {
            setError(err.response?.data?.message || "Confirm failed.");
        }
    }

    function toggleSkill(skillId) {
        setSelected((prev) => (prev.includes(skillId) ? prev.filter((x) => x !== skillId) : [...prev, skillId]));
    }

    return (
        <div className="candidate-page">
            <h1>Resume analysis</h1>
            <p className="muted">
                AI-generated insight. Final recruitment decisions must be reviewed by authorized users.
            </p>
            {error && <p className="error" role="alert">{error}</p>}
            {success && <p className="success" role="status">{success}</p>}

            {status && (
                <section>
                    <h2>Provider status</h2>
                    <p>Deterministic: {status.deterministicProviderStatus}</p>
                    <p>External AI: {status.externalAiProviderStatus}</p>
                    <label>
                        <input
                            type="checkbox"
                            checked={!!status.allowExternalAiProcessing}
                            onChange={(e) => toggleConsent(e.target.checked)}
                        />
                        {" "}
                        Allow sending resume content to an external AI provider (when configured)
                    </label>
                    {status.externalAiProviderStatus === "NotConfigured" && (
                        <p className="muted">External AI is Not Configured. Deterministic parsing will be used.</p>
                    )}
                </section>
            )}

            <button type="button" onClick={parse}>Parse resume</button>

            {!analysis && <p className="muted">No analysis yet. Run parse to extract skills for review.</p>}
            {analysis && (
                <section>
                    <h2>Status: {analysis.status}</h2>
                    <p>Provider: {analysis.provider} ({analysis.providerType})</p>
                    {analysis.fallbackNote && <p className="muted">{analysis.fallbackNote}</p>}
                    {analysis.failureReason && <p className="error">{analysis.failureReason}</p>}
                    <p>{analysis.analysisSummary}</p>
                    <p>Name: {analysis.extractedName || "—"}</p>
                    <p>Email: {analysis.extractedEmail || "—"}</p>
                    <p>Phone: {analysis.extractedPhone || "—"}</p>
                    <h3>Extracted skills</h3>
                    <ul>
                        {(analysis.skills || []).map((s) => (
                            <li key={s.id}>
                                {s.status === "Pending" ? (
                                    <label>
                                        <input
                                            type="checkbox"
                                            checked={selected.includes(s.id)}
                                            onChange={() => toggleSkill(s.id)}
                                        />
                                        {" "}
                                        {s.canonicalName} ({s.confidence})
                                    </label>
                                ) : (
                                    <span>{s.canonicalName} — {s.status}</span>
                                )}
                            </li>
                        ))}
                    </ul>
                    {analysis.status === "ReviewRequired" && (
                        <button type="button" onClick={confirm}>Confirm selected skills</button>
                    )}
                </section>
            )}
            <Link to="/candidate/profile">Back to profile</Link>
        </div>
    );
}
