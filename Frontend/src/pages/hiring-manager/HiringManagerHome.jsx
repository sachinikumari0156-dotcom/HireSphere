import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";

export default function HiringManagerHome() {
    const [data, setData] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/hiring-manager/dashboard");
                if (alive) setData(response.data);
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load dashboard.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    if (loading) {
        return <main className="hm-page"><p>Loading hiring manager dashboard…</p></main>;
    }

    if (error) {
        return <main className="hm-page"><p className="hm-error" role="alert">{error}</p></main>;
    }

    return (
        <main className="hm-page">
            <h2>Dashboard</h2>
            <p className="hm-muted">Live metrics for vacancies assigned to you only.</p>
            <section className="hm-stats" aria-label="Hiring manager metrics">
                <article><h3>Active vacancies</h3><p>{data.assignedActiveVacancies}</p></article>
                <article><h3>Paused vacancies</h3><p>{data.assignedPausedVacancies}</p></article>
                <article><h3>Awaiting review</h3><p>{data.candidatesAwaitingReview}</p></article>
                <article><h3>Shortlisted</h3><p>{data.candidatesShortlisted}</p></article>
                <article><h3>Upcoming interviews</h3><p>{data.upcomingInterviews}</p></article>
                <article><h3>Pending feedback</h3><p>{data.pendingInterviewFeedback}</p></article>
                <article><h3>Pending evaluations</h3><p>{data.pendingEvaluations}</p></article>
                <article><h3>Pending decisions</h3><p>{data.pendingHiringDecisions}</p></article>
            </section>
            {(!data.recentActivity || data.recentActivity.length === 0) && (
                <p className="hm-muted">No manager activity yet.</p>
            )}
            <Link className="hm-btn" to="/hiring-manager/jobs">View assigned vacancies</Link>
        </main>
    );
}
