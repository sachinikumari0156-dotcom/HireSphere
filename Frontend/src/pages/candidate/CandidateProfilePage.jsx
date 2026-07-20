import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";
import "../CandidateDashboard.css";

const emptyProfile = {
    fullName: "",
    phoneNumber: "",
    address: "",
    summary: "",
    location: "",
    yearsOfExperience: "",
    desiredJobTitle: "",
    preferredWorkArrangement: "",
    salaryExpectation: "",
    availability: "",
    portfolioUrl: "",
    linkedInUrl: "",
    gitHubUrl: ""
};

export default function CandidateProfilePage() {
    const [profile, setProfile] = useState(emptyProfile);
    const [detail, setDetail] = useState(null);
    const [experience, setExperience] = useState({
        companyName: "", jobTitle: "", startDate: "", endDate: "", description: "", location: "", isCurrentRole: false
    });
    const [education, setEducation] = useState({
        institution: "", degree: "", fieldOfStudy: "", startDate: "", endDate: "", grade: "", isCurrentStudy: false
    });
    const [skill, setSkill] = useState({ skillId: "", proficiencyLevel: "Intermediate", yearsOfExperience: 1 });
    const [skillsCatalog, setSkillsCatalog] = useState([]);
    const [cert, setCert] = useState({
        name: "", issuingOrganization: "", issueDate: "", expiryDate: "", credentialId: "", credentialUrl: ""
    });
    const [message, setMessage] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);
    const [resumes, setResumes] = useState([]);

    useEffect(() => {
        let alive = true;
        (async () => {
            setLoading(true);
            setError(null);
            try {
                const [profileRes, skillsRes, resumesRes] = await Promise.all([
                    api.get("/candidate/profile"),
                    api.get("/candidate/skills/catalog").catch(() => ({ data: [] })),
                    api.get("/candidate/resumes").catch(() => ({ data: [] }))
                ]);
                if (!alive) return;
                const data = profileRes.data;
                setDetail(data);
                setProfile({
                    fullName: data.fullName || "",
                    phoneNumber: data.phoneNumber || "",
                    address: data.address || "",
                    summary: data.summary || "",
                    location: data.location || "",
                    yearsOfExperience: data.yearsOfExperience ?? "",
                    desiredJobTitle: data.desiredJobTitle || "",
                    preferredWorkArrangement: data.preferredWorkArrangement ?? "",
                    salaryExpectation: data.salaryExpectation ?? "",
                    availability: data.availability || "",
                    portfolioUrl: data.portfolioUrl || "",
                    linkedInUrl: data.linkedInUrl || "",
                    gitHubUrl: data.gitHubUrl || ""
                });
                setSkillsCatalog(Array.isArray(skillsRes.data) ? skillsRes.data : []);
                setResumes(Array.isArray(resumesRes.data) ? resumesRes.data : []);
            } catch (err) {
                if (alive) {
                    setError(err.response?.data?.message || "Could not load profile.");
                }
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    const reloadProfile = async () => {
        setError(null);
        try {
            const [profileRes, skillsRes, resumesRes] = await Promise.all([
                api.get("/candidate/profile"),
                api.get("/candidate/skills/catalog").catch(() => ({ data: [] })),
                api.get("/candidate/resumes").catch(() => ({ data: [] }))
            ]);
            const data = profileRes.data;
            setDetail(data);
            setSkillsCatalog(Array.isArray(skillsRes.data) ? skillsRes.data : []);
            setResumes(Array.isArray(resumesRes.data) ? resumesRes.data : []);
        } catch (err) {
            setError(err.response?.data?.message || "Could not reload profile.");
        }
    };
    const saveProfile = async (e) => {
        e.preventDefault();
        setMessage(null);
        setError(null);
        try {
            const payload = {
                ...profile,
                yearsOfExperience: profile.yearsOfExperience === "" ? null : Number(profile.yearsOfExperience),
                salaryExpectation: profile.salaryExpectation === "" ? null : Number(profile.salaryExpectation),
                preferredWorkArrangement: profile.preferredWorkArrangement === ""
                    ? null
                    : Number(profile.preferredWorkArrangement)
            };
            const response = await api.put("/candidate/profile", payload);
            setDetail(response.data);
            setMessage("Profile saved.");
        } catch (err) {
            setError(err.response?.data?.message || "Save failed.");
        }
    };

    const addExperience = async (e) => {
        e.preventDefault();
        try {
            await api.post("/candidate/experience", {
                ...experience,
                endDate: experience.isCurrentRole || !experience.endDate ? null : experience.endDate
            });
            setMessage("Experience added.");
            await reloadProfile();
        } catch (err) {
            setError(err.response?.data?.message || "Could not add experience.");
        }
    };

    const addEducation = async (e) => {
        e.preventDefault();
        try {
            await api.post("/candidate/education", {
                ...education,
                endDate: education.isCurrentStudy || !education.endDate ? null : education.endDate
            });
            setMessage("Education added.");
            await reloadProfile();
        } catch (err) {
            setError(err.response?.data?.message || "Could not add education.");
        }
    };

    const addSkill = async (e) => {
        e.preventDefault();
        try {
            await api.post("/candidate/skills", {
                skillId: Number(skill.skillId),
                proficiencyLevel: skill.proficiencyLevel,
                yearsOfExperience: Number(skill.yearsOfExperience)
            });
            setMessage("Skill added.");
            await reloadProfile();
        } catch (err) {
            setError(err.response?.data?.message || "Could not add skill.");
        }
    };

    const addCertification = async (e) => {
        e.preventDefault();
        try {
            await api.post("/candidate/certifications", {
                ...cert,
                expiryDate: cert.expiryDate || null
            });
            setMessage("Certification added.");
            await reloadProfile();
        } catch (err) {
            setError(err.response?.data?.message || "Could not add certification.");
        }
    };

    const uploadResume = async (e) => {
        const file = e.target.files?.[0];
        if (!file) return;
        const form = new FormData();
        form.append("file", file);
        try {
            await api.post("/candidate/resumes", form, {
                headers: { "Content-Type": "multipart/form-data" }
            });
            setMessage("Resume uploaded.");
            await reloadProfile();
        } catch (err) {
            setError(err.response?.data?.message || "Resume upload failed.");
        } finally {
            e.target.value = "";
        }
    };

    if (loading) {
        return <main className="dash-page"><p>Loading profile…</p></main>;
    }

    return (
        <main className="dash-page">
            <header className="dash-header">
                <h1>Profile &amp; documents</h1>
                <p>Completion: {detail?.profileCompletionPercent ?? 0}%</p>
                <Link to="/candidate">Back to dashboard</Link>
            </header>

            {message && <p className="success">{message}</p>}
            {error && <p className="error">{error}</p>}

            <section>
                <h2>Personal &amp; professional</h2>
                <form onSubmit={saveProfile} className="profile-form">
                    {Object.entries({
                        fullName: "Full name",
                        phoneNumber: "Phone",
                        address: "Address",
                        summary: "Summary",
                        location: "Location",
                        yearsOfExperience: "Years of experience",
                        desiredJobTitle: "Desired job title",
                        salaryExpectation: "Salary expectation",
                        availability: "Availability",
                        portfolioUrl: "Portfolio URL",
                        linkedInUrl: "LinkedIn URL",
                        gitHubUrl: "GitHub URL"
                    }).map(([key, label]) => (
                        <label key={key}>
                            {label}
                            <input
                                value={profile[key]}
                                onChange={(e) => setProfile((p) => ({ ...p, [key]: e.target.value }))}
                            />
                        </label>
                    ))}
                    <label>
                        Preferred work arrangement
                        <select
                            value={profile.preferredWorkArrangement}
                            onChange={(e) => setProfile((p) => ({ ...p, preferredWorkArrangement: e.target.value }))}
                        >
                            <option value="">Not set</option>
                            <option value="0">OnSite</option>
                            <option value="1">Remote</option>
                            <option value="2">Hybrid</option>
                        </select>
                    </label>
                    <button type="submit">Save profile</button>
                </form>
            </section>

            <section>
                <h2>Work experience</h2>
                <ul>
                    {(detail?.workExperiences || []).map((item) => (
                        <li key={item.id}>
                            {item.jobTitle} @ {item.companyName}
                            {item.isCurrentRole ? " (current)" : ""}
                        </li>
                    ))}
                    {(detail?.workExperiences || []).length === 0 && <li className="empty-state">No experience yet.</li>}
                </ul>
                <form onSubmit={addExperience}>
                    <input placeholder="Company" aria-label="Company" value={experience.companyName} onChange={(e) => setExperience({ ...experience, companyName: e.target.value })} required />
                    <input placeholder="Title" aria-label="Job title" value={experience.jobTitle} onChange={(e) => setExperience({ ...experience, jobTitle: e.target.value })} required />
                    <input type="date" aria-label="Experience start date" value={experience.startDate} onChange={(e) => setExperience({ ...experience, startDate: e.target.value })} required />
                    <input type="date" aria-label="Experience end date" value={experience.endDate} onChange={(e) => setExperience({ ...experience, endDate: e.target.value })} disabled={experience.isCurrentRole} />
                    <label>
                        <input type="checkbox" checked={experience.isCurrentRole} onChange={(e) => setExperience({ ...experience, isCurrentRole: e.target.checked })} />
                        Current role
                    </label>
                    <button type="submit">Add experience</button>
                </form>
            </section>

            <section>
                <h2>Education</h2>
                <ul>
                    {(detail?.educations || []).map((item) => (
                        <li key={item.id}>
                            {item.degree} — {item.institution}
                            {item.isCurrentStudy ? " (current)" : ""}
                        </li>
                    ))}
                    {(detail?.educations || []).length === 0 && <li className="empty-state">No education yet.</li>}
                </ul>
                <form onSubmit={addEducation}>
                    <input placeholder="Institution" value={education.institution} onChange={(e) => setEducation({ ...education, institution: e.target.value })} required />
                    <input placeholder="Qualification" value={education.degree} onChange={(e) => setEducation({ ...education, degree: e.target.value })} required />
                    <input placeholder="Field of study" value={education.fieldOfStudy} onChange={(e) => setEducation({ ...education, fieldOfStudy: e.target.value })} />
                    <input type="date" aria-label="Education start date" value={education.startDate} onChange={(e) => setEducation({ ...education, startDate: e.target.value })} />
                    <input type="date" aria-label="Education end date" value={education.endDate} onChange={(e) => setEducation({ ...education, endDate: e.target.value })} disabled={education.isCurrentStudy} />
                    <label>
                        <input type="checkbox" checked={education.isCurrentStudy} onChange={(e) => setEducation({ ...education, isCurrentStudy: e.target.checked })} />
                        Currently studying
                    </label>
                    <button type="submit">Add education</button>
                </form>
            </section>

            <section>
                <h2>Skills</h2>
                <ul>
                    {(detail?.skills || []).map((item) => (
                        <li key={item.id}>{item.skillName} — {item.proficiencyLevel}</li>
                    ))}
                    {(detail?.skills || []).length === 0 && <li className="empty-state">No skills yet.</li>}
                </ul>
                <form onSubmit={addSkill}>
                    <label>
                        Skill
                        <select value={skill.skillId} onChange={(e) => setSkill({ ...skill, skillId: e.target.value })} required>
                            <option value="">Select skill</option>
                            {skillsCatalog.map((s) => (
                                <option key={s.id} value={s.id}>{s.name}</option>
                            ))}
                        </select>
                    </label>
                    <button type="submit">Add skill</button>
                </form>
            </section>

            <section>
                <h2>Certifications</h2>
                <ul>
                    {(detail?.certifications || []).map((item) => (
                        <li key={item.id}>{item.name} — {item.issuingOrganization}</li>
                    ))}
                    {(detail?.certifications || []).length === 0 && <li className="empty-state">No certifications yet.</li>}
                </ul>
                <form onSubmit={addCertification}>
                    <input placeholder="Name" aria-label="Certification name" value={cert.name} onChange={(e) => setCert({ ...cert, name: e.target.value })} required />
                    <input placeholder="Issuer" aria-label="Certification issuer" value={cert.issuingOrganization} onChange={(e) => setCert({ ...cert, issuingOrganization: e.target.value })} required />
                    <input type="date" aria-label="Certification issue date" value={cert.issueDate} onChange={(e) => setCert({ ...cert, issueDate: e.target.value })} required />
                    <button type="submit">Add certification</button>
                </form>
            </section>

            <section>
                <h2>Resume upload</h2>
                <p>PDF, DOC, DOCX, PNG, or JPEG. Max 5 MB. Cloud storage verification pending — local storage used.</p>
                <ul>
                    {resumes.map((item) => (
                        <li key={item.id}>
                            {item.fileName}
                            {item.isPrimary ? " (primary)" : ""}
                            {item.storageKey ? ` · key: ${item.storageKey}` : ""}
                        </li>
                    ))}
                    {resumes.length === 0 && <li className="empty-state">No resumes uploaded yet.</li>}
                </ul>
                <label>
                    Upload resume
                    <input type="file" accept=".pdf,.doc,.docx,.png,.jpeg,.jpg" onChange={uploadResume} />
                </label>
            </section>
        </main>
    );
}
