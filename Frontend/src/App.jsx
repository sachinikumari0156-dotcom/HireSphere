import { BrowserRouter, Routes, Route } from "react-router-dom";


import Navbar from "./components/Navbar";


import Home from "./pages/Home";
import Login from "./pages/Login";
import Register from "./pages/Register";

import CandidateDashboard from "./pages/CandidateDashboard";
import RecruiterDashboard from "./pages/RecruiterDashboard";


import "./App.css";


function App() {

    return (

        <BrowserRouter>

            <Navbar />


            <Routes>


                <Route
                    path="/"
                    element={<Home />}
                />


                <Route
                    path="/login"
                    element={<Login />}
                />


                <Route
                    path="/register"
                    element={<Register />}
                />


                <Route
                    path="/candidate-dashboard"
                    element={<CandidateDashboard />}
                />


                <Route
                    path="/recruiter-dashboard"
                    element={<RecruiterDashboard />}
                />


            </Routes>


        </BrowserRouter>

    );

}


export default App;