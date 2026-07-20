import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import api from "../../api/axios";

export default function RecruiterComparePage() {
    const [params] = useSearchParams();
    const ids = useMemo(
        () => (params.get("ids") || "")
            .split(",")
            .map((x) => Number(x.trim()))
            .filter((x) => Number.isFinite(x) && x > 0),
        [params]
    );
    const [manualIds, setManualIds] = useState(ids.join(","));
    const [result, setResult] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (ids.length < 2) return undefined;
        let alive = true;
        (async () => {
            try {
                const response = await api.post("/recruiter/applications/compare", {
                    applicationIds: ids
                });
                if (alive) setResult(response.data);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Comparison failed.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [ids]);

    async function compare(applicationIds) {
        setLoading(true);
        setError(null);
        setResult(null);
        try {
            if (applicationIds.length < 2 || applicationIds.length > 5) {
                setError("Select between 2 and 5 applicants.");
                return;
            }
            const response = await api.post("/recruiter/applications/compare", {
                applicationIds
            });
            setResult(response.data);
        } catch (err) {
            setError(err.response?.data?.message || "Comparison failed.");
        } finally {
            setLoading(false);
        }
    }

    return (
        <main className="rec-page">
            <h2>Candidate comparison</h2>
            <p className="rec-muted">
                Comparison supports review only and is not an automatic hiring decision.
            </p>

            <form
                className="rec-form"
                onSubmit={(e) => {
                    e.preventDefault();
                    const parsed = manualIds
                        .split(",")
                        .map((x) => Number(x.trim()))
                        .filter((x) => Number.isFinite(x) && x > 0);
                    compare(parsed);
                }}
            >
                <label>
                    Application IDs (comma-separated, max 5)
                    <input value={manualIds} onChange={(e) => setManualIds(e.target.value)} />
                </label>
                <button type="submit" className="rec-btn" disabled={loading}>
                    {loading ? "Comparing…" : "Compare"}
                </button>
            </form>

            {error && <p className="rec-error" role="alert">{error}</p>}
            {result?.notice && <p className="rec-notice">{result.notice}</p>}

            {result?.items?.length > 0 && (
                <div className="rec-compare-grid">
                    {result.items.map((item) => (
                        <article key={item.applicationId}>
                            <h3>{item.candidateName}</h3>
                            <p>Status: {item.applicationStatus}</p>
                            <p>Match: {item.matchScore ?? "—"}</p>
                            <p>Experience: {item.yearsOfExperience ?? "—"} years</p>
                            <p>{item.professionalSummary || "No summary"}</p>
                            <p><strong>Skills:</strong> {(item.skills || []).join(", ") || "—"}</p>
                            <p>
                                <strong>Missing required:</strong>{" "}
                                {(item.missingRequiredSkills || []).join(", ") || "None"}
                            </p>
                            <p>Assessment: {item.assessmentStatus || "—"}</p>
                            <p>Interview: {item.interviewStatus || "—"}</p>
                        </article>
                    ))}
                </div>
            )}
        </main>
    );
}
