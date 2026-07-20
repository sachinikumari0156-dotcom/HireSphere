import { Link } from "react-router-dom";
import "./Home.css";

function Home() {

    return (
        <div className="home-page">

            <section className="home-hero">

                <div className="home-hero-content">

                    <span className="home-eyebrow">
                        AI-Powered Recruitment Platform
                    </span>

                    <h1 className="home-headline">
                        Find the right job.
                        <br />
                        Find the right talent.
                    </h1>

                    <p className="home-subcopy">
                        HireSphere connects candidates and recruiters through
                        a smarter, faster hiring process. Post jobs, apply in
                        seconds, and track every step in one place.
                    </p>

                    <div className="home-cta-group">

                        <Link to="/register" className="home-cta-primary">
                            Get Started
                        </Link>

                        <Link to="/login" className="home-cta-secondary">
                            Sign In
                        </Link>

                    </div>

                </div>

            </section>


            <section className="home-features">

                <h2 className="home-section-title">
                    Why choose HireSphere
                </h2>

                <div className="home-features-grid">

                    <div className="home-feature-card">
                        <div className="home-feature-icon">1</div>
                        <h3>Smart Job Matching</h3>
                        <p>
                            Discover roles that match your skills and
                            experience, without endless scrolling.
                        </p>
                    </div>

                    <div className="home-feature-card">
                        <div className="home-feature-icon">2</div>
                        <h3>One-Click Apply</h3>
                        <p>
                            Build your profile once and apply to jobs
                            instantly, right from your dashboard.
                        </p>
                    </div>

                    <div className="home-feature-card">
                        <div className="home-feature-icon">3</div>
                        <h3>Recruiter Tools</h3>
                        <p>
                            Post listings, review applicants, and manage
                            your hiring pipeline in one clean workspace.
                        </p>
                    </div>

                    <div className="home-feature-card">
                        <div className="home-feature-icon">4</div>
                        <h3>Real-Time Tracking</h3>
                        <p>
                            Know exactly where every application stands,
                            from submitted to accepted.
                        </p>
                    </div>

                </div>

            </section>


            <section className="home-cta-banner">

                <h2>Ready to get started?</h2>

                <p>
                    Join HireSphere today as a candidate or a recruiter.
                </p>

                <Link to="/register" className="home-cta-primary">
                    Create Your Account
                </Link>

            </section>


            <footer className="home-footer">
                <p>Copyright HireSphere. All rights reserved.</p>
            </footer>

        </div>
    );

}

export default Home;