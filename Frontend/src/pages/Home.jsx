import "./Home.css";

function Home() {
    return (
        <div className="hero">
            <div className="hero-text">
                <span className="hero-eyebrow">Hiring made simple</span>
                <h1>
                    Find your dream job
                    <br />
                    with <span>HireSphere</span>
                </h1>
                <p>
                    A smart recruitment platform connecting talented candidates
                    with top companies. Build a profile once, and let the right
                    opportunities find you.
                </p>
                <div className="hero-actions">
                    <button>Explore jobs</button>
                    <a href="/register" className="hero-secondary-link">
                        Post a job instead
                    </a>
                </div>
                <div className="hero-stats">
                    <div>
                        <strong>12k+</strong>
                        <span>Open roles</span>
                    </div>
                    <div>
                        <strong>3.4k</strong>
                        <span>Companies hiring</span>
                    </div>
                    <div>
                        <strong>98%</strong>
                        <span>Match satisfaction</span>
                    </div>
                </div>
            </div>

            <div className="hero-card-wrap">
                <div className="hero-card">
                    <div className="hero-card-icon">💼</div>
                    <h3>Thousands of opportunities</h3>
                    <p>Connect. Apply. Grow.</p>
                    <div className="hero-card-progress">
                        <span></span>
                    </div>

                    <div className="job-chip c1">
                        Frontend Developer
                        <span>Colombo · Remote</span>
                    </div>
                    <div className="job-chip c2">
                        Product Designer
                        <span>Negombo · Full-time</span>
                    </div>
                    <div className="job-chip c3">
                        Data Analyst
                        <span>Kandy · Hybrid</span>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default Home;