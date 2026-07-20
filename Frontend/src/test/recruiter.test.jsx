import React from 'react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import RecruiterHome from '../pages/recruiter/RecruiterHome';
import RecruiterJobFormPage from '../pages/recruiter/RecruiterJobFormPage';
import RecruiterJobsPage from '../pages/recruiter/RecruiterJobsPage';
import RecruiterPipelinePage from '../pages/recruiter/RecruiterPipelinePage';
import RecruiterComparePage from '../pages/recruiter/RecruiterComparePage';
import ProtectedRoute from '../components/ProtectedRoute';
import AccessDenied from '../pages/AccessDenied';
import { AuthContext } from '../auth/auth-context';
import { authStub } from './authStub';

vi.mock('../api/axios', () => ({
    default: {
        get: vi.fn(),
        post: vi.fn(),
        put: vi.fn(),
        patch: vi.fn(),
        delete: vi.fn()
    }
}));

import api from '../api/axios';

function renderWithAuth(ui, { route = '/', path } = {}, authOverrides = {}) {
    return render(
        <AuthContext.Provider value={authStub({
            user: { fullName: 'Test Recruiter', role: 'Recruiter', email: 'r@example.com' },
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

describe('Recruiter dashboard', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders loading then empty success metrics', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                activeJobs: 0,
                draftJobs: 0,
                pausedJobs: 0,
                closedJobs: 0,
                totalApplicants: 0,
                newApplicants: 0,
                candidatesInScreening: 0,
                shortlistedCandidates: 0,
                pendingAssessments: 0,
                upcomingInterviews: 0,
                recentActivity: []
            }
        });

        renderWithAuth(<RecruiterHome />);
        expect(screen.getByText(/loading recruiter dashboard/i)).toBeInTheDocument();
        expect(await screen.findByRole('heading', { name: /dashboard/i })).toBeInTheDocument();
        expect(screen.getByText(/no recruitment activity yet/i)).toBeInTheDocument();
        expect(screen.getByText(/live metrics for your organization only/i)).toBeInTheDocument();
    });

    it('shows error state', async () => {
        api.get.mockRejectedValueOnce({ response: { data: { message: 'Dashboard unavailable.' } } });
        renderWithAuth(<RecruiterHome />);
        expect(await screen.findByText(/dashboard unavailable/i)).toBeInTheDocument();
    });
});

describe('Recruiter job form validation', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('requires title description and location', async () => {
        const user = userEvent.setup();
        renderWithAuth(<RecruiterJobFormPage />, { route: '/recruiter/jobs/new', path: '/recruiter/jobs/new' });

        await user.click(screen.getByRole('button', { name: /create draft/i }));
        expect(await screen.findByText(/title, description, and location are required/i)).toBeInTheDocument();
        expect(api.post).not.toHaveBeenCalled();
    });
});

describe('Recruiter job list filtering', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        api.get.mockResolvedValue({
            data: {
                items: [
                    {
                        id: 1,
                        title: 'Engineer',
                        status: 'Draft',
                        location: 'Colombo',
                        applicantCount: 0
                    }
                ],
                totalCount: 1
            }
        });
    });

    it('loads jobs and submits filters', async () => {
        const user = userEvent.setup();
        renderWithAuth(<RecruiterJobsPage />);
        expect(await screen.findByText('Engineer')).toBeInTheDocument();

        await user.type(screen.getByLabelText(/keyword/i), 'Engineer');
        await user.click(screen.getByRole('button', { name: /filter/i }));

        await waitFor(() => {
            expect(api.get).toHaveBeenCalledWith('/recruiter/jobs', expect.objectContaining({
                params: expect.objectContaining({ keyword: 'Engineer' })
            }));
        });
    });
});

describe('Protected recruiter route', () => {
    it('denies candidate from recruiter route', async () => {
        render(
            <AuthContext.Provider value={authStub({
                user: { fullName: 'Cand', role: 'Candidate', email: 'c@example.com' },
                token: 'tok',
                isAuthenticated: true
            })}>
                <MemoryRouter initialEntries={['/recruiter']}>
                    <Routes>
                        <Route
                            path="/recruiter"
                            element={(
                                <ProtectedRoute roles={['Recruiter']}>
                                    <div>Recruiter secret</div>
                                </ProtectedRoute>
                            )}
                        />
                        <Route path="/access-denied" element={<AccessDenied />} />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );

        expect(await screen.findByText(/access denied/i)).toBeInTheDocument();
        expect(screen.queryByText(/recruiter secret/i)).not.toBeInTheDocument();
    });

    it('allows recruiter on protected route', async () => {
        render(
            <AuthContext.Provider value={authStub({
                user: { fullName: 'Rec', role: 'Recruiter', email: 'r@example.com' },
                token: 'tok',
                isAuthenticated: true
            })}>
                <MemoryRouter initialEntries={['/recruiter']}>
                    <Routes>
                        <Route
                            path="/recruiter"
                            element={(
                                <ProtectedRoute roles={['Recruiter']}>
                                    <div>Recruiter secret</div>
                                </ProtectedRoute>
                            )}
                        />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );

        expect(await screen.findByText(/recruiter secret/i)).toBeInTheDocument();
    });
});

describe('Pipeline confirmation and comparison limit', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        api.get.mockResolvedValue({
            data: {
                items: [
                    {
                        applicationId: 11,
                        candidateName: 'Ada',
                        status: 'Pending',
                        matchScore: 70,
                        yearsOfExperience: 3
                    },
                    {
                        applicationId: 12,
                        candidateName: 'Grace',
                        status: 'Pending',
                        matchScore: 65,
                        yearsOfExperience: 4
                    }
                ],
                totalCount: 2
            }
        });
        api.patch.mockResolvedValue({});
    });

    it('asks for confirmation before status action', async () => {
        const user = userEvent.setup();
        renderWithAuth(<RecruiterPipelinePage />, {
            route: '/recruiter/jobs/5/applicants',
            path: '/recruiter/jobs/:id/applicants'
        });

        expect(await screen.findByText('Ada')).toBeInTheDocument();
        await user.click(screen.getAllByRole('button', { name: /shortlist/i })[0]);
        expect(screen.getByText(/confirm moving application/i)).toBeInTheDocument();
        await user.click(screen.getByRole('button', { name: /^confirm$/i }));
        await waitFor(() => {
            expect(api.patch).toHaveBeenCalledWith(
                '/recruiter/applications/11/status',
                expect.objectContaining({ status: 'Shortlisted' })
            );
        });
    });

    it('limits comparison selection to 5', async () => {
        api.get.mockResolvedValue({
            data: {
                items: Array.from({ length: 6 }, (_, i) => ({
                    applicationId: i + 1,
                    candidateName: `Cand ${i + 1}`,
                    status: 'Pending',
                    matchScore: 50,
                    yearsOfExperience: 1
                })),
                totalCount: 6
            }
        });
        const user = userEvent.setup();
        renderWithAuth(<RecruiterPipelinePage />, {
            route: '/recruiter/jobs/5/applicants',
            path: '/recruiter/jobs/:id/applicants'
        });

        await screen.findByText('Cand 1');
        const boxes = screen.getAllByRole('checkbox');
        for (let i = 0; i < 6; i += 1) {
            await user.click(boxes[i]);
        }
        expect(await screen.findByText(/compare at most 5 applicants/i)).toBeInTheDocument();
    });
});

describe('Recruiter compare page', () => {
    it('validates selection count', async () => {
        const user = userEvent.setup();
        renderWithAuth(<RecruiterComparePage />);
        await user.clear(screen.getByLabelText(/application ids/i));
        await user.type(screen.getByLabelText(/application ids/i), '1');
        await user.click(screen.getByRole('button', { name: /compare/i }));
        expect(await screen.findByText(/select between 2 and 5 applicants/i)).toBeInTheDocument();
    });
});

describe('Phase 5.2 recruiter UI', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('shows ranking explanation and human-review notice', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                totalScore: 72,
                confidence: 'Medium',
                providerName: 'Deterministic',
                modelVersion: 'recruiter-rank-v1',
                explanation: 'Matched required skills: C#.',
                humanReviewNotice: 'AI-generated insight. Final recruitment decisions must be reviewed by authorized users.',
                matchedRequiredSkills: ['C#'],
                missingRequiredSkills: []
            }
        });
        const { default: RecruiterRankingPage } = await import('../pages/recruiter/RecruiterRankingPage');
        renderWithAuth(<RecruiterRankingPage />, {
            route: '/recruiter/applications/9/ranking',
            path: '/recruiter/applications/:id/ranking'
        });
        expect(await screen.findByText(/ai-generated insight/i)).toBeInTheDocument();
        expect(screen.getByTestId('ranking-explanation')).toHaveTextContent(/matched required skills/i);
    });

    it('validates assessment builder title', async () => {
        const user = userEvent.setup();
        const { default: RecruiterAssessmentBuilderPage } = await import('../pages/recruiter/RecruiterAssessmentBuilderPage');
        renderWithAuth(<RecruiterAssessmentBuilderPage />, {
            route: '/recruiter/assessments/new',
            path: '/recruiter/assessments/:id'
        });
        await user.click(screen.getByRole('button', { name: /^create$/i }));
        expect(await screen.findByText(/title is required/i)).toBeInTheDocument();
        expect(api.post).not.toHaveBeenCalled();
    });

    it('requires confirmation for screening decision', async () => {
        api.get.mockResolvedValueOnce({
            data: [{
                applicationId: 3,
                candidateName: 'Ada',
                jobTitle: 'Engineer',
                status: 'Pending',
                requiredAnswersCompleted: 1,
                requiredAnswersTotal: 1
            }]
        });
        const user = userEvent.setup();
        const { default: RecruiterScreeningPage } = await import('../pages/recruiter/RecruiterScreeningPage');
        renderWithAuth(<RecruiterScreeningPage />);
        expect(await screen.findByText('Ada')).toBeInTheDocument();
        await user.click(screen.getByRole('button', { name: /shortlist/i }));
        expect(screen.getByText(/confirm shortlisted/i)).toBeInTheDocument();
    });

    it('loads message thread and sends', async () => {
        api.get.mockResolvedValueOnce({ data: { messages: [], totalCount: 0 } });
        api.post.mockResolvedValue({});
        const user = userEvent.setup();
        const { default: RecruiterMessageThreadPage } = await import('../pages/recruiter/RecruiterMessageThreadPage');
        renderWithAuth(<RecruiterMessageThreadPage />, {
            route: '/recruiter/applications/4/messages',
            path: '/recruiter/applications/:id/messages'
        });
        expect(await screen.findByText(/no messages yet/i)).toBeInTheDocument();
        await user.type(screen.getByLabelText(/^message$/i), 'Hello candidate');
        await user.click(screen.getByRole('button', { name: /send message/i }));
        await waitFor(() => {
            expect(api.post).toHaveBeenCalledWith(
                '/recruiter/applications/4/messages',
                { body: 'Hello candidate' }
            );
        });
    });

    it('validates interview schedule form', async () => {
        const user = userEvent.setup();
        const { default: RecruiterScheduleInterviewPage } = await import('../pages/recruiter/RecruiterScheduleInterviewPage');
        renderWithAuth(<RecruiterScheduleInterviewPage />);
        await user.click(screen.getByRole('button', { name: /^schedule$/i }));
        expect(await screen.findByText(/application id and start time are required/i)).toBeInTheDocument();
    });

    it('shows interview conflict warning', async () => {
        api.post.mockResolvedValueOnce({
            data: {
                scheduled: false,
                conflicts: [{ conflictType: 'Candidate', message: 'Candidate already has an overlapping interview.' }]
            }
        });
        const user = userEvent.setup();
        const { default: RecruiterScheduleInterviewPage } = await import('../pages/recruiter/RecruiterScheduleInterviewPage');
        renderWithAuth(<RecruiterScheduleInterviewPage />);
        await user.type(screen.getByLabelText(/application id/i), '12');
        await user.type(screen.getByLabelText(/start/i), '2030-01-01T10:00');
        await user.click(screen.getByRole('button', { name: /^schedule$/i }));
        expect(await screen.findByText(/conflict warning/i)).toBeInTheDocument();
        expect(screen.getByText(/overlapping interview/i)).toBeInTheDocument();
    });

    it('loads reports empty and success states', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                applicationsTotal: 0,
                shortlistRate: 0,
                rejectionRate: 0,
                assessmentAssignments: 0,
                interviewsScheduled: 0,
                applicationsByStatus: [],
                applicationsOverTime: []
            }
        });
        const { default: RecruiterReportsPage } = await import('../pages/recruiter/RecruiterReportsPage');
        renderWithAuth(<RecruiterReportsPage />);
        expect(await screen.findByText(/no applications in the selected range/i)).toBeInTheDocument();
    });
});
