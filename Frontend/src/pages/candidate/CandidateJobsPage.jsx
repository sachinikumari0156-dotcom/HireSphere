import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

export default function CandidateJobsPage() {
    const [searchParams, setSearchParams] = useSearchParams();
    const [result, setResult] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    const [keyword, setKeyword] = useState(searchParams.get("keyword") || "");
    const [location, setLocation] = useState(searchParams.get("location") || "");
    const [employmentType, setEmploymentType] = useState(searchParams.get("employmentType") || "");
    const [workArrangement, setWorkArrangement] = useState(searchParams.get("workArrangement") || "");
    const [page, setPage] = useState(Number(searchParams.get("page") || "1"));

    useEffect(() => {
        let alive = true;
        (async () => {
            setLoading(true);
            setError(null);
            try {
                const params = {
                    page,
                    pageSize: 10,
                    sortBy: "postedDate",
                    sortDir: "desc"
                };
                if (keyword.trim()) params.keyword = keyword.trim();
                if (location.trim()) params.location = location.trim();
                if (employmentType) params.employmentType = employmentType;
                if (workArrangement) params.workArrangement = workArrangement;

                const response = await api.get("/candidate/jobs", { params });
                if (alive) setResult(response.data);
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load jobs.");
                    setResult(null);
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [keyword, location, employmentType, workArrangement, page]);

    function applyFilters(event) {
        event.preventDefault();
        setPage(1);
        const next = {};
        if (keyword.trim()) next.keyword = keyword.trim();
        if (location.trim()) next.location = location.trim();
        if (employmentType) next.employmentType = employmentType;
        if (workArrangement) next.workArrangement = workArrangement;
        next.page = "1";
        setSearchParams(next);
    }

    return (
        <main className="dash-page">
            <header className="dash-header">
                <h1>Browse jobs</h1>
                <p>Search open roles. Only active jobs are shown.</p>
                <nav className="dash-nav">
                    <Link to="/candidate">Dashboard</Link>
                    <Link to="/candidate/recommendations">Recommendations</Link>
                    <Link to="/candidate/applications">My applications</Link>
                </nav>
            </header>

            <form className="filter-form" onSubmit={applyFilters}>
                <label>
                    Keyword
                    <input value={keyword} onChange={(e) => setKeyword(e.target.value)} placeholder="Title, skills…" />
                </label>
                <label>
                    Location
                    <input value={location} onChange={(e) => setLocation(e.target.value)} placeholder="City" />
                </label>
                <label>
                    Employment type
                    <select value={employmentType} onChange={(e) => setEmploymentType(e.target.value)}>
                        <option value="">Any</option>
                        <option value="FullTime">Full time</option>
                        <option value="PartTime">Part time</option>
                        <option value="Contract">Contract</option>
                        <option value="Internship">Internship</option>
                        <option value="Temporary">Temporary</option>
                    </select>
                </label>
                <label>
                    Work arrangement
                    <select value={workArrangement} onChange={(e) => setWorkArrangement(e.target.value)}>
                        <option value="">Any</option>
                        <option value="OnSite">On site</option>
                        <option value="Remote">Remote</option>
                        <option value="Hybrid">Hybrid</option>
                    </select>
                </label>
                <button type="submit">Search</button>
            </form>

            {loading && <p>Loading jobs…</p>}
            {error && <p className="error">{error}</p>}

            {!loading && !error && result && result.items?.length === 0 && (
                <p className="empty-state">No open jobs match your filters.</p>
            )}

            {!loading && !error && result?.items?.length > 0 && (
                <section className="job-list">
                    {result.items.map((job) => (
                        <article key={job.id} className="job-box">
                            <h2>
                                <Link to={`/candidate/jobs/${job.id}`}>{job.title}</Link>
                            </h2>
                            <p>{job.location} · {job.employmentType} · {job.workArrangement}</p>
                            <p>{job.description}</p>
                            {job.matchScore != null && (
                                <p>Match score: {Number(job.matchScore).toFixed(0)}%</p>
                            )}
                        </article>
                    ))}
                    <div className="pager">
                        <button
                            type="button"
                            disabled={page <= 1}
                            onClick={() => setPage((p) => Math.max(1, p - 1))}
                        >
                            Previous
                        </button>
                        <span>
                            Page {result.page} of {Math.max(result.totalPages, 1)} ({result.totalCount} jobs)
                        </span>
                        <button
                            type="button"
                            disabled={page >= result.totalPages}
                            onClick={() => setPage((p) => p + 1)}
                        >
                            Next
                        </button>
                    </div>
                </section>
            )}
        </main>
    );
}
