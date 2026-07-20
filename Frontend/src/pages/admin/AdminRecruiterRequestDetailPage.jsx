import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

export default function AdminRecruiterRequestDetailPage() {
    const { id } = useParams();
    const [item, setItem] = useState(null);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);

    useEffect(() => {
        api.get(`/admin/recruiter-requests/${id}`)
            .then((r) => setItem(r.data))
            .catch((err) => setError(err.response?.data?.message || "Not found."));
    }, [id]);

    async function approve() {
        try {
            await api.post(`/admin/recruiter-requests/${id}/approve`, {});
            setSuccess("Approved.");
            const r = await api.get(`/admin/recruiter-requests/${id}`);
            setItem(r.data);
        } catch (err) {
            setError(err.response?.data?.message || "Approve failed.");
        }
    }

    async function reject() {
        const notes = window.prompt("Rejection reason?");
        if (!notes) return;
        try {
            await api.post(`/admin/recruiter-requests/${id}/reject`, { notes });
            setSuccess("Rejected.");
            const r = await api.get(`/admin/recruiter-requests/${id}`);
            setItem(r.data);
        } catch (err) {
            setError(err.response?.data?.message || "Reject failed.");
        }
    }

    if (!item && !error) return <div className="admin-page"><p>Loading…</p></div>;
    if (error && !item) return <div className="admin-page"><p className="admin-error">{error}</p></div>;

    return (
        <div className="admin-page">
            <h2>Recruiter request</h2>
            <p>{item.fullName} · {item.businessEmail}</p>
            <p>Organization: {item.organizationName}</p>
            <p>Status: {item.status}</p>
            <p>{item.message}</p>
            {error && <p className="admin-error" role="alert">{error}</p>}
            {success && <p className="admin-success" role="status">{success}</p>}
            {item.status === "Pending" && (
                <>
                    <button type="button" className="admin-btn" onClick={approve}>Approve</button>
                    {" "}
                    <button type="button" className="admin-btn secondary" onClick={reject}>Reject</button>
                </>
            )}
            <p><Link to="/admin/recruiter-requests">Back</Link></p>
        </div>
    );
}
