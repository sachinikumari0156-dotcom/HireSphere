import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

export default function CandidateJobDetailPage() {
    const { id } = useParams();
    const [job, setJob] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            setLoading(true);
            setError(null);
            try {
                const response = await api.get(`/candidate/jobs/${id}`);
                if (alive) setJob(response.data);
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load job.");
                    setJob(null);
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    if (loading) {
        return <div className="dash-page"><p>Loading job…</p></div>;
    }

    if (error) {
        return (
            <div className="dash-page">
                <p className="error">{error}</p>
                <Link to="/candidate/jobs">Back to jobs</Link>
            </div>
        );
    }

    const match = job?.match;

    return (
        <div className="dash-page">
            <header className="dash-header">
                <h1>{job.title}</h1>
                <p>
                    {job.organizationName ? `${job.organizationName} · ` : ""}
                    {job.location} · {job.employmentType} · {job.workArrangement}
                </p>
                <nav className="dash-nav">
                    <Link to="/candidate/jobs">All jobs</Link>
                    {!job.alreadyApplied && (
                        <Link to={`/candidate/jobs/${job.id}/apply`}>Apply</Link>
                    )}
                    {job.alreadyApplied && <span>Already applied</span>}
                </nav>
            </header>

            <section className="job-box">
                <h2>Description</h2>
                <p style={{ whiteSpace: "pre-wrap" }}>{job.description}</p>
            </section>

            {job.skills?.length > 0 && (
                <section className="job-box">
                    <h2>Skills</h2>
                    <ul>
                        {job.skills.map((s) => (
                            <li key={s.skillId}>
                                {s.skillName}{s.isRequired ? " (required)" : ""}
                            </li>
                        ))}
                    </ul>
                </section>
            )}

            {match && (
                <section className="job-box match-panel">
                    <h2>Match explanation</h2>
                    <p>
                        Score: <strong>{Number(match.matchScore).toFixed(0)}%</strong>
                        {" · "}Provider: {match.provider}
                    </p>
                    <p>{match.explanation}</p>
                    {match.matchedSkills?.length > 0 && (
                        <p>Matched skills: {match.matchedSkills.join(", ")}</p>
                    )}
                    {match.missingSkills?.length > 0 && (
                        <p>Missing skills: {match.missingSkills.join(", ")}</p>
                    )}
                    <p className="empty-state">{match.humanReviewNotice}</p>
                </section>
            )}
        </div>
    );
}
