import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";

export default function RecruiterHome() {
    const [data, setData] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/recruiter/dashboard");
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
        return <main className="rec-page"><p>Loading recruiter dashboard…</p></main>;
    }

    if (error) {
        return <main className="rec-page"><p className="rec-error" role="alert">{error}</p></main>;
    }

    const empty = data
        && data.activeJobs === 0
        && data.draftJobs === 0
        && data.totalApplicants === 0;

    return (
        <main className="rec-page">
            <h2>Dashboard</h2>
            <p className="rec-muted">Live metrics for your organization only.</p>

            {empty && (
                <p className="rec-muted">No recruitment activity yet. Create a job to get started.</p>
            )}

            <section className="rec-stats" aria-label="Recruitment metrics">
                <article><h3>Active jobs</h3><p>{data?.activeJobs ?? 0}</p></article>
                <article><h3>Draft</h3><p>{data?.draftJobs ?? 0}</p></article>
                <article><h3>Paused</h3><p>{data?.pausedJobs ?? 0}</p></article>
                <article><h3>Closed</h3><p>{data?.closedJobs ?? 0}</p></article>
                <article><h3>Applicants</h3><p>{data?.totalApplicants ?? 0}</p></article>
                <article><h3>New (7d)</h3><p>{data?.newApplicants ?? 0}</p></article>
                <article><h3>Screening</h3><p>{data?.candidatesInScreening ?? 0}</p></article>
                <article><h3>Shortlisted</h3><p>{data?.shortlistedCandidates ?? 0}</p></article>
                <article><h3>Assessments</h3><p>{data?.pendingAssessments ?? 0}</p></article>
                <article><h3>Interviews</h3><p>{data?.upcomingInterviews ?? 0}</p></article>
            </section>

            <div className="rec-actions">
                <Link className="rec-btn" to="/recruiter/jobs">View jobs</Link>
                <Link className="rec-btn secondary" to="/recruiter/jobs/new">Create job</Link>
            </div>

            <section style={{ marginTop: "2rem" }}>
                <h2>Recent activity</h2>
                {(data?.recentActivity?.length ?? 0) === 0 ? (
                    <p className="rec-muted">No recent audit activity.</p>
                ) : (
                    <ul className="rec-activity">
                        {data.recentActivity.map((item, index) => (
                            <li key={`${item.action}-${item.createdAtUtc}-${index}`}>
                                <strong>{item.action}</strong>
                                {" · "}
                                {item.details || item.entityType}
                                <div className="rec-muted">{new Date(item.createdAtUtc).toLocaleString()}</div>
                            </li>
                        ))}
                    </ul>
                )}
            </section>
        </main>
    );
}
