import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";
import FilterDrawer from "../../components/ui/FilterDrawer";
import { StatusBadge } from "../../components/ui/primitives";
import { friendlyStatus } from "../../utils/statusLabels";

const STATUSES = [
    "", "Pending", "UnderReview", "ManualReview", "Assessment",
    "Shortlisted", "InterviewScheduled", "Rejected", "Withdrawn"
];

export default function RecruiterPipelinePage() {
    const { id } = useParams();
    const [items, setItems] = useState([]);
    const [selected, setSelected] = useState([]);
    const [keyword, setKeyword] = useState("");
    const [status, setStatus] = useState("");
    const [error, setError] = useState(null);
    const [message, setMessage] = useState(null);
    const [loading, setLoading] = useState(true);
    const [confirmAction, setConfirmAction] = useState(null);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const params = { page: 1, pageSize: 50 };
                const response = await api.get(`/recruiter/jobs/${id}/applications`, { params });
                if (!alive) return;
                setItems(response.data.items || []);
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load applicants.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    async function load() {
        setLoading(true);
        setError(null);
        try {
            const params = { page: 1, pageSize: 50 };
            if (keyword.trim()) params.keyword = keyword.trim();
            if (status) params.status = status;
            const response = await api.get(`/recruiter/jobs/${id}/applications`, { params });
            setItems(response.data.items || []);
        } catch (err) {
            setError(err.response?.data?.message || "Could not load applicants.");
        } finally {
            setLoading(false);
        }
    }

    function toggleSelect(applicationId) {
        setSelected((prev) => {
            if (prev.includes(applicationId)) {
                return prev.filter((x) => x !== applicationId);
            }
            if (prev.length >= 5) {
                setError("You can compare at most 5 applicants.");
                return prev;
            }
            setError(null);
            return [...prev, applicationId];
        });
    }

    async function applyStatus(applicationId, nextStatus) {
        setConfirmAction(null);
        setMessage(null);
        setError(null);
        try {
            await api.patch(`/recruiter/applications/${applicationId}/status`, {
                status: nextStatus,
                notes: `Moved to ${nextStatus}`
            });
            setMessage(`Application updated to ${friendlyStatus(nextStatus)}.`);
            await load();
        } catch (err) {
            setError(err.response?.data?.message || "Status update failed.");
        }
    }

    const filterForm = (
        <form
            className="rec-filters"
            onSubmit={(e) => {
                e.preventDefault();
                load();
            }}
        >
            <label>
                Search
                <input value={keyword} onChange={(e) => setKeyword(e.target.value)} />
            </label>
            <label>
                Status
                <select value={status} onChange={(e) => setStatus(e.target.value)}>
                    {STATUSES.map((s) => (
                        <option key={s || "all"} value={s}>{s ? friendlyStatus(s) : "All statuses"}</option>
                    ))}
                </select>
            </label>
            <div className="rec-actions">
                <button type="submit" className="rec-btn">Filter</button>
                <Link
                    className="rec-btn secondary"
                    to={`/recruiter/compare?ids=${selected.join(",")}`}
                    aria-disabled={selected.length < 2}
                    onClick={(e) => {
                        if (selected.length < 2) {
                            e.preventDefault();
                            setError("Select at least 2 applicants to compare.");
                        }
                    }}
                >
                    Compare selected ({selected.length}/5)
                </Link>
            </div>
        </form>
    );

    return (
        <main className="rec-page portal-page">
            <h2>Applicant pipeline</h2>
            <p className="rec-muted">
                Job #{id}. Select up to 5 applicants to compare.
                {" "}
                <Link to={`/recruiter/jobs/${id}`}>Back to job</Link>
            </p>

            <FilterDrawer title="pipeline filters">{filterForm}</FilterDrawer>

            {loading && <p>Loading applicants…</p>}
            {error && <p className="rec-error" role="alert">{error}</p>}
            {message && <p className="rec-success" role="status">{message}</p>}
            {!loading && items.length === 0 && <p className="rec-muted">No applicants yet.</p>}

            {confirmAction && (
                <div className="rec-notice" role="dialog" aria-label="Confirm status change">
                    <p>
                        Confirm moving application #{confirmAction.id} to {friendlyStatus(confirmAction.status)}?
                    </p>
                    <div className="rec-actions">
                        <button
                            type="button"
                            className="rec-btn"
                            onClick={() => applyStatus(confirmAction.id, confirmAction.status)}
                        >
                            Confirm
                        </button>
                        <button
                            type="button"
                            className="rec-btn secondary"
                            onClick={() => setConfirmAction(null)}
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            )}

            {items.length > 0 && (
                <div className="rec-table-wrap portal-table-wrap" tabIndex={0} aria-label="Applicant pipeline">
                    <table className="rec-table">
                        <caption className="hs-sr-only">Applicants for job {id}</caption>
                        <thead>
                            <tr>
                                <th scope="col">Select</th>
                                <th scope="col">Name</th>
                                <th scope="col">Status</th>
                                <th scope="col">Match</th>
                                <th scope="col">Experience</th>
                                <th scope="col">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {items.map((item) => (
                                <tr key={item.applicationId}>
                                    <td data-label="Select">
                                        <input
                                            type="checkbox"
                                            checked={selected.includes(item.applicationId)}
                                            onChange={() => toggleSelect(item.applicationId)}
                                            aria-label={`Select ${item.candidateName}`}
                                        />
                                    </td>
                                    <td data-label="Name">{item.candidateName}</td>
                                    <td data-label="Status"><StatusBadge label={friendlyStatus(item.status)} /></td>
                                    <td data-label="Match">{item.matchScore ?? "—"}</td>
                                    <td data-label="Experience">{item.yearsOfExperience ?? "—"}</td>
                                    <td data-label="Actions">
                                        <Link to={`/recruiter/applications/${item.applicationId}`}>Review</Link>
                                        {" · "}
                                        <button
                                            type="button"
                                            className="rec-btn secondary"
                                            onClick={() => setConfirmAction({
                                                id: item.applicationId,
                                                status: "UnderReview"
                                            })}
                                        >
                                            Screening
                                        </button>
                                        {" · "}
                                        <button
                                            type="button"
                                            className="rec-btn secondary"
                                            onClick={() => setConfirmAction({
                                                id: item.applicationId,
                                                status: "Shortlisted"
                                            })}
                                        >
                                            Shortlist
                                        </button>
                                        {" · "}
                                        <button
                                            type="button"
                                            className="rec-btn danger"
                                            onClick={() => setConfirmAction({
                                                id: item.applicationId,
                                                status: "Rejected"
                                            })}
                                        >
                                            Reject
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </main>
    );
}
