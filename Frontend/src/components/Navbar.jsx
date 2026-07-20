import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth";
import "./Navbar.css";

function Navbar() {
    const navigate = useNavigate();
    const { user, isAuthenticated, logout, roleHome } = useAuth();

    const handleLogout = async () => {
        await logout();
        navigate("/login");
    };

    return (
        <nav className="navbar">
            <div className="navbar-inner">
                <Link to="/" className="navbar-brand">
                    Hire<span>Sphere</span>
                </Link>

                <div className="navbar-links">
                    <Link to="/" className="navbar-link">
                        Home
                    </Link>

                    {isAuthenticated && user ? (
                        <>
                            <Link
                                to={roleHome(user.role)}
                                className="navbar-link"
                            >
                                Dashboard
                            </Link>

                            {user.role === "Candidate" && (
                                <Link
                                    to="/candidate/profile"
                                    className="navbar-link"
                                >
                                    My Profile
                                </Link>
                            )}

                            <span className="navbar-user">
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
                            <Link to="/login" className="navbar-link">
                                Sign In
                            </Link>

                            <Link to="/register" className="navbar-cta">
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
