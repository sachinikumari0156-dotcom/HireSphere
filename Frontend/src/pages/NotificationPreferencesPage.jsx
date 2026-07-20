import { useEffect, useState } from "react";
import api from "../api/axios";
import "./NotificationPreferencesPage.css";

export default function NotificationPreferencesPage() {
    const [prefs, setPrefs] = useState(null);
    const [deliveries, setDeliveries] = useState([]);
    const [error, setError] = useState(null);
    const [saved, setSaved] = useState(false);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const [p, d] = await Promise.all([
                    api.get("/users/notification-preferences"),
                    api.get("/notifications/deliveries")
                ]);
                if (!alive) return;
                setPrefs(p.data);
                setDeliveries(d.data || []);
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load preferences.");
            }
        })();
        return () => { alive = false; };
    }, []);

    async function save(e) {
        e.preventDefault();
        setSaved(false);
        setError(null);
        try {
            const response = await api.put("/users/notification-preferences", prefs);
            setPrefs(response.data);
            setSaved(true);
        } catch (err) {
            setError(err.response?.data?.message || "Could not save preferences.");
        }
    }

    if (!prefs && !error) return <main className="pref-page"><p>Loading preferences…</p></main>;
    if (error && !prefs) return <main className="pref-page"><p className="pref-error">{error}</p></main>;

    return (
        <main className="pref-page">
            <h1>Notification preferences</h1>
            <p className="pref-muted">Security messages cannot be disabled. Optional channels require consent.</p>
            <form onSubmit={save} className="pref-form">
                <label>
                    <input
                        type="checkbox"
                        checked={!!prefs.emailEnabled}
                        onChange={(e) => setPrefs({ ...prefs, emailEnabled: e.target.checked })}
                    />
                    Email notifications
                </label>
                <label>
                    <input
                        type="checkbox"
                        checked={!!prefs.applicationUpdates}
                        onChange={(e) => setPrefs({ ...prefs, applicationUpdates: e.target.checked })}
                    />
                    Application updates
                </label>
                <label>
                    <input
                        type="checkbox"
                        checked={!!prefs.assessmentReminders}
                        onChange={(e) => setPrefs({ ...prefs, assessmentReminders: e.target.checked })}
                    />
                    Assessment reminders
                </label>
                <label>
                    <input
                        type="checkbox"
                        checked={!!prefs.interviewReminders}
                        onChange={(e) => setPrefs({ ...prefs, interviewReminders: e.target.checked })}
                    />
                    Interview reminders
                </label>
                <label>
                    <input
                        type="checkbox"
                        checked={!!prefs.smsEnabled}
                        onChange={(e) => setPrefs({ ...prefs, smsEnabled: e.target.checked })}
                    />
                    SMS channel
                </label>
                <label>
                    <input
                        type="checkbox"
                        checked={!!prefs.smsConsent}
                        onChange={(e) => setPrefs({ ...prefs, smsConsent: e.target.checked })}
                    />
                    SMS consent (required for Development Mock SMS)
                </label>
                {prefs.smsEnabled && !prefs.smsConsent && (
                    <p className="pref-error" role="alert">Consent is required before SMS can be sent.</p>
                )}
                <button type="submit">Save preferences</button>
                {saved && <p className="pref-ok">Preferences saved.</p>}
                {error && <p className="pref-error">{error}</p>}
            </form>

            <h2>Recent deliveries</h2>
            {deliveries.length === 0 && <p className="pref-muted">No delivery records yet.</p>}
            <ul className="pref-deliveries">
                {deliveries.map((d) => (
                    <li key={d.id}>
                        <strong>{d.notificationType}</strong> — {d.channel} — {d.status}
                        {d.provider && <> ({d.provider})</>}
                        {d.safeFailureCode && <> — {d.safeFailureCode}</>}
                    </li>
                ))}
            </ul>
        </main>
    );
}
