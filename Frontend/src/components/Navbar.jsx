import { useEffect, useId, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth";
import "./Navbar.css";

function Navbar() {
    const navigate = useNavigate();
    const { user, isAuthenticated, logout, roleHome } = useAuth();
    const [menuOpen, setMenuOpen] = useState(false);
    const menuId = useId();

    useEffect(() => {
        function onKey(e) {
            if (e.key === "Escape") setMenuOpen(false);
        }
        document.addEventListener("keydown", onKey);
        return () => document.removeEventListener("keydown", onKey);
    }, []);

    const handleLogout = async () => {
        setMenuOpen(false);
        await logout();
        navigate("/login");
    };

    return (
        <nav className="navbar" aria-label="Primary">
            <div className="navbar-inner">
                <Link to="/" className="navbar-brand" onClick={() => setMenuOpen(false)}>
                    Hire<span>Sphere</span>
                </Link>

                <button
                    type="button"
                    className="navbar-menu-toggle hs-btn hs-btn--secondary"
                    aria-expanded={menuOpen}
                    aria-controls={menuId}
                    aria-label={menuOpen ? "Close menu" : "Open menu"}
                    onClick={() => setMenuOpen((v) => !v)}
                >
                    Menu
                </button>

                <div id={menuId} className={`navbar-links ${menuOpen ? "is-open" : ""}`}>
                    <Link to="/" className="navbar-link" onClick={() => setMenuOpen(false)}>
                        Home
                    </Link>

                    {isAuthenticated && user ? (
                        <>
                            <Link
                                to={roleHome(user.role)}
                                className="navbar-link"
                                onClick={() => setMenuOpen(false)}
                            >
                                Dashboard
                            </Link>

                            {user.role === "Candidate" && (
                                <>
                                    <Link
                                        to="/candidate/profile"
                                        className="navbar-link"
                                        onClick={() => setMenuOpen(false)}
                                    >
                                        My Profile
                                    </Link>
                                    <Link
                                        to="/notification-preferences"
                                        className="navbar-link"
                                        onClick={() => setMenuOpen(false)}
                                    >
                                        Preferences
                                    </Link>
                                </>
                            )}

                            {(user.role === "Recruiter" ||
                                user.role === "HiringManager" ||
                                user.role === "Admin") && (
                                <Link
                                    to="/notification-preferences"
                                    className="navbar-link"
                                    onClick={() => setMenuOpen(false)}
                                >
                                    Preferences
                                </Link>
                            )}

                            <span className="navbar-user" aria-label={`Signed in as ${user.fullName}`}>
                                {user.fullName}
                            </span>

                            <button
                                type="button"
                                className="navbar-logout"
                                onClick={handleLogout}
                            >
                                Logout
                            </button>
                        </>
                    ) : (
                        <>
                            <Link to="/login" className="navbar-link" onClick={() => setMenuOpen(false)}>
                                Sign In
                            </Link>

                            <Link to="/register" className="navbar-cta" onClick={() => setMenuOpen(false)}>
                                Get Started
                            </Link>
                        </>
                    )}
                </div>
            </div>
        </nav>
    );
}

export default Navbar;
