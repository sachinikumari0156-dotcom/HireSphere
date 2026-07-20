import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";

export default function RecruiterMessageThreadPage() {
    const { id } = useParams();
    const [thread, setThread] = useState(null);
    const [body, setBody] = useState("");
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);
    const [sending, setSending] = useState(false);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/recruiter/applications/${id}/messages`);
                if (!alive) return;
                setThread(response.data);
                await api.post(`/recruiter/applications/${id}/messages/read`);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load messages.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id]);

    async function load() {
        const response = await api.get(`/recruiter/applications/${id}/messages`);
        setThread(response.data);
        await api.post(`/recruiter/applications/${id}/messages/read`);
    }

    async function send(e) {
        e.preventDefault();
        setSending(true);
        setError(null);
        try {
            await api.post(`/recruiter/applications/${id}/messages`, { body });
            setBody("");
            await load();
        } catch (err) {
            setError(err.response?.data?.message || "Send failed.");
        } finally {
            setSending(false);
        }
    }

    return (
        <div className="rec-page">
            <h2>Application messages</h2>
            <p className="rec-muted">In-app thread only. External email/SMS delivery is not configured in Phase 5.</p>
            {loading && <p>Loading messages…</p>}
            {error && <p className="rec-error" role="alert">{error}</p>}
            {!loading && (thread?.messages?.length ?? 0) === 0 && (
                <p className="rec-muted">No messages yet.</p>
            )}
            <ul className="rec-activity">
                {(thread?.messages || []).map((m) => (
                    <li key={m.id}>
                        <strong>{m.senderRole}</strong> · {new Date(m.sentAtUtc).toLocaleString()}
                        <div>{m.body}</div>
                    </li>
                ))}
            </ul>
            <form className="rec-form" onSubmit={send}>
                <label>
                    Message
                    <textarea rows={3} value={body} onChange={(e) => setBody(e.target.value)} required />
                </label>
                <button type="submit" className="rec-btn" disabled={sending}>
                    {sending ? "Sending…" : "Send message"}
                </button>
            </form>
            <div className="rec-actions">
                <Link className="rec-btn secondary" to={`/recruiter/applications/${id}`}>Back</Link>
            </div>
        </div>
    );
}
