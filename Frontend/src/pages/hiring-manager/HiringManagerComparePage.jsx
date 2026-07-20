import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import api from "../../api/axios";

export default function HiringManagerComparePage() {
    const [params] = useSearchParams();
    const idsFromQuery = useMemo(
        () => (params.get("ids") || "").split(",").map((x) => Number(x)).filter(Boolean),
        [params]
    );
    const [idsText, setIdsText] = useState(idsFromQuery.join(","));
    const [result, setResult] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);

    async function compare(ids) {
        setLoading(true);
        setError(null);
        setResult(null);
        try {
            const response = await api.post("/hiring-manager/candidates/compare", {
                applicationIds: ids
            });
            setResult(response.data);
        } catch (err) {
            setError(err.response?.data?.message || "Comparison failed.");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        if (idsFromQuery.length === 0) return undefined;
        let alive = true;
        (async () => {
            setLoading(true);
            setError(null);
            setResult(null);
            try {
                const response = await api.post("/hiring-manager/candidates/compare", {
                    applicationIds: idsFromQuery
                });
                if (alive) setResult(response.data);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Comparison failed.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [idsFromQuery]);

    return (
        <div className="hm-page">
            <h2>Candidate comparison</h2>
            <p className="hm-muted">Compare up to 5 candidates from the same assigned vacancy. Ranking is advisory.</p>
            <form
                className="hm-form"
                onSubmit={(e) => {
                    e.preventDefault();
                    const ids = idsText.split(",").map((x) => Number(x.trim())).filter(Boolean);
                    compare(ids);
                }}
            >
                <label>
                    Application IDs (comma-separated)
                    <input value={idsText} onChange={(e) => setIdsText(e.target.value)} />
                </label>
                <button type="submit" className="hm-btn">Compare</button>
            </form>
            {loading && <p>Loading comparison…</p>}
            {error && <p className="hm-error" role="alert">{error}</p>}
            {result && (
                <>
                    <p className="hm-notice" role="note">{result.humanReviewNotice}</p>
                    <p>Job: {result.jobTitle}</p>
                    <div className="hm-compare-grid">
                        {(result.candidates || []).map((c) => (
                            <article key={c.applicationId}>
                                <h3>{c.candidateName}</h3>
                                <p>Score: {c.matchScore ?? "—"}</p>
                                <p>Status: {c.status}</p>
                                <p>Skills: {(c.skills || []).join(", ")}</p>
                                <p>Missing: {(c.missingRequiredSkills || []).join(", ") || "None"}</p>
                            </article>
                        ))}
                    </div>
                </>
            )}
        </div>
    );
}
