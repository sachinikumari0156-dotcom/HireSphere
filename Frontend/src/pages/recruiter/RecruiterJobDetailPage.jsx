import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

export default function RecruiterJobDetailPage() {
    const { id } = useParams();
    const [job, setJob] = useState(null);
    const [error, setError] = useState(null);
    const [message, setMessage] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/recruiter/jobs/${id}`);
                if (alive) setJob(response.data);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load job.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    async function changeStatus(status) {
        setMessage(null);
        setError(null);
        try {
            const response = await api.patch(`/recruiter/jobs/${id}/status`, { status });
            setJob(response.data);
            setMessage(`Status updated to ${status}.`);
        } catch (err) {
            setError(err.response?.data?.message || "Status change failed.");
        }
    }

    if (loading) return <div className="rec-page"><p>Loading job…</p></div>;
    if (error && !job) return <div className="rec-page"><p className="rec-error">{error}</p></div>;
    if (!job) return <div className="rec-page"><p className="rec-muted">Job not found.</p></div>;

    return (
        <div className="rec-page">
            <h2>{job.title}</h2>
            <p className="rec-muted">Status: {job.status} · {job.location}</p>
            {message && <p className="rec-success" role="status">{message}</p>}
            {error && <p className="rec-error" role="alert">{error}</p>}

            <section>
                <h3>Description</h3>
                <p>{job.description}</p>
                {job.responsibilities && (
                    <>
                        <h3>Responsibilities</h3>
                        <p>{job.responsibilities}</p>
                    </>
                )}
            </section>

            <section>
                <h3>Skills</h3>
                {(job.skills?.length ?? 0) === 0 ? (
                    <p className="rec-muted">No structured skills.</p>
                ) : (
                    <ul>
                        {job.skills.map((s) => (
                            <li key={s.id || s.skillId}>
                                {s.skillName} {s.isRequired ? "(required)" : "(preferred)"}
                            </li>
                        ))}
                    </ul>
                )}
            </section>

            <section>
                <h3>Screening questions</h3>
                {(job.screeningQuestions?.length ?? 0) === 0 ? (
                    <p className="rec-muted">No screening questions.</p>
                ) : (
                    <ol>
                        {job.screeningQuestions.map((q) => (
                            <li key={q.id}>{q.questionText}{q.isRequired ? " *" : ""}</li>
                        ))}
                    </ol>
                )}
            </section>

            <div className="rec-actions">
                <Link className="rec-btn secondary" to={`/recruiter/jobs/${id}/edit`}>Edit</Link>
                <Link className="rec-btn" to={`/recruiter/jobs/${id}/applicants`}>Applicant pipeline</Link>
                {job.status === "Draft" && (
                    <button type="button" className="rec-btn" onClick={() => changeStatus("Published")}>
                        Publish
                    </button>
                )}
                {(job.status === "Published" || job.status === "Open") && (
                    <>
                        <button type="button" className="rec-btn secondary" onClick={() => changeStatus("Paused")}>
                            Pause
                        </button>
                        <button type="button" className="rec-btn danger" onClick={() => changeStatus("Closed")}>
                            Close
                        </button>
                    </>
                )}
                {job.status === "Paused" && (
                    <button type="button" className="rec-btn" onClick={() => changeStatus("Published")}>
                        Resume
                    </button>
                )}
            </div>
        </div>
    );
}
