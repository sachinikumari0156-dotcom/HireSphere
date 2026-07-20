import React from 'react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import HiringManagerHome from '../pages/hiring-manager/HiringManagerHome';
import HiringManagerJobsPage from '../pages/hiring-manager/HiringManagerJobsPage';
import HiringManagerApplicationPage from '../pages/hiring-manager/HiringManagerApplicationPage';
import HiringManagerCandidatesPage from '../pages/hiring-manager/HiringManagerCandidatesPage';
import HiringManagerInterviewDetailPage from '../pages/hiring-manager/HiringManagerInterviewDetailPage';
import HiringManagerEvaluationPage from '../pages/hiring-manager/HiringManagerEvaluationPage';
import ProtectedRoute from '../components/ProtectedRoute';
import AccessDenied from '../pages/AccessDenied';
import { AuthContext } from '../auth/auth-context';
import { authStub } from './authStub';

vi.mock('../api/axios', () => ({
    default: {
        get: vi.fn(),
        post: vi.fn(),
        put: vi.fn()
    }
}));

import api from '../api/axios';

function renderWithAuth(ui, { route = '/', path } = {}, authOverrides = {}) {
    return render(
        <AuthContext.Provider value={authStub({
            user: { fullName: 'Test HM', role: 'HiringManager', email: 'hm@example.com' },
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

describe('Hiring Manager dashboard', () => {
    beforeEach(() => vi.clearAllMocks());

    it('renders loading then empty success metrics', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                assignedActiveVacancies: 0,
                assignedPausedVacancies: 0,
                candidatesAwaitingReview: 0,
                candidatesShortlisted: 0,
                upcomingInterviews: 0,
                pendingInterviewFeedback: 0,
                pendingEvaluations: 0,
                pendingHiringDecisions: 0,
                recentActivity: []
            }
        });
        renderWithAuth(<HiringManagerHome />);
        expect(screen.getByText(/loading hiring manager dashboard/i)).toBeInTheDocument();
        expect(await screen.findByRole('heading', { name: /dashboard/i })).toBeInTheDocument();
        expect(screen.getByText(/no manager activity yet/i)).toBeInTheDocument();
    });
});

describe('Assigned vacancy list', () => {
    beforeEach(() => vi.clearAllMocks());

    it('shows vacancies', async () => {
        api.get.mockResolvedValueOnce({
            data: { items: [{ id: 1, title: 'Platform Eng', status: 'Published', applicantCount: 2, shortlistCount: 1 }], totalCount: 1 }
        });
        renderWithAuth(<HiringManagerJobsPage />);
        expect(await screen.findByText(/platform eng/i)).toBeInTheDocument();
    });
});

describe('Candidate review states', () => {
    beforeEach(() => vi.clearAllMocks());

    it('shows error state', async () => {
        api.get.mockRejectedValueOnce({ response: { data: { message: 'Application not found or access denied.' } } });
        renderWithAuth(<HiringManagerApplicationPage />, {
            route: '/hiring-manager/applications/9',
            path: '/hiring-manager/applications/:id'
        });
        expect(await screen.findByText(/application not found or access denied/i)).toBeInTheDocument();
    });

    it('shows success review with human-review notice', async () => {
        api.get.mockResolvedValue({
            data: {
                applicationId: 9,
                jobId: 3,
                jobTitle: 'Role',
                candidateName: 'Ada',
                status: 'Shortlisted',
                matchScore: 80,
                matchExplanation: 'Strong skills',
                humanReviewNotice: 'AI-generated insight. Final recruitment decisions must be reviewed by authorized users.',
                skills: ['C#'],
                missingRequiredSkills: [],
                resumes: [{ documentId: 1, fileName: 'ada.pdf', isPrimary: true }]
            }
        });
        renderWithAuth(<HiringManagerApplicationPage />, {
            route: '/hiring-manager/applications/9',
            path: '/hiring-manager/applications/:id'
        });
        expect(await screen.findByRole('heading', { name: /candidate review/i }, { timeout: 5000 })).toBeInTheDocument();
        expect(screen.getByText(/ai-generated insight/i)).toBeInTheDocument();
        expect(screen.getByText(/ada.pdf/i)).toBeInTheDocument();
    });
});
describe('Comparison selection limit', () => {
    beforeEach(() => vi.clearAllMocks());

    it('limits selection to 5', async () => {
        const user = userEvent.setup();
        api.get.mockResolvedValueOnce({
            data: {
                items: Array.from({ length: 6 }, (_, i) => ({
                    applicationId: i + 1,
                    candidateName: `Cand ${i + 1}`,
                    status: 'Shortlisted',
                    matchScore: 50
                }))
            }
        });
        renderWithAuth(<HiringManagerCandidatesPage />, {
            route: '/hiring-manager/jobs/4/candidates',
            path: '/hiring-manager/jobs/:id/candidates'
        });
        await screen.findByText(/cand 1/i);
        for (let i = 1; i <= 6; i += 1) {
            await user.click(screen.getByLabelText(`Select Cand ${i}`));
        }
        expect(await screen.findByText(/comparison is limited to 5/i)).toBeInTheDocument();
    });
});

describe('Feedback and evaluation forms', () => {
    beforeEach(() => vi.clearAllMocks());

    it('loads interview feedback form', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                id: 8,
                applicationId: 3,
                candidateName: 'Ada',
                jobTitle: 'Role',
                interviewDateUtc: '2030-01-01T10:00:00Z',
                timeZoneId: 'Asia/Colombo',
                candidateResponse: 'Confirmed',
                myFeedback: null
            }
        });
        renderWithAuth(<HiringManagerInterviewDetailPage />, {
            route: '/hiring-manager/interviews/8',
            path: '/hiring-manager/interviews/:id'
        });
        expect(await screen.findByRole('heading', { name: /interview detail/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/recommendation/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/private panel comments/i)).toBeInTheDocument();
    });

    it('shows evaluation draft/submit controls', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                id: 1,
                applicationId: 3,
                submissionStatus: 'Draft',
                justification: 'Draft text',
                requiredSkillsAlignment: 70
            }
        });
        renderWithAuth(<HiringManagerEvaluationPage />, {
            route: '/hiring-manager/applications/3/evaluation',
            path: '/hiring-manager/applications/:id/evaluation'
        });
        expect(await screen.findByRole('heading', { name: /candidate evaluation/i })).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /save draft/i })).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /submit evaluation/i })).toBeInTheDocument();
        expect(screen.getByText(/status: draft/i)).toBeInTheDocument();
    });
});

    it('denies Candidate', async () => {
        render(
            <AuthContext.Provider value={authStub({
                user: { fullName: 'Cand', role: 'Candidate', email: 'c@example.com' },
                token: 'tok',
                isAuthenticated: true
            })}>
                <MemoryRouter initialEntries={['/hiring-manager']}>
                    <Routes>
                        <Route
                            path="/hiring-manager"
                            element={(
                                <ProtectedRoute roles={['HiringManager']}>
                                    <div>manager secret</div>
                                </ProtectedRoute>
                            )}
                        />
                        <Route path="/access-denied" element={<AccessDenied />} />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );
        expect(await screen.findByText(/access denied/i)).toBeInTheDocument();
        expect(screen.queryByText(/manager secret/i)).not.toBeInTheDocument();
    });

    it('denies Recruiter', async () => {
        render(
            <AuthContext.Provider value={authStub({
                user: { fullName: 'Rec', role: 'Recruiter', email: 'r@example.com' },
                token: 'tok',
                isAuthenticated: true
            })}>
                <MemoryRouter initialEntries={['/hiring-manager']}>
                    <Routes>
                        <Route
                            path="/hiring-manager"
                            element={(
                                <ProtectedRoute roles={['HiringManager']}>
                                    <div>manager secret</div>
                                </ProtectedRoute>
                            )}
                        />
                        <Route path="/access-denied" element={<AccessDenied />} />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );
        expect(await screen.findByText(/access denied/i)).toBeInTheDocument();
    });
});
