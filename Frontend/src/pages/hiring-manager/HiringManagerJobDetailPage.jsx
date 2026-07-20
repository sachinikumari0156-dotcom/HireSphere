import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

export default function HiringManagerJobDetailPage() {
    const { id } = useParams();
    const [job, setJob] = useState(null);
    const [comment, setComment] = useState("");
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            setLoading(true);
            setError(null);
            try {
                const response = await api.get(`/hiring-manager/jobs/${id}`);
                if (alive) setJob(response.data);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load vacancy.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    async function reload() {
        setLoading(true);
        setError(null);
        try {
            const response = await api.get(`/hiring-manager/jobs/${id}`);
            setJob(response.data);
        } catch (err) {
            setError(err.response?.data?.message || "Could not load vacancy.");
        } finally {
            setLoading(false);
        }
    }
    async function submitComment(e) {
        e.preventDefault();
        setSuccess(null);
        setError(null);
        try {
            await api.post(`/hiring-manager/jobs/${id}/review-comments`, { content: comment });
            setComment("");
            setSuccess("Review comment saved.");
            await reload();
        } catch (err) {
            setError(err.response?.data?.message || "Could not save comment.");
        }
    }

    if (loading) return <div className="hm-page"><p>Loading vacancy…</p></div>;
    if (error && !job) {
        return <div className="hm-page"><p className="hm-error" role="alert">{error}</p></div>;
    }

    return (
        <div className="hm-page">
            <h2>{job.title}</h2>
            <p className="hm-muted">{job.status} · {job.location} · Recruiter: {job.recruiterName || "—"}</p>
            {error && <p className="hm-error" role="alert">{error}</p>}
            {success && <p className="hm-success" role="status">{success}</p>}
            <p>{job.description}</p>
            <p><strong>Min experience:</strong> {job.minimumExperienceYears ?? "—"} years</p>
            <p><strong>Education:</strong> {job.educationRequirement || "—"}</p>
            <p><strong>Deadline:</strong> {job.applicationDeadlineUtc || "—"}</p>
            <h3>Skills</h3>
            <ul>
                {(job.skills || []).map((s) => (
                    <li key={`${s.skillName}-${s.isRequired}`}>
                        {s.skillName}{s.isRequired ? " (required)" : " (preferred)"}
                    </li>
                ))}
            </ul>
            <p>Applicants: {job.applicantCount} · Shortlisted: {job.shortlistCount} · Interviews: {job.interviewCount}</p>
            <Link className="hm-btn" to={`/hiring-manager/jobs/${id}/candidates`}>Review candidates</Link>
            <form className="hm-form" onSubmit={submitComment}>
                <label>
                    Vacancy review comment
                    <textarea value={comment} onChange={(e) => setComment(e.target.value)} rows={3} />
                </label>
                <button type="submit" className="hm-btn secondary">Add comment</button>
            </form>
            <h3>Review comments</h3>
            {(job.reviewComments || []).length === 0 && <p className="hm-muted">No comments yet.</p>}
            <ul>
                {(job.reviewComments || []).map((c) => (
                    <li key={c.id}>{c.authorName}: {c.content}</li>
                ))}
            </ul>
        </div>
    );
}
