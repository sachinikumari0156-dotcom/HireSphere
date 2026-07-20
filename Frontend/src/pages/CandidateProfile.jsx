import "./CandidateDashboard.css";
import { useEffect, useState } from "react";

function CandidateProfile() {

    const token = localStorage.getItem("token");
    const API_URL = "https://localhost:7000/api";

    const [fullName, setFullName] = useState("");
    const [phoneNumber, setPhoneNumber] = useState("");
    const [skills, setSkills] = useState("");
    const [resumePath, setResumePath] = useState("");

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [banner, setBanner] = useState(null);

    useEffect(function () {

        function loadProfile() {

            fetch(API_URL + "/CandidateProfile/Me", {
                headers: { Authorization: "Bearer " + token }
            })
                .then(function (res) {
                    if (res.ok) {
                        return res.json();
                    }
                    throw new Error("Failed to load profile");
                })
                .then(function (data) {
                    setFullName(data.fullName || "");
                    setPhoneNumber(data.phoneNumber || "");
                    setSkills(data.skills || "");
                    setResumePath(data.resumePath || "");
                    setLoading(false);
                })
                .catch(function (err) {
                    console.log("Profile Load Error:", err);
                    setLoading(false);
                });

        }

        loadProfile();

    }, [token]);

    function saveProfile(e) {

        e.preventDefault();
        setBanner(null);
        setSaving(true);

        var payload = {
            fullName: fullName,
            phoneNumber: phoneNumber,
            skills: skills,
            resumePath: resumePath
        };

        fetch(API_URL + "/CandidateProfile/Me", {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                Authorization: "Bearer " + token
            },
            body: JSON.stringify(payload)
        })
            .then(function (res) {
                if (res.ok) {
                    setBanner({ type: "success", text: "Profile updated successfully." });
                }
                else {
                    setBanner({ type: "error", text: "Failed to update profile." });
                }
                setSaving(false);
            })
            .catch(function (err) {
                console.log("Profile Save Error:", err);
                setBanner({ type: "error", text: "Something went wrong." });
                setSaving(false);
            });

    }

    if (loading) {
        return (
            <div className="candidate-page">
                <p>Loading profile...</p>
            </div>
        );
    }

    return (
        <div className="candidate-page">

            <div className="candidate-header">
                <h1>My Profile</h1>
                <p>Keep your details up to date so recruiters can reach you.</p>
            </div>

            <div className="jobs-section">

                {banner && (
                    <div className={"reg-banner " + banner.type}>
                        {banner.text}
                    </div>
                )}

                <form onSubmit={saveProfile} noValidate>

                    <div className="field">
                        <label>Full Name</label>
                        <input
                            placeholder="Jane Perera"
                            value={fullName}
                            onChange={function (e) { setFullName(e.target.value); }}
                        />
                    </div>

                    <div className="field">
                        <label>Phone Number</label>
                        <input
                            placeholder="07XXXXXXXX"
                            value={phoneNumber}
                            onChange={function (e) { setPhoneNumber(e.target.value); }}
                        />
                    </div>

                    <div className="field">
                        <label>Skills</label>
                        <input
                            placeholder="React, JavaScript, SQL"
                            value={skills}
                            onChange={function (e) { setSkills(e.target.value); }}
                        />
                    </div>

                    <div className="field">
                        <label>Resume Link</label>
                        <input
                            placeholder="https://drive.google.com/your-resume"
                            value={resumePath}
                            onChange={function (e) { setResumePath(e.target.value); }}
                        />
                    </div>

                    <button type="submit" className="reg-submit" disabled={saving}>
                        {saving ? "Saving..." : "Save Profile"}
                    </button>

                </form>

            </div>

        </div>
    );

}

export default CandidateProfile;