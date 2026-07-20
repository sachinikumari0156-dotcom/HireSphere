import { useEffect, useState } from "react";
import api from "../../api/axios";

export default function RecruiterReportsPage() {
    const [summary, setSummary] = useState(null);
    const [jobId, setJobId] = useState("");
    const [fromUtc, setFrom] = useState("");
    const [toUtc, setTo] = useState("");
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/recruiter/reports/summary");
                if (alive) setSummary(response.data);
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load reports.");
                    setSummary(null);
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    async function load(params = {}) {
        setLoading(true);
        setError(null);
        try {
            const response = await api.get("/recruiter/reports/summary", { params });
            setSummary(response.data);
        } catch (err) {
            setError(err.response?.data?.message || "Could not load reports.");
            setSummary(null);
        } finally {
            setLoading(false);
        }
    }
    async function exportCsv() {
        setError(null);
        try {
            const params = {};
            if (jobId) params.jobId = jobId;
            if (fromUtc) params.fromUtc = new Date(fromUtc).toISOString();
            if (toUtc) params.toUtc = new Date(toUtc).toISOString();
            const response = await api.get("/recruiter/reports/export", {
                params,
                responseType: "blob"
            });
            const url = URL.createObjectURL(response.data);
            const a = document.createElement("a");
            a.href = url;
            a.download = "recruiter-report.csv";
            a.click();
            URL.revokeObjectURL(url);
        } catch (err) {
            setError(err.response?.data?.message || "CSV export failed.");
        }
    }

    const empty = summary && summary.applicationsTotal === 0;

    return (
        <main className="rec-page">
            <h2>Recruitment reports</h2>
            <p className="rec-muted">Organization-scoped metrics from SQL Server. No fake chart data.</p>

            <form
                className="rec-filters"
                onSubmit={(e) => {
                    e.preventDefault();
                    const params = {};
                    if (jobId) params.jobId = jobId;
                    if (fromUtc) params.fromUtc = new Date(fromUtc).toISOString();
                    if (toUtc) params.toUtc = new Date(toUtc).toISOString();
                    load(params);
                }}
            >
                <label>
                    Job ID
                    <input value={jobId} onChange={(e) => setJobId(e.target.value)} />
                </label>
                <label>
                    From (UTC)
                    <input type="datetime-local" value={fromUtc} onChange={(e) => setFrom(e.target.value)} />
                </label>
                <label>
                    To (UTC)
                    <input type="datetime-local" value={toUtc} onChange={(e) => setTo(e.target.value)} />
                </label>
                <div className="rec-actions">
                    <button type="submit" className="rec-btn">Apply filters</button>
                    <button type="button" className="rec-btn secondary" onClick={exportCsv}>Export CSV</button>
                </div>
            </form>

            {loading && <p>Loading reports…</p>}
            {error && <p className="rec-error" role="alert">{error}</p>}
            {empty && <p className="rec-muted">No applications in the selected range.</p>}

            {summary && !loading && (
                <>
                    <section className="rec-stats" aria-label="Report summary">
                        <article><h3>Applications</h3><p>{summary.applicationsTotal}</p></article>
                        <article><h3>Shortlist rate</h3><p>{summary.shortlistRate}%</p></article>
                        <article><h3>Rejection rate</h3><p>{summary.rejectionRate}%</p></article>
                        <article><h3>Assessments</h3><p>{summary.assessmentAssignments}</p></article>
                        <article><h3>Interviews</h3><p>{summary.interviewsScheduled}</p></article>
                    </section>
                    <section>
                        <h3>Applications by status</h3>
                        <ul className="rec-activity">
                            {(summary.applicationsByStatus || []).map((row) => (
                                <li key={row.name}>{row.name}: {row.count}</li>
                            ))}
                        </ul>
                    </section>
                    <section>
                        <h3>Applications over time</h3>
                        <ul className="rec-activity">
                            {(summary.applicationsOverTime || []).map((row) => (
                                <li key={row.name}>{row.name}: {row.count}</li>
                            ))}
                        </ul>
                    </section>
                </>
            )}
        </main>
    );
}
