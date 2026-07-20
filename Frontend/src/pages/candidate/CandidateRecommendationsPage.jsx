import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

export default function CandidateRecommendationsPage() {
    const [data, setData] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/candidate/recommendations");
                if (alive) setData(response.data);
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load recommendations.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    if (loading) {
        return <main className="dash-page"><p>Loading recommendations…</p></main>;
    }

    if (error) {
        return <main className="dash-page"><p className="error">{error}</p></main>;
    }

    return (
        <main className="dash-page">
            <header className="dash-header">
                <h1>Recommended jobs</h1>
                <p>
                    Deterministic match scores based on your profile.
                    Profile completion: {data?.profileCompletionPercent ?? 0}%.
                </p>
                <nav className="dash-nav">
                    <Link to="/candidate">Dashboard</Link>
                    <Link to="/candidate/jobs">Browse jobs</Link>
                    <Link to="/candidate/profile">Profile</Link>
                </nav>
            </header>

            {!data?.profileCompleteEnough && (
                <p className="empty-state">{data?.message}</p>
            )}

            {data?.profileCompleteEnough && data.jobs?.length === 0 && (
                <p className="empty-state">{data.message || "No recommendations available."}</p>
            )}

            {data?.jobs?.length > 0 && (
                <section className="job-list">
                    {data.jobs.map((job) => (
                        <article key={job.id} className="job-box">
                            <h2>
                                <Link to={`/candidate/jobs/${job.id}`}>{job.title}</Link>
                            </h2>
                            <p>{job.location} · Match {Number(job.matchScore ?? 0).toFixed(0)}%</p>
                            <p>{job.description}</p>
                        </article>
                    ))}
                </section>
            )}
        </main>
    );
}
