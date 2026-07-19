import { Link } from "react-router-dom";
import "./Navbar.css";

function Navbar() {
    return (
        <nav className="navbar">
            <h2 className="logo">
                Hire<span>Sphere</span>
            </h2>
            <div className="nav-links">
                <Link to="/">Home</Link>
                <Link to="/login">Login</Link>
                <Link to="/register" className="nav-cta">
                    Register
                </Link>
            </div>
        </nav>
    );
}

export default Navbar;