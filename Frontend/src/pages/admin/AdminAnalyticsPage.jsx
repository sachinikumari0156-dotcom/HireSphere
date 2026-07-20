import { useEffect, useState } from "react";
import api from "../../api/axios";

export default function AdminAnalyticsPage() {
    const [recruitment, setRecruitment] = useState(null);
    const [skills, setSkills] = useState(null);
    const [departments, setDepartments] = useState(null);
    const [error, setError] = useState(null);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const [r, s, d] = await Promise.all([
                    api.get("/admin/analytics/recruitment"),
                    api.get("/admin/analytics/skills"),
                    api.get("/admin/analytics/departments")
                ]);
                if (!alive) return;
                setRecruitment(r.data);
                setSkills(s.data);
                setDepartments(d.data);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load analytics.");
            }
        })();
        return () => { alive = false; };
    }, []);

    if (error) return <main className="admin-page"><p className="admin-error">{error}</p></main>;
    if (!recruitment) return <main className="admin-page"><p>Loading analytics…</p></main>;

    return (
        <main className="admin-page">
            <h2>Recruitment analytics</h2>
            <p className="admin-muted">{recruitment.unavailableMetricsNote}</p>
            <p>Shortlisted: {recruitment.shortlisted} · Rejected: {recruitment.rejected} · Hired: {recruitment.hired}</p>
            <h3>Applications by status</h3>
            <ul>
                {(recruitment.applicationsByStatus || []).map((x) => (
                    <li key={x.name}>{x.name}: {x.count}</li>
                ))}
            </ul>
            {(recruitment.applicationsByStatus || []).length === 0 && <p className="admin-muted">No application data.</p>}
            <h3>Department activity</h3>
            <ul>
                {(departments?.jobsByDepartment || []).map((x) => (
                    <li key={x.name}>{x.name}: {x.count} jobs</li>
                ))}
            </ul>
            <h3>Skill demand</h3>
            <ul>
                {(skills?.skillDemandFromJobs || []).map((x) => (
                    <li key={x.name}>{x.name}: {x.count}</li>
                ))}
            </ul>
            {(skills?.skillDemandFromJobs || []).length === 0 && <p className="admin-muted">No skill demand data.</p>}
        </main>
    );
}
