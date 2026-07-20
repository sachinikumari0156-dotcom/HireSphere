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

    if (loading) return <main className="hm-page"><p>Loading candidate review…</p></main>;
    if (error) {
        return <main className="hm-page"><p className="hm-error" role="alert">{error}</p></main>;
    }

    const text = JSON.stringify(detail).toLowerCase();
    const hasPathLeak = text.includes("c:\\") || text.includes("passwordhash");

    return (
        <main className="hm-page">
            <h2>Candidate review</h2>
            <p className="hm-muted">{detail.candidateName} · {detail.jobTitle} · {detail.status}</p>
            <p>Match score: {detail.matchScore ?? "—"}</p>
            <p>{detail.matchExplanation}</p>
            <p className="hm-notice" role="note">{detail.humanReviewNotice}</p>
            <h3>Summary</h3>
            <p>{detail.professionalSummary || "—"}</p>
            <h3>Skills</h3>
            <p>{(detail.skills || []).join(", ") || "—"}</p>
            <p><strong>Missing required:</strong> {(detail.missingRequiredSkills || []).join(", ") || "None"}</p>
            <h3>Resumes</h3>
            <ul>
                {(detail.resumes || []).map((r) => (
                    <li key={r.documentId}>{r.fileName}{r.isPrimary ? " (primary)" : ""}</li>
                ))}
            </ul>
            {(!detail.resumes || detail.resumes.length === 0) && <p className="hm-muted">No resume metadata.</p>}
            {hasPathLeak && <p className="hm-error">Unexpected sensitive field exposure.</p>}
            <Link className="hm-btn secondary" to={`/hiring-manager/jobs/${detail.jobId}/candidates`}>
                Back to candidates
            </Link>
        </main>
    );
}
