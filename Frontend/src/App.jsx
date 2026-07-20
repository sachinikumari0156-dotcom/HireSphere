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
import CandidateJobsPage from "./pages/candidate/CandidateJobsPage";
import CandidateJobDetailPage from "./pages/candidate/CandidateJobDetailPage";
import CandidateRecommendationsPage from "./pages/candidate/CandidateRecommendationsPage";
import CandidateApplyPage from "./pages/candidate/CandidateApplyPage";
import CandidateApplicationsPage from "./pages/candidate/CandidateApplicationsPage";
import CandidateApplicationDetailPage from "./pages/candidate/CandidateApplicationDetailPage";
import CandidateAssessmentsPage from "./pages/candidate/CandidateAssessmentsPage";
import CandidateAssessmentDetailPage from "./pages/candidate/CandidateAssessmentDetailPage";
import CandidateInterviewsPage from "./pages/candidate/CandidateInterviewsPage";
import CandidateInterviewDetailPage from "./pages/candidate/CandidateInterviewDetailPage";
import CandidateNotificationsPage from "./pages/candidate/CandidateNotificationsPage";
import RecruiterLayout from "./pages/recruiter/RecruiterLayout";
import RecruiterHome from "./pages/recruiter/RecruiterHome";
import RecruiterJobsPage from "./pages/recruiter/RecruiterJobsPage";
import RecruiterJobFormPage from "./pages/recruiter/RecruiterJobFormPage";
import RecruiterJobDetailPage from "./pages/recruiter/RecruiterJobDetailPage";
import RecruiterPipelinePage from "./pages/recruiter/RecruiterPipelinePage";
import RecruiterApplicationPage from "./pages/recruiter/RecruiterApplicationPage";
import RecruiterComparePage from "./pages/recruiter/RecruiterComparePage";
import RecruiterScreeningPage from "./pages/recruiter/RecruiterScreeningPage";
import RecruiterRankingPage from "./pages/recruiter/RecruiterRankingPage";
import RecruiterAssessmentsPage from "./pages/recruiter/RecruiterAssessmentsPage";
import RecruiterAssessmentBuilderPage from "./pages/recruiter/RecruiterAssessmentBuilderPage";
import RecruiterMessageThreadPage from "./pages/recruiter/RecruiterMessageThreadPage";
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
                        path="/candidate/jobs"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateJobsPage />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/jobs/:id"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateJobDetailPage />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/jobs/:id/apply"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateApplyPage />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/recommendations"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateRecommendationsPage />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/applications"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateApplicationsPage />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/applications/:id"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateApplicationDetailPage />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/assessments"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateAssessmentsPage />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/assessments/:id"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateAssessmentDetailPage />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/interviews"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateInterviewsPage />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/interviews/:id"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateInterviewDetailPage />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/notifications"
                        element={
                            <ProtectedRoute roles={["Candidate"]}>
                                <CandidateNotificationsPage />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/recruiter"
                        element={
                            <ProtectedRoute roles={["Recruiter"]}>
                                <RecruiterLayout />
                            </ProtectedRoute>
                        }
                    >
                        <Route index element={<RecruiterHome />} />
                        <Route path="jobs" element={<RecruiterJobsPage />} />
                        <Route path="jobs/new" element={<RecruiterJobFormPage />} />
                        <Route path="jobs/:id" element={<RecruiterJobDetailPage />} />
                        <Route path="jobs/:id/edit" element={<RecruiterJobFormPage />} />
                        <Route path="jobs/:id/applicants" element={<RecruiterPipelinePage />} />
                        <Route path="applications/:id" element={<RecruiterApplicationPage />} />
                        <Route path="applications/:id/ranking" element={<RecruiterRankingPage />} />
                        <Route path="applications/:id/messages" element={<RecruiterMessageThreadPage />} />
                        <Route path="screening" element={<RecruiterScreeningPage />} />
                        <Route path="assessments" element={<RecruiterAssessmentsPage />} />
                        <Route path="assessments/new" element={<RecruiterAssessmentBuilderPage />} />
                        <Route path="assessments/:id" element={<RecruiterAssessmentBuilderPage />} />
                        <Route path="compare" element={<RecruiterComparePage />} />
                    </Route>
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
                    <Route path="/candidate-profile" element={<Navigate to="/candidate/profile" replace />} />
                    <Route path="/recruiter-dashboard" element={<Navigate to="/recruiter" replace />} />
                    <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    );
}

export default App;
