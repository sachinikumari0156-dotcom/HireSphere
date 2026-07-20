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
            <Link to="/" className="logo">
                Hire<span>Sphere</span>
            </Link>

            <div className="nav-links">
                <Link to="/">Home</Link>

                {isAuthenticated && user ? (
                    <>
                        <Link to={roleHome(user.role)}>Dashboard</Link>
                        <span className="username">{user.fullName}</span>
                        <button className="logout-btn" onClick={handleLogout}>
                            Logout
                        </button>
                    </>
                ) : (
                    <>
                        <Link to="/login">Login</Link>
                        <Link to="/register" className="nav-cta">
                            Register
                        </Link>
                    </>
                )}
            </div>
        </nav>
    );
}

export default Navbar;
