import { BrowserRouter, Navigate, Routes, Route } from "react-router-dom";
import { AuthProvider } from "./auth/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import Navbar from "./components/Navbar";
import Home from "./pages/Home";
import Login from "./pages/Login";
import Register from "./pages/Register";
import RecruiterRequest from "./pages/RecruiterRequest";
import AccessDenied from "./pages/AccessDenied";
import SessionExpired from "./pages/SessionExpired";
import CandidateHome from "./pages/candidate/CandidateHome";
import CandidateProfilePage from "./pages/candidate/CandidateProfilePage";
import RecruiterDashboard from "./pages/RecruiterDashboard";
import { AdminDashboard, HiringManagerDashboard } from "./pages/RoleDashboards";
import "./App.css";

function App() {
    return (
        <AuthProvider>
            <BrowserRouter>
                <Navbar />
                <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/recruiter-request" element={<RecruiterRequest />} />
                    <Route path="/access-denied" element={<AccessDenied />} />
                    <Route path="/session-expired" element={<SessionExpired />} />

                    <Route
                        path="/candidate"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateHome />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/profile"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateProfilePage />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/recruiter/*"
                        element={
                            <ProtectedRoute roles={["Recruiter"]}>
                                <RecruiterDashboard />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/hiring-manager/*"
                        element={
                            <ProtectedRoute roles={["HiringManager"]}>
                                <HiringManagerDashboard />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/admin/*"
                        element={
                            <ProtectedRoute roles={["Admin"]}>
                                <AdminDashboard />
                            </ProtectedRoute>
                        }
                    />

                    <Route path="/candidate-dashboard" element={<Navigate to="/candidate" replace />} />
                    <Route path="/recruiter-dashboard" element={<Navigate to="/recruiter" replace />} />
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    );
}

export default App;
