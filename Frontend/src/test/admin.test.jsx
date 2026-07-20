import React from 'react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import AdminHome from '../pages/admin/AdminHome';
import AdminUsersPage from '../pages/admin/AdminUsersPage';
import AdminOrganizationsPage from '../pages/admin/AdminOrganizationsPage';
import AdminUserDetailPage from '../pages/admin/AdminUserDetailPage';
import ProtectedRoute from '../components/ProtectedRoute';
import AccessDenied from '../pages/AccessDenied';
import { AuthContext } from '../auth/auth-context';
import { authStub } from './authStub';

vi.mock('../api/axios', () => ({
    default: {
        get: vi.fn(),
        post: vi.fn(),
        put: vi.fn(),
        patch: vi.fn()
    }
}));

import api from '../api/axios';

function renderAdmin(ui, { route = '/admin', path } = {}, authOverrides = {}) {
    return render(
        <AuthContext.Provider value={authStub({
            user: { fullName: 'Admin', role: 'Admin', email: 'a@example.com', userId: 1 },
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

describe('Admin dashboard', () => {
    beforeEach(() => vi.clearAllMocks());

    it('renders metrics on success', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                activeUsers: 2,
                disabledUsers: 0,
                pendingRecruiterRequests: 1,
                candidates: 1,
                recruiters: 0,
                hiringManagers: 0,
                administrators: 1,
                organizations: 1,
                departments: 1,
                activeJobs: 0,
                applications: 0,
                pendingFinalDecisions: 0,
                upcomingInterviews: 0,
                recentAuditEvents: []
            }
        });
        renderAdmin(<AdminHome />);
        expect(screen.getByText(/loading administrator dashboard/i)).toBeInTheDocument();
        expect(await screen.findByRole('heading', { name: /dashboard/i })).toBeInTheDocument();
        expect(screen.getByText(/no recent audit events/i)).toBeInTheDocument();
    });
});

describe('Admin users and org form', () => {
    beforeEach(() => vi.clearAllMocks());

    it('shows user filters', async () => {
        api.get.mockResolvedValueOnce({ data: { items: [], totalCount: 0 } });
        renderAdmin(<AdminUsersPage />);
        expect(await screen.findByRole('heading', { name: /users/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/search/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/^role$/i)).toBeInTheDocument();
    });

    it('validates organization form', async () => {
        api.get.mockResolvedValueOnce({ data: [] });
        renderAdmin(<AdminOrganizationsPage />);
        expect(await screen.findByRole('heading', { name: /organizations/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/^name$/i)).toBeRequired();
        expect(screen.getByLabelText(/^code$/i)).toBeRequired();
    });

    it('shows self-disable protection note for own user', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                userId: 1,
                fullName: 'Admin',
                email: 'a@example.com',
                role: 'Admin',
                status: 'Active',
                roles: [{ roleId: 1, roleName: 'Admin' }]
            }
        });
        renderAdmin(<AdminUserDetailPage />, {
            route: '/admin/users/1',
            path: '/admin/users/:id'
        });
        expect(await screen.findByText(/self-disable is blocked/i)).toBeInTheDocument();
    });
});

describe('Non-admin denied admin route', () => {
    it('denies Candidate', async () => {
        render(
            <AuthContext.Provider value={authStub({
                user: { fullName: 'Cand', role: 'Candidate', email: 'c@example.com' },
                token: 'tok',
                isAuthenticated: true
            })}>
                <MemoryRouter initialEntries={['/admin']}>
                    <Routes>
                        <Route
                            path="/admin"
                            element={(
                                <ProtectedRoute roles={['Admin']}>
                                    <div>admin secret</div>
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

describe('Admin phase 7.2 pages', () => {
    beforeEach(() => vi.clearAllMocks());

    it('loads audit logs and export control', async () => {
        api.get.mockResolvedValueOnce({ data: { items: [], totalCount: 0 } });
        const { default: AdminAuditPage } = await import('../pages/admin/AdminAuditPage');
        renderAdmin(<AdminAuditPage />);
        expect(await screen.findByRole('heading', { name: /audit logs/i })).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /export csv/i })).toBeInTheDocument();
        expect(screen.getByText(/no audit events/i)).toBeInTheDocument();
    });

    it('shows NotConfigured provider statuses', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                apiHealth: 'Operational',
                databaseConnectivity: 'Connected',
                pendingRecruiterRequests: 0,
                disabledAccounts: 0,
                pendingAssessments: 0,
                upcomingInterviews: 0,
                pendingFinalDecisions: 0,
                emailProviderStatus: 'NotConfigured',
                smsProviderStatus: 'NotConfigured',
                calendarProviderStatus: 'NotConfigured',
                storageProviderStatus: 'NotConfigured',
                providerNotes: 'Phase 8'
            }
        });
        const { default: AdminMonitoringPage } = await import('../pages/admin/AdminMonitoringPage');
        renderAdmin(<AdminMonitoringPage />);
        expect(await screen.findByRole('heading', { name: /monitoring/i })).toBeInTheDocument();
        expect(screen.getByText(/email: notconfigured/i)).toBeInTheDocument();
    });

    it('loads analytics empty skill demand', async () => {
        api.get
            .mockResolvedValueOnce({
                data: {
                    applicationsByStatus: [],
                    shortlisted: 0,
                    rejected: 0,
                    hired: 0,
                    unavailableMetricsNote: 'note'
                }
            })
            .mockResolvedValueOnce({ data: { skillDemandFromJobs: [] } })
            .mockResolvedValueOnce({ data: { jobsByDepartment: [] } });
        const { default: AdminAnalyticsPage } = await import('../pages/admin/AdminAnalyticsPage');
        renderAdmin(<AdminAnalyticsPage />);
        expect(await screen.findByRole('heading', { name: /recruitment analytics/i })).toBeInTheDocument();
        expect(screen.getByText(/no skill demand data/i)).toBeInTheDocument();
    });

    it('requires reason on final decision form', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                applicationId: 9,
                candidateName: 'C',
                jobTitle: 'J',
                applicationStatus: 'Interviewed',
                latestRecommendation: 'RecommendHire',
                warnings: []
            }
        });
        const { AdminFinalDecisionDetailPage } = await import('../pages/admin/AdminFinalDecisionsPage');
        renderAdmin(<AdminFinalDecisionDetailPage />, {
            route: '/admin/final-decisions/9',
            path: '/admin/final-decisions/:applicationId'
        });
        expect(await screen.findByRole('heading', { name: /final decision review/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/reason/i)).toBeRequired();
    });
});
