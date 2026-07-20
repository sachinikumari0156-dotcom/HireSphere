import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import api from "../../api/axios";

const emptyForm = {
    title: "",
    description: "",
    responsibilities: "",
    location: "",
    employmentType: "FullTime",
    workArrangement: "OnSite",
    vacancies: 1,
    salaryMin: "",
    salaryMax: "",
    salaryCurrency: "USD",
    salaryVisible: false,
    minimumExperienceYears: "",
    educationRequirement: "",
    applicationDeadlineUtc: "",
    hiringManagerUserId: "",
    skills: [{ skillName: "", isRequired: true }],
    screeningQuestions: [{ questionText: "", questionType: "Text", isRequired: true, sortOrder: 0 }]
};

export default function RecruiterJobFormPage() {
    const { id } = useParams();
    const isEdit = Boolean(id);
    const navigate = useNavigate();
    const [form, setForm] = useState(emptyForm);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);
    const [loading, setLoading] = useState(isEdit);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        if (!isEdit) return undefined;
        let alive = true;
        (async () => {
            try {
                const response = await api.get(`/recruiter/jobs/${id}`);
                if (!alive) return;
                const job = response.data;
                setForm({
                    title: job.title || "",
                    description: job.description || "",
                    responsibilities: job.responsibilities || "",
                    location: job.location || "",
                    employmentType: job.employmentType || "FullTime",
                    workArrangement: job.workArrangement || "OnSite",
                    vacancies: job.vacancies || 1,
                    salaryMin: job.salaryMin ?? "",
                    salaryMax: job.salaryMax ?? "",
                    salaryCurrency: job.salaryCurrency || "USD",
                    salaryVisible: Boolean(job.salaryVisible),
                    minimumExperienceYears: job.minimumExperienceYears ?? "",
                    educationRequirement: job.educationRequirement || "",
                    applicationDeadlineUtc: job.applicationDeadlineUtc
                        ? job.applicationDeadlineUtc.slice(0, 16)
                        : "",
                    hiringManagerUserId: job.hiringManagerUserId ?? "",
                    skills: (job.skills?.length ? job.skills : [{ skillName: "", isRequired: true }])
                        .map((s) => ({
                            skillName: s.skillName || "",
                            isRequired: s.isRequired !== false,
                            skillId: s.skillId
                        })),
                    screeningQuestions: (job.screeningQuestions?.length
                        ? job.screeningQuestions
                        : [{ questionText: "", questionType: "Text", isRequired: true, sortOrder: 0 }]
                    ).map((q, index) => ({
                        id: q.id,
                        questionText: q.questionText || "",
                        questionType: q.questionType || "Text",
                        isRequired: Boolean(q.isRequired),
                        sortOrder: q.sortOrder ?? index
                    }))
                });
            } catch (err) {
                if (alive) setError(err.response?.data?.message || "Could not load job.");
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [id, isEdit]);

    function updateField(name, value) {
        setForm((prev) => ({ ...prev, [name]: value }));
    }

    async function onSubmit(e) {
        e.preventDefault();
        setError(null);
        setSuccess(null);

        if (!form.title.trim() || !form.description.trim() || !form.location.trim()) {
            setError("Title, description, and location are required.");
            return;
        }

        if (Number(form.vacancies) < 1) {
            setError("Vacancies must be greater than zero.");
            return;
        }

        if (form.salaryMin !== "" && form.salaryMax !== ""
            && Number(form.salaryMin) > Number(form.salaryMax)) {
            setError("Minimum salary cannot exceed maximum salary.");
            return;
        }

        const payload = {
            title: form.title.trim(),
            description: form.description.trim(),
            responsibilities: form.responsibilities.trim() || null,
            location: form.location.trim(),
            employmentType: form.employmentType,
            workArrangement: form.workArrangement,
            vacancies: Number(form.vacancies),
            salaryMin: form.salaryMin === "" ? null : Number(form.salaryMin),
            salaryMax: form.salaryMax === "" ? null : Number(form.salaryMax),
            salaryCurrency: form.salaryCurrency || null,
            salaryVisible: form.salaryVisible,
            minimumExperienceYears: form.minimumExperienceYears === ""
                ? null
                : Number(form.minimumExperienceYears),
            educationRequirement: form.educationRequirement.trim() || null,
            applicationDeadlineUtc: form.applicationDeadlineUtc
                ? new Date(form.applicationDeadlineUtc).toISOString()
                : null,
            hiringManagerUserId: form.hiringManagerUserId === ""
                ? null
                : Number(form.hiringManagerUserId),
            skills: form.skills
                .filter((s) => s.skillName.trim())
                .map((s) => ({
                    skillId: s.skillId || null,
                    skillName: s.skillName.trim(),
                    isRequired: Boolean(s.isRequired)
                })),
            screeningQuestions: form.screeningQuestions
                .filter((q) => q.questionText.trim())
                .map((q, index) => ({
                    id: q.id || null,
                    questionText: q.questionText.trim(),
                    questionType: q.questionType || "Text",
                    isRequired: Boolean(q.isRequired),
                    sortOrder: index
                }))
        };

        setSaving(true);
        try {
            const response = isEdit
                ? await api.put(`/recruiter/jobs/${id}`, payload)
                : await api.post("/recruiter/jobs", payload);
            setSuccess(isEdit ? "Job updated." : "Draft job created.");
            navigate(`/recruiter/jobs/${response.data.id}`);
        } catch (err) {
            setError(err.response?.data?.message || "Could not save job.");
        } finally {
            setSaving(false);
        }
    }

    if (loading) {
        return <main className="rec-page"><p>Loading job form…</p></main>;
    }

    return (
        <main className="rec-page">
            <h2>{isEdit ? "Edit job" : "Create job"}</h2>
            <p className="rec-muted">Organization is taken from your authenticated session.</p>

            {error && <p className="rec-error" role="alert">{error}</p>}
            {success && <p className="rec-success" role="status">{success}</p>}

            <form className="rec-form" onSubmit={onSubmit} noValidate>
                <div className="rec-form-grid">
                    <label>
                        Title
                        <input
                            required
                            value={form.title}
                            onChange={(e) => updateField("title", e.target.value)}
                        />
                    </label>
                    <label>
                        Location
                        <input
                            required
                            value={form.location}
                            onChange={(e) => updateField("location", e.target.value)}
                        />
                    </label>
                    <label>
                        Employment type
                        <select
                            value={form.employmentType}
                            onChange={(e) => updateField("employmentType", e.target.value)}
                        >
                            {["FullTime", "PartTime", "Contract", "Internship", "Temporary"].map((v) => (
                                <option key={v} value={v}>{v}</option>
                            ))}
                        </select>
                    </label>
                    <label>
                        Work arrangement
                        <select
                            value={form.workArrangement}
                            onChange={(e) => updateField("workArrangement", e.target.value)}
                        >
                            {["OnSite", "Remote", "Hybrid"].map((v) => (
                                <option key={v} value={v}>{v}</option>
                            ))}
                        </select>
                    </label>
                    <label>
                        Vacancies
                        <input
                            type="number"
                            min="1"
                            value={form.vacancies}
                            onChange={(e) => updateField("vacancies", e.target.value)}
                        />
                    </label>
                    <label>
                        Hiring manager user id
                        <input
                            type="number"
                            value={form.hiringManagerUserId}
                            onChange={(e) => updateField("hiringManagerUserId", e.target.value)}
                        />
                    </label>
                    <label>
                        Salary min
                        <input
                            type="number"
                            value={form.salaryMin}
                            onChange={(e) => updateField("salaryMin", e.target.value)}
                        />
                    </label>
                    <label>
                        Salary max
                        <input
                            type="number"
                            value={form.salaryMax}
                            onChange={(e) => updateField("salaryMax", e.target.value)}
                        />
                    </label>
                    <label>
                        Currency
                        <input
                            value={form.salaryCurrency}
                            onChange={(e) => updateField("salaryCurrency", e.target.value)}
                        />
                    </label>
                    <label>
                        Min experience (years)
                        <input
                            type="number"
                            value={form.minimumExperienceYears}
                            onChange={(e) => updateField("minimumExperienceYears", e.target.value)}
                        />
                    </label>
                    <label>
                        Application deadline (UTC)
                        <input
                            type="datetime-local"
                            value={form.applicationDeadlineUtc}
                            onChange={(e) => updateField("applicationDeadlineUtc", e.target.value)}
                        />
                    </label>
                </div>

                <label>
                    <span>
                        <input
                            type="checkbox"
                            checked={form.salaryVisible}
                            onChange={(e) => updateField("salaryVisible", e.target.checked)}
                        />
                        {" "}Salary visible to candidates
                    </span>
                </label>

                <label>
                    Description
                    <textarea
                        required
                        rows={5}
                        value={form.description}
                        onChange={(e) => updateField("description", e.target.value)}
                    />
                </label>
                <label>
                    Responsibilities
                    <textarea
                        rows={4}
                        value={form.responsibilities}
                        onChange={(e) => updateField("responsibilities", e.target.value)}
                    />
                </label>
                <label>
                    Education requirement
                    <input
                        value={form.educationRequirement}
                        onChange={(e) => updateField("educationRequirement", e.target.value)}
                    />
                </label>

                <fieldset>
                    <legend>Skills</legend>
                    {form.skills.map((skill, index) => (
                        <div className="rec-skill-row" key={`skill-${index}`}>
                            <label>
                                Skill name
                                <input
                                    value={skill.skillName}
                                    onChange={(e) => {
                                        const skills = [...form.skills];
                                        skills[index] = { ...skill, skillName: e.target.value };
                                        updateField("skills", skills);
                                    }}
                                />
                            </label>
                            <label>
                                Required
                                <input
                                    type="checkbox"
                                    checked={skill.isRequired}
                                    onChange={(e) => {
                                        const skills = [...form.skills];
                                        skills[index] = { ...skill, isRequired: e.target.checked };
                                        updateField("skills", skills);
                                    }}
                                />
                            </label>
                            <button
                                type="button"
                                className="rec-btn secondary"
                                onClick={() => updateField(
                                    "skills",
                                    form.skills.filter((_, i) => i !== index)
                                )}
                            >
                                Remove
                            </button>
                        </div>
                    ))}
                    <button
                        type="button"
                        className="rec-btn secondary"
                        onClick={() => updateField("skills", [...form.skills, { skillName: "", isRequired: false }])}
                    >
                        Add skill
                    </button>
                </fieldset>

                <fieldset>
                    <legend>Screening questions</legend>
                    {form.screeningQuestions.map((question, index) => (
                        <div className="rec-question-row" key={`q-${index}`}>
                            <label>
                                Question
                                <input
                                    value={question.questionText}
                                    onChange={(e) => {
                                        const screeningQuestions = [...form.screeningQuestions];
                                        screeningQuestions[index] = {
                                            ...question,
                                            questionText: e.target.value
                                        };
                                        updateField("screeningQuestions", screeningQuestions);
                                    }}
                                />
                            </label>
                            <label>
                                Required
                                <input
                                    type="checkbox"
                                    checked={question.isRequired}
                                    onChange={(e) => {
                                        const screeningQuestions = [...form.screeningQuestions];
                                        screeningQuestions[index] = {
                                            ...question,
                                            isRequired: e.target.checked
                                        };
                                        updateField("screeningQuestions", screeningQuestions);
                                    }}
                                />
                            </label>
                            <button
                                type="button"
                                className="rec-btn secondary"
                                onClick={() => updateField(
                                    "screeningQuestions",
                                    form.screeningQuestions.filter((_, i) => i !== index)
                                )}
                            >
                                Remove
                            </button>
                        </div>
                    ))}
                    <button
                        type="button"
                        className="rec-btn secondary"
                        onClick={() => updateField("screeningQuestions", [
                            ...form.screeningQuestions,
                            { questionText: "", questionType: "Text", isRequired: false, sortOrder: form.screeningQuestions.length }
                        ])}
                    >
                        Add question
                    </button>
                </fieldset>

                <div className="rec-actions">
                    <button type="submit" className="rec-btn" disabled={saving}>
                        {saving ? "Saving…" : isEdit ? "Save changes" : "Create draft"}
                    </button>
                    <Link className="rec-btn secondary" to="/recruiter/jobs">Cancel</Link>
                </div>
            </form>
        </main>
    );
}
