import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/axios";
import "./Login.css";

function Login() {

    const navigate = useNavigate();

    const [user, setUser] = useState({
        email: "",
        password: "",
    });

    const [showPassword, setShowPassword] = useState(false);
    const [errors, setErrors] = useState({});
    const [banner, setBanner] = useState(null);
    const [submitting, setSubmitting] = useState(false);


    const updateField = (field) => (e) => {

        setUser({
            ...user,
            [field]: e.target.value
        });

        if (errors[field]) {

            setErrors({
                ...errors,
                [field]: null
            });
        }
    };


    const validate = () => {

        const next = {};

        if (!user.email.trim()) {
            next.email = "Enter your email.";
        }

        if (!user.password) {
            next.password = "Enter your password.";
        }


        setErrors(next);

        return Object.keys(next).length === 0;
    };



    const handleLogin = async (e) => {

        e.preventDefault();

        setBanner(null);


        if (!validate()) return;


        setSubmitting(true);


        try {


            const response = await api.post(
                "/Auth/Login",
                user
            );


            console.log(
                "LOGIN RESPONSE:",
                response.data
            );


            // Save JWT token
            localStorage.setItem(
                "token",
                response.data.token
            );


            // Save user details
            localStorage.setItem(
                "user",
                JSON.stringify(response.data)
            );


            setBanner({
                type: "success",
                text: "Login successful. Redirecting..."
            });



            // Role based redirect

            setTimeout(() => {


                if (response.data.role === "Candidate") {

                    navigate("/candidate-dashboard");

                }
                else if (response.data.role === "Recruiter") {

                    navigate("/recruiter-dashboard");

                }
                else {

                    navigate("/dashboard");

                }


            }, 1000);



        } catch (error) {


            console.log(
                "LOGIN ERROR:",
                error.response?.data
            );


            const message =
                error.response?.data?.message ||
                (typeof error.response?.data === "string"
                    ? error.response.data
                    : null) ||
                "Invalid email or password.";



            setBanner({
                type: "error",
                text: message
            });



        } finally {

            setSubmitting(false);

        }

    };



    return (

        <div className="reg-page">


            <div className="reg-brand">


                <div className="reg-brand-mark">

                    hire<span>flow</span>

                </div>


                <div className="reg-brand-copy">

                    <span className="reg-eyebrow">
                        Welcome back
                    </span>


                    <h1 className="reg-headline">
                        Pick up right where you left off.
                    </h1>


                    <p className="reg-subcopy">

                        Sign in to check your applications,
                        manage your listings, and stay on top
                        of every conversation.

                    </p>


                </div>


                <div className="reg-brand-footer">

                    © {new Date().getFullYear()} Hireflow

                </div>


            </div>





            <div className="reg-form-side">


                <div className="reg-card">


                    <h1>
                        Sign in
                    </h1>


                    <p className="reg-lead">

                        New here?
                        <a href="/register">
                            Create an account
                        </a>

                    </p>



                    {banner && (

                        <div className={`reg-banner ${banner.type}`}>

                            {banner.text}

                        </div>

                    )}






                    <form onSubmit={handleLogin} noValidate>



                        <div className={`field ${errors.email ? "field-error" : ""}`}>


                            <label>
                                Email
                            </label>


                            <input

                                type="email"

                                placeholder="jane@example.com"

                                value={user.email}

                                onChange={
                                    updateField("email")
                                }

                            />


                            {
                                errors.email &&
                                <div className="field-error-msg">

                                    {errors.email}

                                </div>
                            }


                        </div>







                        <div className={`field ${errors.password ? "field-error" : ""}`}>


                            <label>
                                Password
                            </label>



                            <div className="password-wrap">


                                <input

                                    type={
                                        showPassword
                                            ? "text"
                                            : "password"
                                    }

                                    placeholder="Enter your password"

                                    value={user.password}

                                    onChange={
                                        updateField("password")
                                    }

                                />



                                <button

                                    type="button"

                                    className="password-toggle"

                                    onClick={() =>
                                        setShowPassword(!showPassword)
                                    }

                                >

                                    {
                                        showPassword
                                            ? "Hide"
                                            : "Show"
                                    }


                                </button>


                            </div>



                            {
                                errors.password &&

                                <div className="field-error-msg">

                                    {errors.password}

                                </div>

                            }



                        </div>






                        <p style={{
                            textAlign: "right",
                            margin: "-8px 0 18px"
                        }}>


                            <a
                                href="/forgot-password"
                                style={{
                                    fontSize: "12.5px",
                                    color: "var(--slate-soft)"
                                }}
                            >

                                Forgot password?

                            </a>


                        </p>






                        <button

                            type="submit"

                            className="reg-submit"

                            disabled={submitting}

                        >

                            {
                                submitting
                                    ? "Signing in..."
                                    : "Sign in"
                            }


                        </button>




                    </form>


                </div>


            </div>


        </div>


    );

}


export default Login;