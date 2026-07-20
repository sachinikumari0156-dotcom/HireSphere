import "./CandidateDashboard.css";
import { useEffect, useState } from "react";

function RecruiterDashboard() {

    const user = JSON.parse(localStorage.getItem("user"));
    const token = localStorage.getItem("token");
    const API_URL = "https://localhost:7000/api";

    const [jobs, setJobs] = useState([]);
    const [applications, setApplications] = useState([]);
    const [posting, setPosting] = useState(false);
    const [banner, setBanner] = useState(null);

    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [skills, setSkills] = useState("");
    const [location, setLocation] = useState("");

    useEffect(function () {

        function loadMyJobs() {
            fetch(API_URL + "/Jobs/MyJobs", {
                headers: { Authorization: "Bearer " + token }
            })
                .then(function (res) {
                    if (res.ok) {
                        return res.json();
                    }
                    return [];
                })
                .then(function (data) {
                    setJobs(data);
                })
                .catch(function (err) {
                    console.log("MyJobs Error:", err);
                });
        }

        function loadApplications() {
            fetch(API_URL + "/Applications/RecruiterApplicationDetails", {
                headers: { Authorization: "Bearer " + token }
            })
                .then(function (res) {
                    if (res.ok) {
                        return res.json();
                    }
                    return [];
                })
                .then(function (data) {
                    setApplications(data);
                })
                .catch(function (err) {
                    console.log("Applications Error:", err);
                });
        }

        loadMyJobs();
        loadApplications();

    }, [token]);

    function postJob(e) {

        e.preventDefault();
        setBanner(null);
        setPosting(true);

        var jobData = {
            title: title,
            description: description,
            requiredSkills: skills,
            location: location
        };

        fetch(API_URL + "/Jobs", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Authorization: "Bearer " + token
            },
            body: JSON.stringify(jobData)
        })
            .then(function (res) {
                if (res.ok) {
                    return res.json();
                }
                throw new Error("Post failed");
            })
            .then(function (created) {

                var updatedJobs = jobs.slice();
                updatedJobs.push(created);
                setJobs(updatedJobs);

                setTitle("");
                setDescription("");
                setSkills("");
                setLocation("");

                setBanner({ type: "success", text: "Job posted successfully." });
                setPosting(false);

            })
            .catch(function (err) {
                console.log("Post Job Error:", err);
                setBanner({ type: "error", text: "Failed to post job." });
                setPosting(false);
            });

    }

    function updateApplicationStatus(applicationId, status) {

        fetch(API_URL + "/Applications/" + applicationId, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                Authorization: "Bearer " + token
            },
            body: JSON.stringify({ status: status })
        })
            .then(function (res) {
                if (res.ok) {

                    var updated = applications.map(function (app) {
                        if (app.id === applicationId) {
                            app.status = status;
                        }
                        return app;
                    });

                    setApplications(updated.slice());

                }
                else {
                    alert("Failed to update status.");
                }
            })
            .catch(function (err) {
                console.log("Update Status Error:", err);
            });

    }

    function deleteJob(jobId) {

        var confirmDelete = window.confirm("Delete this job posting?");

        if (!confirmDelete) {
            return;
        }

        fetch(API_URL + "/Jobs/" + jobId, {
            method: "DELETE",
            headers: { Authorization: "Bearer " + token }
        })
            .then(function (res) {
                if (res.ok) {
                    var remaining = jobs.filter(function (j) {
                        return j.id !== jobId;
                    });
                    setJobs(remaining);
                }
                else {
                    alert("Failed to delete job.");
                }
            })
            .catch(function (err) {
                console.log("Delete Job Error:", err);
            });

    }

    var pendingCount = applications.filter(function (a) {
        return a.status === "Pending";
    }).length;

    var bannerClass = "reg-banner";
    if (banner) {
        bannerClass = "reg-banner " + banner.type;
    }

    return (
        <div className="candidate-page">

            <div className="candidate-header">
                <h1>Welcome, {user ? user.fullName : ""}</h1>
                <p>Post jobs and manage your applicants.</p>
            </div>

            <div className="dashboard-cards">
                <div className="dash-card">
                    <h2>{jobs.length}</h2>
                    <p>Posted Jobs</p>
                </div>
                <div className="dash-card">
                    <h2>{applications.length}</h2>
                    <p>Total Applications</p>
                </div>
                <div className="dash-card">
                    <h2>{pendingCount}</h2>
                    <p>Pending Review</p>
                </div>
            </div>

            <div className="jobs-section">
                <h2>Post a New Job</h2>

                {banner && (
                    <div className={bannerClass}>
                        {banner.text}
                    </div>
                )}

                <form onSubmit={postJob} noValidate>

                    <div className="field">
                        <label>Job Title</label>
                        <input
                            placeholder="Frontend Developer"
                            value={title}
                            onChange={function (e) { setTitle(e.target.value); }}
                            required
                        />
                    </div>

                    <div className="field">
                        <label>Description</label>
                        <input
                            placeholder="React developer needed"
                            value={description}
                            onChange={function (e) { setDescription(e.target.value); }}
                            required
                        />
                    </div>

                    <div className="field">
                        <label>Required Skills</label>
                        <input
                            placeholder="React, JavaScript"
                            value={skills}
                            onChange={function (e) { setSkills(e.target.value); }}
                            required
                        />
                    </div>

                    <div className="field">
                        <label>Location</label>
                        <input
                            placeholder="Remote or Colombo"
                            value={location}
                            onChange={function (e) { setLocation(e.target.value); }}
                            required
                        />
                    </div>

                    <button type="submit" className="reg-submit" disabled={posting}>
                        {posting ? "Posting..." : "Post Job"}
                    </button>

                </form>
            </div>

            <div className="jobs-section">
                <h2>Your Job Listings</h2>

                {jobs.length === 0 && <p>No jobs posted yet.</p>}

                {jobs.map(function (job) {
                    return (
                        <div className="job-box" key={job.id}>
                            <h3>{job.title}</h3>
                            <p>{job.description}</p>
                            <p>Skills: {job.requiredSkills}</p>
                            <p>Location: {job.location}</p>
                            <button
                                onClick={function () { deleteJob(job.id); }}
                                style={{ background: "#c0392b" }}
                            >
                                Delete Job
                            </button>
                        </div>
                    );
                })}
            </div>

            <div className="jobs-section">
                <h2>Applications Received</h2>

                {applications.length === 0 && <p>No applications yet.</p>}

                {applications.map(function (app) {
                    return (
                        <div className="job-box" key={app.id}>
                            <h3>{app.candidateName ? app.candidateName : "Candidate"}</h3>
                            <p>Applied for: {app.jobTitle}</p>
                            <p>Phone: {app.phoneNumber ? app.phoneNumber : "N/A"}</p>
                            <p>Skills: {app.skills ? app.skills : "N/A"}</p>
                            <p>Cover Letter: {app.coverLetter}</p>
                            <p>Status: <strong>{app.status}</strong></p>

                            {app.resumePath && (
                                <p>
                                    <a href={app.resumePath} target="_blank" rel="noreferrer">
                                        View Resume
                                    </a>
                                </p>
                            )}

                            {app.status === "Pending" && (
                                <div style={{ display: "flex", gap: "10px" }}>
                                    <button
                                        onClick={function () { updateApplicationStatus(app.id, "Accepted"); }}
                                    >
                                        Accept
                                    </button>
                                    <button
                                        onClick={function () { updateApplicationStatus(app.id, "Rejected"); }}
                                        style={{ background: "#c0392b" }}
                                    >
                                        Reject
                                    </button>
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>

        </div>
    );

}

export default RecruiterDashboard;