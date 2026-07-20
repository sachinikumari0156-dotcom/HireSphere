import { useEffect, useState } from "react";
import api from "../../api/axios";

export default function AdminRolesPage() {
    const [roles, setRoles] = useState([]);
    const [permissions, setPermissions] = useState([]);
    const [selected, setSelected] = useState(null);
    const [checked, setChecked] = useState([]);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);

    useEffect(() => {
        Promise.all([api.get("/admin/roles"), api.get("/admin/permissions")])
            .then(([r, p]) => {
                setRoles(r.data || []);
                setPermissions(p.data || []);
            })
            .catch((err) => setError(err.response?.data?.message || "Could not load roles."));
    }, []);

    function selectRole(role) {
        setSelected(role);
        setChecked(role.permissionIds || []);
        setSuccess(null);
    }

    function toggle(id) {
        setChecked((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));
    }

    async function save() {
        if (!selected) return;
        setError(null);
        try {
            await api.put(`/admin/roles/${selected.id}/permissions`, { permissionIds: checked });
            setSuccess("Permissions updated.");
            const r = await api.get("/admin/roles");
            setRoles(r.data || []);
        } catch (err) {
            setError(err.response?.data?.message || "Update failed.");
        }
    }

    return (
        <div className="admin-page">
            <h2>Roles and permissions</h2>
            {error && <p className="admin-error" role="alert">{error}</p>}
            {success && <p className="admin-success" role="status">{success}</p>}
            <ul>
                {roles.map((r) => (
                    <li key={r.id}>
                        <button type="button" className="admin-btn secondary" onClick={() => selectRole(r)}>
                            {r.name}
                        </button>
                    </li>
                ))}
            </ul>
            {selected && (
                <section>
                    <h3>{selected.name} permissions</h3>
                    {permissions.map((p) => (
                        <label key={p.id} style={{ display: "block" }}>
                            <input
                                type="checkbox"
                                checked={checked.includes(p.id)}
                                onChange={() => toggle(p.id)}
                            />
                            {" "}
                            {p.code} — {p.name}
                        </label>
                    ))}
                    <button type="button" className="admin-btn" onClick={save}>Save permissions</button>
                </section>
            )}
        </div>
    );
}
