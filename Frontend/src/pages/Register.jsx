import { useState } from "react";
import api from "../api/axios";
import "./Register.css";

const ROLES = [
    {
        id: "Candidate",
        title: "Candidate",
        desc: "Find and apply to jobs",
    },
    {
        id: "Recruiter",
        title: "Recruiter",
        desc: "Post jobs, hire talent",
    },
];

function Register() {
    const [user, setUser] = useState({
        fullName: "",
        email: "",
        password: "",
        role: "Candidate",
    });
    const [showPassword, setShowPassword] = useState(false);
    const [errors, setErrors] = useState({});
    const [banner, setBanner] = useState(null); // { type: "error" | "success", text }
    const [submitting, setSubmitting] = useState(false);

    const updateField = (field) => (e) => {
        setUser({ ...user, [field]: e.target.value });
        if (errors[field]) setErrors({ ...errors, [field]: null });
    };

    const validate = () => {
        const next = {};
        if (!user.fullName.trim()) next.fullName = "Enter your full name.";
        if (!user.email.trim()) {
            next.email = "Enter your email.";
        } else if (!/^\S+@\S+\.\S+$/.test(user.email)) {
            next.email = "Enter a valid email address.";
        }
        if (!user.password) {
            next.password = "Create a password.";
        } else if (user.password.length < 6) {
            next.password = "Use at least 6 characters.";
        }
        setErrors(next);
        return Object.keys(next).length === 0;
    };

    const handleRegister = async (e) => {
        e.preventDefault();
        setBanner(null);
        if (!validate()) return;

        setSubmitting(true);
        try {
            const response = await api.post("/Auth/Register", user);
            setBanner({ type: "success", text: "Account created. You can now sign in." });
            console.log(response.data);
        } catch (error) {
            console.log("FULL ERROR:", error);
            console.log("RESPONSE DATA:", error.response?.data);
            console.log("STATUS:", error.response?.status);
            const message =
                error.response?.data?.message ||
                (typeof error.response?.data === "string" ? error.response.data : null) ||
                "Registration failed. Please check your details and try again.";
            setBanner({ type: "error", text: message });
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="reg-page">
            <div className="reg-brand" data-role={user.role}>
                <div className="reg-brand-mark">
                    hire<span>flow</span>
                </div>
                <div className="reg-brand-copy">
                    <span className="reg-eyebrow">
                        {user.role === "Recruiter" ? "For recruiters" : "For candidates"}
                    </span>
                    <h1 className="reg-headline">
                        {user.role === "Recruiter"
                            ? "Post roles and reach the right people, faster."
                            : "Find your next role, without the noise."}
                    </h1>
                    <p className="reg-subcopy">
                        {user.role === "Recruiter"
                            ? "Manage listings, review applicants, and move candidates through your pipeline in one place."
                            : "Build a profile once, apply in a click, and track every application in one place."}
                    </p>
                </div>
                <div className="reg-brand-footer">&copy; {new Date().getFullYear()} Hireflow</div>
            </div>

            <div className="reg-form-side">
                <div className="reg-card">
                    <h1>Create your account</h1>
                    <p className="reg-lead">
                        Already have one? <a href="/login">Sign in</a>
                    </p>

                    {banner && (
                        <div className={`reg-banner ${banner.type}`}>{banner.text}</div>
                    )}

                    <form onSubmit={handleRegister} noValidate>
                        <div className="role-toggle">
                            {ROLES.map((r) => (
                                <button
                                    type="button"
                                    key={r.id}
                                    className="role-option"
                                    data-active={user.role === r.id}
                                    data-role={r.id}
                                    onClick={() => setUser({ ...user, role: r.id })}
                                >
                                    <span className="role-title">{r.title}</span>
                                    <span className="role-desc">{r.desc}</span>
                                </button>
                            ))}
                        </div>

                        <div className={`field ${errors.fullName ? "field-error" : ""}`}>
                            <label htmlFor="fullName">Full name</label>
                            <input
                                id="fullName"
                                placeholder="Jane Perera"
                                value={user.fullName}
                                onChange={updateField("fullName")}
                            />
                            {errors.fullName && (
                                <div className="field-error-msg">{errors.fullName}</div>
                            )}
                        </div>

                        <div className={`field ${errors.email ? "field-error" : ""}`}>
                            <label htmlFor="email">Email</label>
                            <input
                                id="email"
                                type="email"
                                placeholder="jane@example.com"
                                value={user.email}
                                onChange={updateField("email")}
                            />
                            {errors.email && (
                                <div className="field-error-msg">{errors.email}</div>
                            )}
                        </div>

                        <div className={`field ${errors.password ? "field-error" : ""}`}>
                            <label htmlFor="password">Password</label>
                            <div className="password-wrap">
                                <input
                                    id="password"
                                    type={showPassword ? "text" : "password"}
                                    placeholder="At least 6 characters"
                                    value={user.password}
                                    onChange={updateField("password")}
                                />
                                <button
                                    type="button"
                                    className="password-toggle"
                                    onClick={() => setShowPassword((s) => !s)}
                                >
                                    {showPassword ? "Hide" : "Show"}
                                </button>
                            </div>
                            {errors.password && (
                                <div className="field-error-msg">{errors.password}</div>
                            )}
                        </div>

                        <button
                            type="submit"
                            className="reg-submit"
                            data-role={user.role}
                            disabled={submitting}
                        >
                            {submitting ? "Creating account…" : "Create account"}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
}

export default Register;