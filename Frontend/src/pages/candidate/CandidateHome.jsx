import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";
import { useAuth } from "../../auth/useAuth";
import "../CandidateDashboard.css";

export default function CandidateHome() {
    const { user } = useAuth();
    const [summary, setSummary] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/candidate/dashboard");
                if (alive) setSummary(response.data);
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
        return <main className="dash-page"><p>Loading dashboard…</p></main>;
    }

    if (error) {
        return <main className="dash-page"><p className="error">{error}</p></main>;
    }

    const empty = !summary || (
        summary.latestApplicationsCount === 0 &&
        summary.interviewsCount === 0 &&
        summary.assessmentsCount === 0
    );

    return (
        <main className="dash-page">
            <header className="dash-header">
                <h1>Candidate dashboard</h1>
                <p>Welcome{user?.fullName ? `, ${user.fullName}` : ""}.</p>
                <nav className="dash-nav">
                    <Link to="/candidate/profile">Profile &amp; documents</Link>
                    <Link to="/candidate/jobs">Browse jobs</Link>
                    <Link to="/candidate/recommendations">Recommendations</Link>
                    <Link to="/candidate/applications">My applications</Link>
                    <Link to="/candidate/assessments">Assessments</Link>
                    <Link to="/candidate/interviews">Interviews</Link>
                    <Link to="/candidate/notifications">Notifications</Link>
                </nav>
            </header>

            <section className="dash-stats">
                <article>
                    <h2>Profile completion</h2>
                    <p>{summary?.profileCompletionPercent ?? 0}%</p>
                </article>
                <article>
                    <h2>Applications</h2>
                    <p>{summary?.latestApplicationsCount ?? 0}</p>
                </article>
                <article>
                    <h2>Upcoming interviews</h2>
                    <p>{summary?.interviewsCount ?? 0}</p>
                </article>
                <article>
                    <h2>Pending assessments</h2>
                    <p>{summary?.assessmentsCount ?? 0}</p>
                </article>
                <article>
                    <h2>Recommendations</h2>
                    <p>{summary?.recommendationsCount ?? 0}</p>
                </article>
                <article>
                    <h2>Unread notifications</h2>
                    <p>{summary?.unreadNotificationsCount ?? 0}</p>
                </article>
                <article>
                    <h2>Resume analysis</h2>
                    <p>{summary?.resumeAnalysisStatus ?? "NotAvailable"}</p>
                </article>
            </section>

            {empty && (
                <p className="empty-state">
                    No applications, interviews, or assessments yet. Complete your profile to get started.
                </p>
            )}
        </main>
    );
}
