import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";

export default function HiringManagerJobsPage() {
    const [items, setItems] = useState([]);
    const [keyword, setKeyword] = useState("");
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    async function load(params = {}) {
        setLoading(true);
        setError(null);
        try {
            const response = await api.get("/hiring-manager/jobs", { params });
            setItems(response.data.items || []);
        } catch (err) {
            setError(err.response?.data?.message || "Could not load vacancies.");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        load();
    }, []);

    return (
        <main className="hm-page">
            <h2>Assigned vacancies</h2>
            <p className="hm-muted">Only jobs assigned to you as Hiring Manager.</p>
            <form
                className="hm-filters"
                onSubmit={(e) => {
                    e.preventDefault();
                    load({ keyword });
                }}
            >
                <label>
                    Keyword
                    <input value={keyword} onChange={(e) => setKeyword(e.target.value)} />
                </label>
                <button type="submit" className="hm-btn">Filter</button>
            </form>
            {error && <p className="hm-error" role="alert">{error}</p>}
            {loading && <p>Loading vacancies…</p>}
            {!loading && items.length === 0 && <p className="hm-muted">No assigned vacancies.</p>}
            {!loading && items.length > 0 && (
                <table className="hm-table">
                    <thead>
                        <tr>
                            <th>Title</th>
                            <th>Status</th>
                            <th>Applicants</th>
                            <th>Shortlist</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {items.map((job) => (
                            <tr key={job.id}>
                                <td data-label="Title">{job.title}</td>
                                <td data-label="Status">{job.status}</td>
                                <td data-label="Applicants">{job.applicantCount}</td>
                                <td data-label="Shortlist">{job.shortlistCount}</td>
                                <td data-label="Actions">
                                    <Link to={`/hiring-manager/jobs/${job.id}`}>Open</Link>
                                    {" · "}
                                    <Link to={`/hiring-manager/jobs/${job.id}/candidates`}>Candidates</Link>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </main>
    );
}
