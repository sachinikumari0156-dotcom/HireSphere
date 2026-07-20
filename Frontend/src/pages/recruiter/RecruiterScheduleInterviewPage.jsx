import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../../api/axios";

export default function RecruiterScheduleInterviewPage() {
    const navigate = useNavigate();
    const [applicationId, setApplicationId] = useState("");
    const [startLocal, setStartLocal] = useState("");
    const [durationMinutes, setDuration] = useState(60);
    const [timeZoneId, setTimeZoneId] = useState("Asia/Colombo");
    const [interviewType, setType] = useState("Video");
    const [meetingLink, setMeetingLink] = useState("");
    const [instructions, setInstructions] = useState("");
    const [internalNotes, setInternalNotes] = useState("");
    const [conflicts, setConflicts] = useState([]);
    const [error, setError] = useState(null);
    const [saving, setSaving] = useState(false);

    async function submit(e, force = false) {
        e.preventDefault();
        setError(null);
        setConflicts([]);
        if (!applicationId || !startLocal) {
            setError("Application ID and start time are required.");
            return;
        }
        if (Number(durationMinutes) < 15) {
            setError("Duration must be at least 15 minutes.");
            return;
        }
        setSaving(true);
        try {
            const response = await api.post("/recruiter/interviews", {
                applicationId: Number(applicationId),
                startAtUtc: new Date(startLocal).toISOString(),
                durationMinutes: Number(durationMinutes),
                timeZoneId,
                interviewType,
                meetingLink,
                meetingInstructions: instructions,
                internalNotes,
                forceDespiteConflicts: force
            });
            if (!response.data.scheduled) {
                setConflicts(response.data.conflicts || []);
                setError("Conflicts detected. Review warnings or force schedule.");
                return;
            }
            navigate(`/recruiter/interviews/${response.data.interview.id}`);
        } catch (err) {
            setError(err.response?.data?.message || "Could not schedule interview.");
        } finally {
            setSaving(false);
        }
    }

    return (
        <main className="rec-page">
            <h2>Schedule interview</h2>
            <p className="rec-muted">Times are stored in UTC with timezone metadata. Calendar providers are Not Configured.</p>
            {error && <p className="rec-error" role="alert">{error}</p>}
            {conflicts.length > 0 && (
                <div className="rec-notice" role="alert">
                    <p>Conflict warning</p>
                    <ul>
                        {conflicts.map((c, i) => (
                            <li key={`${c.conflictType}-${c.conflictingInterviewId}-${i}`}>
                                {c.conflictType}: {c.message}
                            </li>
                        ))}
                    </ul>
                    <button type="button" className="rec-btn danger" onClick={(e) => submit(e, true)}>
                        Schedule anyway
                    </button>
                </div>
            )}
            <form className="rec-form" onSubmit={(e) => submit(e, false)}>
                <label>
                    Application ID
                    <input value={applicationId} onChange={(e) => setApplicationId(e.target.value)} />
                </label>
                <label>
                    Start (local browser input → UTC)
                    <input type="datetime-local" value={startLocal} onChange={(e) => setStartLocal(e.target.value)} />
                </label>
                <label>
                    Duration (minutes)
                    <input type="number" min="15" value={durationMinutes} onChange={(e) => setDuration(e.target.value)} />
                </label>
                <label>
                    Timezone
                    <input value={timeZoneId} onChange={(e) => setTimeZoneId(e.target.value)} />
                </label>
                <label>
                    Type
                    <select value={interviewType} onChange={(e) => setType(e.target.value)}>
                        <option>Video</option>
                        <option>Phone</option>
                        <option>OnSite</option>
                    </select>
                </label>
                <label>
                    Meeting link
                    <input value={meetingLink} onChange={(e) => setMeetingLink(e.target.value)} />
                </label>
                <label>
                    Candidate-safe instructions
                    <textarea rows={2} value={instructions} onChange={(e) => setInstructions(e.target.value)} />
                </label>
                <label>
                    Internal notes (not visible to candidate)
                    <textarea rows={2} value={internalNotes} onChange={(e) => setInternalNotes(e.target.value)} />
                </label>
                <button type="submit" className="rec-btn" disabled={saving}>
                    {saving ? "Scheduling…" : "Schedule"}
                </button>
            </form>
            <div className="rec-actions">
                <Link className="rec-btn secondary" to="/recruiter/interviews">Back</Link>
            </div>
        </main>
    );
}
