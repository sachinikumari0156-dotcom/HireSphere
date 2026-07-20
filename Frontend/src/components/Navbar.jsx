import { Link, useNavigate, useLocation } from "react-router-dom";
import { useState, useEffect } from "react";
import "./Navbar.css";

function Navbar() {

    const navigate = useNavigate();
    const location = useLocation();

    const [user, setUser] = useState(null);

    useEffect(function () {

        function loadUser() {
            var stored = localStorage.getItem("user");

            if (stored) {
                setUser(JSON.parse(stored));
            }
            else {
                setUser(null);
            }
        }

        loadUser();

    }, [location]);

    function handleLogout() {

        localStorage.removeItem("token");
        localStorage.removeItem("user");

        setUser(null);

        navigate("/login");

    }

    var dashboardPath = "/dashboard";

    if (user && user.role === "Candidate") {
        dashboardPath = "/candidate-dashboard";
    }
    else if (user && user.role === "Recruiter") {
        dashboardPath = "/recruiter-dashboard";
    }

    return (
        <nav className="navbar">

            <div className="navbar-inner">

                <Link to="/" className="navbar-brand">
                    hire<span>flow</span>
                </Link>

                <div className="navbar-links">

                    {user ? (
                        <>
                            <Link to={dashboardPath} className="navbar-link">
                                Dashboard
                            </Link>

                            {user.role === "Candidate" && (
                                <Link to="/candidate-profile" className="navbar-link">
                                    My Profile
                                </Link>
                            )}

                            <span className="navbar-user">
                                {user.fullName}
                            </span>

                            <button
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