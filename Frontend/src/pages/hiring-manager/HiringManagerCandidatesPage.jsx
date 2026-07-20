import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

const MAX_COMPARE = 5;

export default function HiringManagerCandidatesPage() {
    const { id } = useParams();
    const [items, setItems] = useState([]);
    const [selected, setSelected] = useState([]);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/hiring-manager/jobs/${id}/candidates`);
                if (alive) setItems(response.data.items || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load candidates.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    function toggle(applicationId) {
        setSelected((prev) => {
            if (prev.includes(applicationId)) {
                return prev.filter((x) => x !== applicationId);
            }
            if (prev.length >= MAX_COMPARE) {
                setError(`Comparison is limited to ${MAX_COMPARE} candidates.`);
                return prev;
            }
            setError(null);
            return [...prev, applicationId];
        });
    }

    return (
        <div className="hm-page">
            <h2>Candidates</h2>
            <p className="hm-muted">Authorized applicants for this assigned vacancy.</p>
            {error && <p className="hm-error" role="alert">{error}</p>}
            {loading && <p>Loading candidates…</p>}
            {!loading && items.length === 0 && <p className="hm-muted">No candidates yet.</p>}
            {selected.length > 0 && (
                <Link className="hm-btn" to={`/hiring-manager/compare?ids=${selected.join(",")}`}>
                    Compare selected ({selected.length})
                </Link>
            )}
            {!loading && items.length > 0 && (
                <table className="hm-table">
                    <thead>
                        <tr>
                            <th>Select</th>
                            <th>Name</th>
                            <th>Status</th>
                            <th>Match</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {items.map((item) => (
                            <tr key={item.applicationId}>
                                <td data-label="Select">
                                    <input
                                        type="checkbox"
                                        checked={selected.includes(item.applicationId)}
                                        onChange={() => toggle(item.applicationId)}
                                        aria-label={`Select ${item.candidateName}`}
                                    />
                                </td>
                                <td data-label="Name">{item.candidateName}</td>
                                <td data-label="Status">{item.status}</td>
                                <td data-label="Match">{item.matchScore ?? "—"}</td>
                                <td data-label="Actions">
                                    <Link to={`/hiring-manager/applications/${item.applicationId}`}>Review</Link>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}
