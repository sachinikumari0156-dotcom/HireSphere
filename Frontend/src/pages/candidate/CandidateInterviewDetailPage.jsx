import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

export default function CandidateInterviewDetailPage() {
    const { id } = useParams();
    const [interview, setInterview] = useState(null);
    const [error, setError] = useState(null);
    const [actionError, setActionError] = useState(null);
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);
    const [reason, setReason] = useState("");
    const [preferred, setPreferred] = useState("");

    async function load() {
        const response = await api.get(`/candidate/interviews/${id}`);
        setInterview(response.data);
    }

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                await load();
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load interview.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [id]);

    async function confirm() {
        setActionError(null);
        setBusy(true);
        try {
            const response = await api.post(`/candidate/interviews/${id}/confirm`);
            setInterview(response.data);
        } catch (err) {
            setActionError(err.response?.data?.message || "Could not confirm interview.");
        } finally {
            setBusy(false);
        }
    }

    async function requestReschedule() {
        setActionError(null);
        setBusy(true);
        try {
            const response = await api.post(`/candidate/interviews/${id}/reschedule-request`, {
                reason,
                preferredTimesNote: preferred
            });
            setInterview(response.data);
            setReason("");
            setPreferred("");
        } catch (err) {
            setActionError(err.response?.data?.message || "Could not request reschedule.");
        } finally {
            setBusy(false);
        }
    }

    async function decline() {
        setActionError(null);
        setBusy(true);
        try {
            const response = await api.post(`/candidate/interviews/${id}/decline`, { reason });
            setInterview(response.data);
            setReason("");
        } catch (err) {
            setActionError(err.response?.data?.message || "Could not decline interview.");
        } finally {
            setBusy(false);
        }
    }

    if (loading) {
        return <div className="dash-page"><p>Loading interview…</p></div>;
    }

    if (error) {
        return <div className="dash-page"><p className="error">{error}</p></div>;
    }

    return (
        <div className="dash-page">
            <header className="dash-header">
                <h1>{interview.jobTitle}</h1>
                <p>
                    {new Date(interview.interviewDateUtc).toLocaleString()} ({interview.timeZoneId})
                </p>
                <nav className="dash-nav">
                    <Link to="/candidate/interviews">All interviews</Link>
                    <Link to={`/candidate/applications/${interview.applicationId}`}>Application</Link>
                </nav>
            </header>

            <section className="job-box">
                <p>Type: {interview.interviewType}</p>
                <p>Status: {interview.status}</p>
                <p>Your response: {interview.candidateResponse}</p>
                {interview.candidateResponseReason && (
                    <p>Reason: {interview.candidateResponseReason}</p>
                )}
            </section>

            <section className="job-box">
                <h2>Meeting info</h2>
                {interview.meetingInfoAvailable ? (
                    <>
                        {interview.meetingLink ? (
                            <p>
                                Link:{" "}
                                <a href={interview.meetingLink} target="_blank" rel="noreferrer">
                                    {interview.meetingLink}
                                </a>
                            </p>
                        ) : (
                            <p>No meeting link provided.</p>
                        )}
                        {interview.meetingInstructions && <p>{interview.meetingInstructions}</p>}
                    </>
                ) : (
                    <p className="empty-state">
                        Meeting details are available after you confirm this interview.
                    </p>
                )}
            </section>

            {(interview.canConfirm || interview.canRequestReschedule || interview.canDecline) && (
                <section className="job-box">
                    <h2>Respond</h2>
                    {(interview.canRequestReschedule || interview.canDecline) && (
                        <>
                            <label>
                                Reason
                                <textarea
                                    value={reason}
                                    onChange={(e) => setReason(e.target.value)}
                                    rows={3}
                                    style={{ width: "100%", display: "block" }}
                                />
                            </label>
                            {interview.canRequestReschedule && (
                                <label>
                                    Preferred times (optional)
                                    <input
                                        type="text"
                                        value={preferred}
                                        onChange={(e) => setPreferred(e.target.value)}
                                        style={{ width: "100%", display: "block" }}
                                    />
                                </label>
                            )}
                        </>
                    )}
                    <div className="wizard-actions">
                        {interview.canConfirm && (
                            <button type="button" onClick={confirm} disabled={busy}>Confirm</button>
                        )}
                        {interview.canRequestReschedule && (
                            <button type="button" onClick={requestReschedule} disabled={busy}>
                                Request reschedule
                            </button>
                        )}
                        {interview.canDecline && (
                            <button type="button" onClick={decline} disabled={busy}>Decline</button>
                        )}
                    </div>
                </section>
            )}

            {actionError && <p className="error">{actionError}</p>}
        </div>
    );
}
