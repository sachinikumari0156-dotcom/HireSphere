import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

export default function RecruiterInterviewDetailPage() {
    const { id } = useParams();
    const [item, setItem] = useState(null);
    const [error, setError] = useState(null);
    const [message, setMessage] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/recruiter/interviews/${id}`);
                if (alive) setItem(response.data);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load interview.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    async function cancelInterview() {
        setError(null);
        try {
            const response = await api.patch(`/recruiter/interviews/${id}/status`, {
                status: "Cancelled",
                notes: "Cancelled by recruiter"
            });
            setItem(response.data);
            setMessage("Interview cancelled.");
        } catch (err) {
            setError(err.response?.data?.message || "Cancel failed.");
        }
    }

    async function downloadIcs() {
        setError(null);
        try {
            const response = await api.get(`/interviews/${id}/calendar.ics`, { responseType: "blob" });
            const url = URL.createObjectURL(response.data);
            const a = document.createElement("a");
            a.href = url;
            a.download = `interview-${id}.ics`;
            a.click();
            URL.revokeObjectURL(url);
            setMessage("ICS downloaded (Internal/ICS providers). Google and Outlook remain Not Configured without OAuth.");
        } catch (err) {
            setError(err.response?.data?.message || "ICS download failed.");
        }
    }

    async function syncInternalCalendar() {
        setError(null);
        try {
            const response = await api.post(`/interviews/${id}/calendar/sync`);
            setMessage(`Calendar sync: ${response.data.calendarSyncStatus}. Google: ${response.data.googleCalendar}. Outlook: ${response.data.outlookCalendar}.`);
            setItem((prev) => prev ? { ...prev, calendarSyncStatus: response.data.calendarSyncStatus } : prev);
        } catch (err) {
            setError(err.response?.data?.message || "Calendar sync failed.");
        }
    }

    if (loading) return <main className="rec-page"><p>Loading interview…</p></main>;
    if (error && !item) return <main className="rec-page"><p className="rec-error">{error}</p></main>;
    if (!item) return null;

    return (
        <main className="rec-page">
            <h2>Interview detail</h2>
            {message && <p className="rec-success">{message}</p>}
            {error && <p className="rec-error">{error}</p>}
            <p>{item.candidateName} · {item.jobTitle}</p>
            <p>
                {new Date(item.startAtUtc).toLocaleString()} – {new Date(item.endAtUtc).toLocaleString()}
                {" "}({item.timeZoneId})
            </p>
            <p>Status: {item.status} · Candidate response: {item.candidateResponse}</p>
            {item.candidateResponseReason && <p>Candidate reason: {item.candidateResponseReason}</p>}
            <p>Calendar sync: {item.calendarSyncStatus}</p>
            <p className="rec-muted">Google Calendar: Not Configured · Outlook Calendar: Not Configured (without OAuth credentials)</p>
            <p>Internal notes: {item.internalNotes || "—"}</p>
            <div className="rec-actions">
                <button type="button" className="rec-btn" onClick={downloadIcs}>Download ICS</button>
                <button type="button" className="rec-btn secondary" onClick={syncInternalCalendar}>Sync internal calendar</button>
                <button type="button" className="rec-btn danger" onClick={cancelInterview}>Cancel</button>
                <Link className="rec-btn secondary" to="/recruiter/interviews">Back</Link>
            </div>
        </main>
    );
}
