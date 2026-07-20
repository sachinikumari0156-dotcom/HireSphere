import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";

export default function AdminRecruiterRequestsPage() {
    const [items, setItems] = useState([]);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);

    async function load() {
        const response = await api.get("/admin/recruiter-requests");
        setItems(response.data || []);
    }

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get("/admin/recruiter-requests");
                if (alive) setItems(response.data || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load requests.");
            }
        })();
        return () => { alive = false; };
    }, []);

    async function approve(id) {
        setError(null);
        setSuccess(null);
        try {
            await api.post(`/admin/recruiter-requests/${id}/approve`, { notes: "Approved via Admin portal" });
            setSuccess(`Request ${id} approved.`);
            await load();
        } catch (err) {
            setError(err.response?.data?.message || "Approve failed.");
        }
    }

    async function reject(id) {
        const reason = window.prompt("Rejection reason?");
        if (!reason) return;
        setError(null);
        try {
            await api.post(`/admin/recruiter-requests/${id}/reject`, { notes: reason });
            setSuccess(`Request ${id} rejected.`);
            await load();
        } catch (err) {
            setError(err.response?.data?.message || "Reject failed.");
        }
    }

    return (
        <main className="admin-page">
            <h2>Recruiter access requests</h2>
            {error && <p className="admin-error" role="alert">{error}</p>}
            {success && <p className="admin-success" role="status">{success}</p>}
            {items.length === 0 && <p className="admin-muted">No recruiter requests.</p>}
            <ul>
                {items.map((r) => (
                    <li key={r.id}>
                        {r.fullName} · {r.businessEmail} · {r.organizationName} · {r.status}
                        {" "}
                        <Link to={`/admin/recruiter-requests/${r.id}`}>Detail</Link>
                        {r.status === "Pending" && (
                            <>
                                {" "}
                                <button type="button" className="admin-btn" onClick={() => approve(r.id)}>Approve</button>
                                {" "}
                                <button type="button" className="admin-btn secondary" onClick={() => reject(r.id)}>Reject</button>
                            </>
                        )}
                    </li>
                ))}
            </ul>
        </main>
    );
}
