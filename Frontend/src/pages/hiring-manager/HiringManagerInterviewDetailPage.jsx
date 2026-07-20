import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

const empty = {
    technicalCompetency: 3,
    communication: 3,
    problemSolving: 3,
    roleKnowledge: 3,
    teamwork: 3,
    leadership: 3,
    culturalContribution: 3,
    strengths: "",
    concerns: "",
    recommendation: "",
    comments: "",
    privatePanelComments: ""
};

export default function HiringManagerInterviewDetailPage() {
    const { id } = useParams();
    const [detail, setDetail] = useState(null);
    const [form, setForm] = useState(empty);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);
    const [loading, setLoading] = useState(true);
    const [dirty, setDirty] = useState(false);

    useEffect(() => {
        let alive = true;
        (async () => {
            setLoading(true);
            try {
                const response = await api.get(`/hiring-manager/interviews/${id}`);
                if (!alive) return;
                setDetail(response.data);
                if (response.data.myFeedback) {
                    setForm({ ...empty, ...response.data.myFeedback });
                }
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load interview.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    async function reload() {
        setLoading(true);
        try {
            const response = await api.get(`/hiring-manager/interviews/${id}`);
            setDetail(response.data);
            if (response.data.myFeedback) {
                setForm({ ...empty, ...response.data.myFeedback });
            }
        } catch (err) {
            setError(err.response?.data?.message || "Could not load interview.");
        } finally {
            setLoading(false);
        }
    }
    useEffect(() => {
        const onBeforeUnload = (e) => {
            if (dirty) {
                e.preventDefault();
                e.returnValue = "";
            }
        };
        window.addEventListener("beforeunload", onBeforeUnload);
        return () => window.removeEventListener("beforeunload", onBeforeUnload);
    }, [dirty]);

    function update(field, value) {
        setDirty(true);
        setForm((prev) => ({ ...prev, [field]: value }));
    }

    async function submit(e) {
        e.preventDefault();
        if (!window.confirm("Submit interview feedback?")) return;
        setError(null);
        setSuccess(null);
        try {
            const method = detail?.myFeedback ? "put" : "post";
            await api[method](`/hiring-manager/interviews/${id}/feedback`, form);
            setSuccess("Feedback submitted.");
            setDirty(false);
            await reload();
        } catch (err) {
            setError(err.response?.data?.message || "Could not save feedback.");
        }
    }

    if (loading) return <main className="hm-page"><p>Loading interview…</p></main>;
    if (!detail) return <main className="hm-page"><p className="hm-error">{error}</p></main>;

    return (
        <main className="hm-page">
            <h2>Interview detail</h2>
            <p className="hm-muted">
                {detail.candidateName} · {detail.jobTitle} · {detail.interviewDateUtc} ({detail.timeZoneId}) · Response: {detail.candidateResponse}
            </p>
            {error && <p className="hm-error" role="alert">{error}</p>}
            {success && <p className="hm-success" role="status">{success}</p>}
            <form className="hm-form" onSubmit={submit}>
                <h3>Structured feedback</h3>
                {detail.myFeedback && <p className="hm-muted">Existing feedback loaded — submitting updates it.</p>}
                {[
                    ["technicalCompetency", "Technical competency"],
                    ["communication", "Communication"],
                    ["problemSolving", "Problem solving"],
                    ["roleKnowledge", "Role knowledge"],
                    ["teamwork", "Teamwork"],
                    ["leadership", "Leadership"],
                    ["culturalContribution", "Cultural contribution (job-relevant)"]
                ].map(([key, label]) => (
                    <label key={key}>
                        {label} (1–5)
                        <input
                            type="number"
                            min="1"
                            max="5"
                            step="0.5"
                            value={form[key]}
                            onChange={(e) => update(key, Number(e.target.value))}
                        />
                    </label>
                ))}
                <label>
                    Strengths
                    <textarea value={form.strengths} onChange={(e) => update("strengths", e.target.value)} />
                </label>
                <label>
                    Concerns
                    <textarea value={form.concerns} onChange={(e) => update("concerns", e.target.value)} />
                </label>
                <label>
                    Recommendation
                    <input required value={form.recommendation} onChange={(e) => update("recommendation", e.target.value)} />
                </label>
                <label>
                    Private panel comments
                    <textarea value={form.privatePanelComments} onChange={(e) => update("privatePanelComments", e.target.value)} />
                </label>
                <button type="submit" className="hm-btn">Save feedback</button>
            </form>
            <Link className="hm-btn secondary" to={`/hiring-manager/applications/${detail.applicationId}/evaluation`}>
                Open evaluation
            </Link>
            <Link className="hm-btn secondary" to={`/hiring-manager/applications/${detail.applicationId}/recommendation`}>
                Recommendation
            </Link>
        </main>
    );
}
