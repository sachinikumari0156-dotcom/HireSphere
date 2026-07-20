import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth";
import "./Login.css";

function Login() {
    const navigate = useNavigate();
    const { login, roleHome } = useAuth();

    const [user, setUser] = useState({
        email: "",
        password: ""
    });

    const [showPassword, setShowPassword] = useState(false);
    const [errors, setErrors] = useState({});
    const [banner, setBanner] = useState(null);
    const [submitting, setSubmitting] = useState(false);

    const updateField = (field) => (e) => {
        setUser((prev) => ({ ...prev, [field]: e.target.value }));
        if (errors[field]) {
            setErrors((prev) => ({ ...prev, [field]: null }));
        }
    };

    const validate = () => {
        const next = {};
        if (!user.email.trim()) next.email = "Enter your email.";
        if (!user.password) next.password = "Enter your password.";
        setErrors(next);
        return Object.keys(next).length === 0;
    };

    const handleLogin = async (e) => {
        e.preventDefault();
        setBanner(null);
        if (!validate()) return;

        setSubmitting(true);
        try {
            const loggedIn = await login(user.email, user.password);
            setBanner({
                type: "success",
                text: "Login successful. Redirecting..."
            });
            setTimeout(() => navigate(roleHome(loggedIn.role)), 800);
        } catch (error) {
            setBanner({
                type: "error",
                text: error.response?.data?.message || "Invalid email or password."
            });
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="reg-page">
            <div className="reg-brand">
                <div className="reg-brand-mark">
                    Hire<span>Sphere</span>
                </div>
                <div className="reg-brand-copy">
                    <span className="reg-eyebrow">Welcome back</span>
                    <h1 className="reg-headline">Pick up right where you left off.</h1>
                    <p className="reg-subcopy">
                        Sign in to check your applications, manage your listings, and stay on top
                        of every conversation.
                    </p>
                </div>
                <div className="reg-brand-footer">
                    &copy; {new Date().getFullYear()} HireSphere
                </div>
            </div>

            <div className="reg-form-side">
                <div className="reg-card">
                    <h1>Sign in</h1>
                    <p className="reg-lead">
                        New here? <Link to="/register">Create a candidate account</Link>
                        <br />
                        Recruiter access? <Link to="/recruiter-request">Request access</Link>
                    </p>

                    {banner && (
                        <div className={`reg-banner ${banner.type}`}>
                            {banner.text}
                        </div>
                    )}

                    <form onSubmit={handleLogin} noValidate>
                        <div className="field">
                            <label htmlFor="login-email">Email</label>
                            <input
                                id="login-email"
                                type="email"
                                placeholder="jane@example.com"
                                value={user.email}
                                onChange={updateField("email")}
                            />
                            {errors.email && (
                                <div className="field-error-msg" role="alert">{errors.email}</div>
                            )}
                        </div>

                        <div className="field">
                            <label htmlFor="login-password">Password</label>
                            <div className="password-wrap">
                                <input
                                    id="login-password"
                                    type={showPassword ? "text" : "password"}
                                    placeholder="Enter your password"
                                    value={user.password}
                                    onChange={updateField("password")}
                                />
                                <button
                                    type="button"
                                    className="password-toggle"
                                    onClick={() => setShowPassword((prev) => !prev)}
                                >
                                    {showPassword ? "Hide" : "Show"}
                                </button>
                            </div>
                            {errors.password && (
                                <div className="field-error-msg" role="alert">{errors.password}</div>
                            )}
                        </div>

                        <button
                            type="submit"
                            className="reg-submit"
                            disabled={submitting}
                        >
                            {submitting ? "Signing in..." : "Sign in"}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
}

export default Login;
