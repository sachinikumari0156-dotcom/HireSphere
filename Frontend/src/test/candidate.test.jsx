import React from 'react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import CandidateHome from '../pages/candidate/CandidateHome';
import CandidateProfilePage from '../pages/candidate/CandidateProfilePage';
import CandidateJobsPage from '../pages/candidate/CandidateJobsPage';
import CandidateRecommendationsPage from '../pages/candidate/CandidateRecommendationsPage';
import CandidateApplyPage from '../pages/candidate/CandidateApplyPage';
import CandidateAssessmentsPage from '../pages/candidate/CandidateAssessmentsPage';
import CandidateInterviewsPage from '../pages/candidate/CandidateInterviewsPage';
import CandidateNotificationsPage from '../pages/candidate/CandidateNotificationsPage';
import { AuthContext } from '../auth/auth-context';
import { authStub } from './authStub';

vi.mock('../api/axios', () => ({
    default: {
        get: vi.fn(),
        post: vi.fn(),
        put: vi.fn(),
        delete: vi.fn()
    }
}));

import api from '../api/axios';

function renderWithAuth(ui, { route = '/', path } = {}, authOverrides = {}) {
    return render(
        <AuthContext.Provider value={authStub({
            user: { fullName: 'Test Candidate', role: 'Candidate', email: 'c@example.com' },
            token: 'tok',
            isAuthenticated: true,
            ...authOverrides
        })}>
            <MemoryRouter initialEntries={[route]}>
                {path ? (
                    <Routes>
                        <Route path={path} element={ui} />
                    </Routes>
                ) : ui}
            </MemoryRouter>
        </AuthContext.Provider>
    );
}

describe('Candidate dashboard', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders backend-driven summary and empty state', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                profileCompletionPercent: 20,
                latestApplicationsCount: 0,
                interviewsCount: 0,
                assessmentsCount: 0,
                recommendationsCount: 0,
                unreadNotificationsCount: 0,
                resumeAnalysisStatus: 'None'
            }
        });

        renderWithAuth(<CandidateHome />);

        expect(await screen.findByRole('heading', { name: /candidate dashboard/i })).toBeInTheDocument();
        expect(screen.getByText(/20%/)).toBeInTheDocument();
        expect(screen.getByText(/no applications, interviews, or assessments yet/i)).toBeInTheDocument();
        expect(screen.getByRole('link', { name: /profile & documents/i })).toBeInTheDocument();
        expect(screen.getByRole('link', { name: /browse jobs/i })).toBeInTheDocument();
    });

    it('shows error state when dashboard fails', async () => {
        api.get.mockRejectedValueOnce({ response: { data: { message: 'Dashboard unavailable.' } } });
        renderWithAuth(<CandidateHome />);
        expect(await screen.findByText(/dashboard unavailable/i)).toBeInTheDocument();
    });
});

describe('Candidate profile page', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('loads profile form from API', async () => {
        api.get
            .mockResolvedValueOnce({
                data: {
                    id: 1,
                    fullName: 'Test Candidate',
                    phoneNumber: null,
                    address: null,
                    summary: null,
                    location: null,
                    desiredJobTitle: 'Developer',
                    yearsOfExperience: 2,
                    preferredWorkArrangement: null,
                    salaryExpectation: null,
                    availability: null,
                    portfolioUrl: null,
                    linkedInUrl: null,
                    gitHubUrl: null,
                    workExperiences: [],
                    educations: [],
                    skills: [],
                    certifications: [],
                    resumes: [],
                    documents: []
                }
            })
            .mockResolvedValueOnce({ data: [] });

        renderWithAuth(<CandidateProfilePage />);

        expect(await screen.findByRole('heading', { name: /profile & documents/i })).toBeInTheDocument();
        await waitFor(() => {
            expect(screen.getByDisplayValue('Developer')).toBeInTheDocument();
        });
        expect(api.get).toHaveBeenCalledWith('/candidate/profile');
    });
});

describe('Candidate jobs and recommendations', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('lists jobs from search API', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                items: [
                    {
                        id: 7,
                        title: 'C# Developer',
                        location: 'Colombo',
                        employmentType: 'FullTime',
                        workArrangement: 'Hybrid',
                        description: 'Build APIs',
                        matchScore: 72
                    }
                ],
                page: 1,
                pageSize: 10,
                totalCount: 1,
                totalPages: 1
            }
        });

        renderWithAuth(<CandidateJobsPage />);

        expect(await screen.findByRole('heading', { name: /browse jobs/i })).toBeInTheDocument();
        expect(await screen.findByRole('link', { name: /c# developer/i })).toBeInTheDocument();
        expect(screen.getByText(/match score: 72%/i)).toBeInTheDocument();
        expect(api.get).toHaveBeenCalledWith('/candidate/jobs', expect.objectContaining({
            params: expect.objectContaining({ page: 1, pageSize: 10 })
        }));
    });

    it('shows incomplete-profile message on recommendations', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                profileCompleteEnough: false,
                profileCompletionPercent: 15,
                message: 'Add at least one skill and complete more of your profile to receive job recommendations.',
                jobs: []
            }
        });

        renderWithAuth(<CandidateRecommendationsPage />);

        expect(await screen.findByRole('heading', { name: /recommended jobs/i })).toBeInTheDocument();
        expect(await screen.findByText(/add at least one skill/i)).toBeInTheDocument();
    });

    it('submits application wizard with terms accepted', async () => {
        const user = userEvent.setup();
        api.get.mockResolvedValueOnce({
            data: {
                jobId: 7,
                jobTitle: 'C# Developer',
                canApply: true,
                resumes: [{ id: 3, fileName: 'cv.pdf', isPrimary: true, storageKey: 'k', uploadedAtUtc: '2026-07-20T00:00:00Z' }],
                screeningQuestions: [
                    { id: 11, questionText: 'Authorized to work?', questionType: 'YesNo', isRequired: true, sortOrder: 1 }
                ]
            }
        });
        api.post.mockResolvedValueOnce({ data: { id: 99 } });

        renderWithAuth(<CandidateApplyPage />, { route: '/candidate/jobs/7/apply', path: '/candidate/jobs/:id/apply' });

        expect(await screen.findByRole('heading', { name: /apply — c# developer/i })).toBeInTheDocument();
        await user.click(screen.getByRole('button', { name: /continue/i }));
        await user.type(screen.getByLabelText(/authorized to work/i), 'Yes');
        await user.click(screen.getByRole('button', { name: /continue/i }));
        await user.click(screen.getByLabelText(/accept the application terms/i));
        await user.click(screen.getByRole('button', { name: /submit application/i }));

        await waitFor(() => {
            expect(api.post).toHaveBeenCalledWith('/candidate/applications', expect.objectContaining({
                jobId: 7,
                resumeId: 3,
                termsAccepted: true,
                screeningAnswers: [{ screeningQuestionId: 11, answerText: 'Yes' }]
            }));
        });
    });
});

describe('Candidate assessments page', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders empty state when no assessments assigned', async () => {
        api.get.mockResolvedValueOnce({ data: [] });
        renderWithAuth(<CandidateAssessmentsPage />);
        expect(await screen.findByRole('heading', { name: /skill assessments/i })).toBeInTheDocument();
        expect(screen.getByText(/no assessments assigned yet/i)).toBeInTheDocument();
    });
});

describe('Candidate interviews page', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('lists scheduled interviews from API', async () => {
        api.get.mockResolvedValueOnce({
            data: [{
                id: 5,
                applicationId: 2,
                jobTitle: 'Backend Engineer',
                interviewDateUtc: '2026-07-25T10:00:00Z',
                timeZoneId: 'Asia/Colombo',
                interviewType: 'Video',
                status: 'Scheduled',
                candidateResponse: 'Pending',
                meetingInfoAvailable: false
            }]
        });
        renderWithAuth(<CandidateInterviewsPage />);
        expect(await screen.findByRole('heading', { name: /interviews/i })).toBeInTheDocument();
        expect(screen.getByRole('link', { name: /backend engineer/i })).toBeInTheDocument();
    });
});

describe('Candidate notifications page', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('shows unread count and items', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                unreadCount: 1,
                items: [{
                    id: 1,
                    title: 'Application submitted',
                    message: 'Your application was submitted.',
                    category: 'ApplicationSubmitted',
                    isRead: false,
                    createdAtUtc: '2026-07-20T12:00:00Z',
                    linkPath: '/candidate/applications/9'
                }]
            }
        });
        renderWithAuth(<CandidateNotificationsPage />);
        expect(await screen.findByRole('heading', { name: /notifications/i })).toBeInTheDocument();
        expect(screen.getByText(/1 unread/i)).toBeInTheDocument();
        expect(screen.getByText(/application submitted/i)).toBeInTheDocument();
    });
});
