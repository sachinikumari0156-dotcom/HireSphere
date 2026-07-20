import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";

const STATUSES = ["", "Draft", "PendingApproval", "Published", "Paused", "Closed", "Archived", "Open"];

export default function RecruiterJobsPage() {
    const [items, setItems] = useState([]);
    const [totalCount, setTotalCount] = useState(0);
    const [keyword, setKeyword] = useState("");
    const [status, setStatus] = useState("");
    const [location, setLocation] = useState("");
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const params = { page: 1, pageSize: 25 };
                const response = await api.get("/recruiter/jobs", { params });
                if (!alive) return;
                setItems(response.data.items || []);
                setTotalCount(response.data.totalCount || 0);
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load jobs.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    async function load() {
        setLoading(true);
        setError(null);
        try {
            const params = { page: 1, pageSize: 25 };
            if (keyword.trim()) params.keyword = keyword.trim();
            if (status) params.status = status;
            if (location.trim()) params.location = location.trim();
            const response = await api.get("/recruiter/jobs", { params });
            setItems(response.data.items || []);
            setTotalCount(response.data.totalCount || 0);
        } catch (err) {
            setError(err.response?.data?.message || "Could not load jobs.");
        } finally {
            setLoading(false);
        }
    }

    return (
        <div className="rec-page">
            <h2>Jobs</h2>
            <p className="rec-muted">{totalCount} job(s) in your organization.</p>

            <form
                className="rec-filters"
                onSubmit={(e) => {
                    e.preventDefault();
                    load();
                }}
            >
                <label>
                    Keyword
                    <input value={keyword} onChange={(e) => setKeyword(e.target.value)} />
                </label>
                <label>
                    Status
                    <select value={status} onChange={(e) => setStatus(e.target.value)}>
                        {STATUSES.map((s) => (
                            <option key={s || "all"} value={s}>{s || "All statuses"}</option>
                        ))}
                    </select>
                </label>
                <label>
                    Location
                    <input value={location} onChange={(e) => setLocation(e.target.value)} />
                </label>
                <div className="rec-actions">
                    <button type="submit" className="rec-btn">Filter</button>
                    <Link className="rec-btn secondary" to="/recruiter/jobs/new">Create job</Link>
                </div>
            </form>

            {loading && <p>Loading jobs…</p>}
            {error && <p className="rec-error" role="alert">{error}</p>}
            {!loading && !error && items.length === 0 && (
                <p className="rec-muted">No jobs match your filters.</p>
            )}

            {!loading && items.length > 0 && (
                <div className="rec-table-wrap">
                    <table className="rec-table">
                        <thead>
                            <tr>
                                <th>Title</th>
                                <th>Status</th>
                                <th>Location</th>
                                <th>Applicants</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {items.map((job) => (
                                <tr key={job.id}>
                                    <td data-label="Title">{job.title}</td>
                                    <td data-label="Status">{job.status}</td>
                                    <td data-label="Location">{job.location}</td>
                                    <td data-label="Applicants">{job.applicantCount}</td>
                                    <td data-label="Actions">
                                        <Link to={`/recruiter/jobs/${job.id}`}>Open</Link>
                                        {" · "}
                                        <Link to={`/recruiter/jobs/${job.id}/edit`}>Edit</Link>
                                        {" · "}
                                        <Link to={`/recruiter/jobs/${job.id}/applicants`}>Pipeline</Link>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}
