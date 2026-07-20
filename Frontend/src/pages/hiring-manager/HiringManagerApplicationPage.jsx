import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

export default function HiringManagerApplicationPage() {
    const { id } = useParams();
    const [detail, setDetail] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/hiring-manager/applications/${id}`);
                if (alive) setDetail(response.data);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load application.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    if (loading) return <div className="hm-page"><p>Loading candidate review…</p></div>;
    if (error) {
        return <div className="hm-page"><p className="hm-error" role="alert">{error}</p></div>;
    }

    const text = JSON.stringify(detail).toLowerCase();
    const hasPathLeak = text.includes("c:\\") || text.includes("passwordhash");

    return (
        <div className="hm-page">
            <h2>Candidate review</h2>
            <p className="hm-muted">{detail.candidateName} · {detail.jobTitle} · {detail.status}</p>

            <section aria-labelledby="hm-ranking-heading">
                <h3 id="hm-ranking-heading">Ranking explanation</h3>
                <p>Match score: {detail.matchScore ?? "—"}</p>
                <p>{detail.matchExplanation}</p>
                <p className="hm-notice" role="note">{detail.humanReviewNotice}</p>
            </section>

            <h3>Summary</h3>
            <p>{detail.professionalSummary || "—"}</p>
            <h3>Skills</h3>
            <p>{(detail.skills || []).join(", ") || "—"}</p>
            <p><strong>Missing required:</strong> {(detail.missingRequiredSkills || []).join(", ") || "None"}</p>

            <section aria-labelledby="hm-resume-heading">
                <h3 id="hm-resume-heading">Resume review</h3>
                <ul>
                    {(detail.resumes || []).map((r) => (
                        <li key={r.documentId}>{r.fileName}{r.isPrimary ? " (primary)" : ""}</li>
                    ))}
                </ul>
                {(!detail.resumes || detail.resumes.length === 0) && <p className="hm-muted">No resume metadata.</p>}
            </section>

            {hasPathLeak && <p className="hm-error">Unexpected sensitive field exposure.</p>}

            <div className="hm-actions">
                <Link className="hm-btn" to={`/hiring-manager/applications/${id}/evaluation`}>Evaluation</Link>
                <Link className="hm-btn" to={`/hiring-manager/applications/${id}/recommendation`}>Recommendation</Link>
                <Link className="hm-btn secondary" to={`/hiring-manager/jobs/${detail.jobId}/candidates`}>
                    Back to candidates
                </Link>
            </div>
        </div>
    );
}
