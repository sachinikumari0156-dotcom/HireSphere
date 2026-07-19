import { Link, useNavigate } from "react-router-dom";
import "./Navbar.css";
import { useState } from "react";

function Navbar() {

    const navigate = useNavigate();

    const [token, setToken] = useState(
        localStorage.getItem("token")
    );

    const user = JSON.parse(
        localStorage.getItem("user")
    );


    const logout = () => {

        localStorage.removeItem("token");
        localStorage.removeItem("user");

        setToken(null);

        navigate("/login");

    };


    return (

        <nav className="navbar">


            <h2 className="logo">

                Hire<span>Sphere</span>

            </h2>



            <div className="nav-links">


                <Link to="/">
                    Home
                </Link>



                {
                    token ? (

                        <>

                            <Link
                                to={
                                    user?.role === "Candidate"
                                        ? "/candidate-dashboard"
                                        : "/recruiter-dashboard"
                                }
                            >

                                Dashboard

                            </Link>


                            <span className="username">

                                {user?.fullName}

                            </span>


                            <button
                                className="logout-btn"
                                onClick={logout}
                            >

                                Logout

                            </button>


                        </>


                    ) : (

                        <>

                            <Link to="/login">
                                Login
                            </Link>


                            <Link
                                to="/register"
                                className="nav-cta"
                            >

                                Register

                            </Link>

                        </>

                    )

                }


            </div>


        </nav>

    );

}


export default Navbar;