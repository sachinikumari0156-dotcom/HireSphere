import { useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/axios";
import "./Register.css";

export default function RecruiterRequest() {
    const [form, setForm] = useState({
        fullName: "",
        businessEmail: "",
        organizationName: "",
        message: ""
    });
    const [errors, setErrors] = useState({});
    const [banner, setBanner] = useState(null);
    const [submitting, setSubmitting] = useState(false);

    const updateField = (field) => (e) => {
        setForm((prev) => ({ ...prev, [field]: e.target.value }));
        if (errors[field]) {
            setErrors((prev) => ({ ...prev, [field]: null }));
        }
    };

    const validate = () => {
        const next = {};
        if (!form.fullName.trim()) next.fullName = "Enter your full name.";
        if (!form.businessEmail.trim()) next.businessEmail = "Enter a business email.";
        else if (!/^\S+@\S+\.\S+$/.test(form.businessEmail)) {
            next.businessEmail = "Enter a valid email address.";
        }
        if (!form.organizationName.trim()) {
            next.organizationName = "Enter your organization name.";
        }
        setErrors(next);
        return Object.keys(next).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setBanner(null);
        if (!validate()) return;

        setSubmitting(true);
        try {
            await api.post("/auth/recruiter-requests", form);
            setBanner({
                type: "success",
                text: "Request submitted. An administrator will review it."
            });
            setForm({
                fullName: "",
                businessEmail: "",
                organizationName: "",
                message: ""
            });
        } catch (error) {
            setBanner({
                type: "error",
                text: error.response?.data?.message || "Request failed. Please try again."
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
                    <span className="reg-eyebrow">For recruiters</span>
                    <h1 className="reg-headline">Request recruiter access.</h1>
                    <p className="reg-subcopy">
                        Recruiter accounts are not self-registered. Submit a request for admin review.
                    </p>
                </div>
            </div>

            <div className="reg-form-side">
                <div className="reg-card">
                    <h1>Recruiter access request</h1>
                    <p className="reg-lead">
                        Looking for a candidate account?{" "}
                        <Link to="/register">Register here</Link>
                    </p>

                    {banner && (
                        <div className={`reg-banner ${banner.type}`}>
                            {banner.text}
                        </div>
                    )}

                    <form onSubmit={handleSubmit} noValidate>
                        <div className="field">
                            <label>Full name</label>
                            <input value={form.fullName} onChange={updateField("fullName")} />
                            {errors.fullName && <div className="field-error-msg">{errors.fullName}</div>}
                        </div>

                        <div className="field">
                            <label>Business email</label>
                            <input
                                type="email"
                                value={form.businessEmail}
                                onChange={updateField("businessEmail")}
                            />
                            {errors.businessEmail && (
                                <div className="field-error-msg">{errors.businessEmail}</div>
                            )}
                        </div>

                        <div className="field">
                            <label>Organization</label>
                            <input
                                value={form.organizationName}
                                onChange={updateField("organizationName")}
                            />
                            {errors.organizationName && (
                                <div className="field-error-msg">{errors.organizationName}</div>
                            )}
                        </div>

                        <div className="field">
                            <label>Message (optional)</label>
                            <textarea
                                rows={3}
                                value={form.message}
                                onChange={updateField("message")}
                            />
                        </div>

                        <button type="submit" className="reg-submit" disabled={submitting}>
                            {submitting ? "Submitting..." : "Submit request"}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
}
