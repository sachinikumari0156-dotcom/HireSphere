import { Link, useNavigate } from "react-router-dom";
import "./Navbar.css";


function Navbar() {

    const navigate = useNavigate();


    const token = localStorage.getItem("token");


    const userData = localStorage.getItem("user");

    const user = userData 
        ? JSON.parse(userData)
        : null;



    const logout = () => {

        localStorage.removeItem("token");
        localStorage.removeItem("user");

        navigate("/login");

        window.location.reload();

    };



    const dashboardPath = () => {

        if (user?.role === "Candidate") {

            return "/candidate-dashboard";

        }


        if (user?.role === "Recruiter") {

            return "/recruiter-dashboard";

        }


        return "/";

    };



    return (

        <nav className="navbar">


            <Link to="/" className="logo">

                Hire<span>Sphere</span>

            </Link>



            <div className="nav-links">


                <Link to="/">

                    Home

                </Link>



                {
                    token && user ? (

                        <>


                            <Link to={dashboardPath()}>

                                Dashboard

                            </Link>



                            <span className="username">

                                {user.fullName}

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