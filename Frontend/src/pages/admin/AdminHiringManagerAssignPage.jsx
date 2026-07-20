import { useState } from "react";
import api from "../../api/axios";

export default function AdminHiringManagerAssignPage() {
    const [userId, setUserId] = useState("");
    const [organizationId, setOrganizationId] = useState("");
    const [departmentId, setDepartmentId] = useState("");
    const [jobId, setJobId] = useState("");
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);

    async function submit(e) {
        e.preventDefault();
        setError(null);
        setSuccess(null);
        try {
            await api.post("/admin/hiring-managers/assign", {
                userId: Number(userId),
                organizationId: organizationId ? Number(organizationId) : null,
                departmentId: departmentId ? Number(departmentId) : null,
                jobId: jobId ? Number(jobId) : null
            });
            setSuccess("Hiring Manager assignment updated.");
        } catch (err) {
            setError(err.response?.data?.message || "Assignment failed.");
        }
    }

    return (
        <main className="admin-page">
            <h2>Hiring Manager assignment</h2>
            <p className="admin-muted">Assign Hiring Manager role, organization/department, and optional vacancy.</p>
            {error && <p className="admin-error" role="alert">{error}</p>}
            {success && <p className="admin-success" role="status">{success}</p>}
            <form className="admin-form" onSubmit={submit}>
                <label>
                    User ID
                    <input required value={userId} onChange={(e) => setUserId(e.target.value)} />
                </label>
                <label>
                    Organization ID
                    <input value={organizationId} onChange={(e) => setOrganizationId(e.target.value)} />
                </label>
                <label>
                    Department ID
                    <input value={departmentId} onChange={(e) => setDepartmentId(e.target.value)} />
                </label>
                <label>
                    Job ID (optional)
                    <input value={jobId} onChange={(e) => setJobId(e.target.value)} />
                </label>
                <button type="submit" className="admin-btn">Assign</button>
            </form>
        </main>
    );
}
