import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth";
import "./Register.css";

function Register() {
    const navigate = useNavigate();
    const { registerCandidate, roleHome } = useAuth();

    const [user, setUser] = useState({
        firstName: "",
        lastName: "",
        email: "",
        password: "",
        confirmPassword: "",
        acceptTerms: false
    });

    const [showPassword, setShowPassword] = useState(false);
    const [errors, setErrors] = useState({});
    const [banner, setBanner] = useState(null);
    const [submitting, setSubmitting] = useState(false);

    const updateField = (field) => (e) => {
        const value = e.target.type === "checkbox" ? e.target.checked : e.target.value;
        setUser((prev) => ({ ...prev, [field]: value }));
        if (errors[field]) {
            setErrors((prev) => ({ ...prev, [field]: null }));
        }
    };

    const validate = () => {
        const next = {};
        if (!user.firstName.trim()) next.firstName = "Enter your first name.";
        if (!user.lastName.trim()) next.lastName = "Enter your last name.";
        if (!user.email.trim()) next.email = "Enter your email.";
        else if (!/^\S+@\S+\.\S+$/.test(user.email)) {
            next.email = "Enter a valid email address.";
        }
        if (!user.password) next.password = "Create a password.";
        else if (user.password.length < 8) {
            next.password = "Use at least 8 characters with a letter and a digit.";
        }
        if (user.password !== user.confirmPassword) {
            next.confirmPassword = "Passwords do not match.";
        }
        if (!user.acceptTerms) {
            next.acceptTerms = "You must accept the privacy terms.";
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
            const registered = await registerCandidate(user);
            setBanner({
                type: "success",
                text: "Account created. Redirecting..."
            });
            setTimeout(() => navigate(roleHome(registered.role)), 800);
        } catch (error) {
            setBanner({
                type: "error",
                text:
                    error.response?.data?.message ||
                    (typeof error.response?.data === "string"
                        ? error.response.data
                        : null) ||
                    "Registration failed. Please try again."
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
                    <span className="reg-eyebrow">For candidates</span>
                    <h1 className="reg-headline">Find your next role, without the noise.</h1>
                    <p className="reg-subcopy">
                        Build a profile once, apply in a click, and track every application.
                    </p>
                </div>
                <div className="reg-brand-footer">
                    &copy; {new Date().getFullYear()} HireSphere
                </div>
            </div>

            <div className="reg-form-side">
                <div className="reg-card">
                    <h1>Create your candidate account</h1>
                    <p className="reg-lead">
                        Already have one? <Link to="/login">Sign in</Link>
                        <br />
                        Recruiter? <Link to="/recruiter-request">Request access</Link>
                    </p>

                    {banner && (
                        <div className={`reg-banner ${banner.type}`}>
                            {banner.text}
                        </div>
                    )}

                    <form onSubmit={handleRegister} noValidate>
                        <div className="field">
                            <label>First name</label>
                            <input value={user.firstName} onChange={updateField("firstName")} />
                            {errors.firstName && (
                                <div className="field-error-msg">{errors.firstName}</div>
                            )}
                        </div>

                        <div className="field">
                            <label>Last name</label>
                            <input value={user.lastName} onChange={updateField("lastName")} />
                            {errors.lastName && (
                                <div className="field-error-msg">{errors.lastName}</div>
                            )}
                        </div>

                        <div className="field">
                            <label>Email</label>
                            <input
                                type="email"
                                value={user.email}
                                onChange={updateField("email")}
                            />
                            {errors.email && (
                                <div className="field-error-msg">{errors.email}</div>
                            )}
                        </div>

                        <div className="field">
                            <label>Password</label>
                            <div className="password-wrap">
                                <input
                                    type={showPassword ? "text" : "password"}
                                    placeholder="At least 8 characters"
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
                                <div className="field-error-msg">{errors.password}</div>
                            )}
                        </div>

                        <div className="field">
                            <label>Confirm password</label>
                            <input
                                type="password"
                                value={user.confirmPassword}
                                onChange={updateField("confirmPassword")}
                            />
                            {errors.confirmPassword && (
                                <div className="field-error-msg">{errors.confirmPassword}</div>
                            )}
                        </div>

                        <div className="field">
                            <label>
                                <input
                                    type="checkbox"
                                    checked={user.acceptTerms}
                                    onChange={updateField("acceptTerms")}
                                />{" "}
                                I accept the privacy and terms of use.
                            </label>
                            {errors.acceptTerms && (
                                <div className="field-error-msg">{errors.acceptTerms}</div>
                            )}
                        </div>

                        <button type="submit" className="reg-submit" disabled={submitting}>
                            {submitting ? "Creating account..." : "Create account"}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
}

export default Register;
