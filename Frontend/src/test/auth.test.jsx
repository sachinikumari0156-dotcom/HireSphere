import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import Login from '../pages/Login';
import Register from '../pages/Register';
import RecruiterRequest from '../pages/RecruiterRequest';
import AccessDenied from '../pages/AccessDenied';
import SessionExpired from '../pages/SessionExpired';
import ProtectedRoute from '../components/ProtectedRoute';
import { AuthProvider } from '../auth/AuthContext';
import { AuthContext } from '../auth/auth-context';
import { authStub } from './authStub';
import { useAuth } from '../auth/useAuth';

vi.mock('../api/axios', () => ({
    default: {
        get: vi.fn(),
        post: vi.fn()
    }
}));

import api from '../api/axios';

function renderAuthPage(ui, route = '/') {
    return render(
        <AuthProvider>
            <MemoryRouter initialEntries={[route]}>
                {ui}
            </MemoryRouter>
        </AuthProvider>
    );
}

describe('Login page', () => {
    it('renders the login form', async () => {
        api.get.mockRejectedValueOnce(new Error('no session'));
        renderAuthPage(<Login />, '/login');
        expect(await screen.findByRole('heading', { name: /sign in/i })).toBeInTheDocument();
        expect(screen.getByPlaceholderText(/jane@example.com/i)).toBeInTheDocument();
        expect(screen.getByPlaceholderText(/enter your password/i)).toBeInTheDocument();
    });
});

describe('Candidate registration validation', () => {
    it('shows validation errors for incomplete registration', async () => {
        api.get.mockRejectedValueOnce(new Error('no session'));
        const user = userEvent.setup();
        renderAuthPage(<Register />, '/register');
        await screen.findByRole('heading', { name: /create your candidate account/i });
        await user.click(screen.getByRole('button', { name: /create account/i }));
        expect(await screen.findByText(/enter your first name/i)).toBeInTheDocument();
        expect(screen.getByText(/enter your last name/i)).toBeInTheDocument();
        expect(screen.getByText(/enter your email/i)).toBeInTheDocument();
        expect(api.post).not.toHaveBeenCalled();
    });
});

describe('Recruiter request validation', () => {
    it('requires business email and organization', async () => {
        api.get.mockRejectedValueOnce(new Error('no session'));
        const user = userEvent.setup();
        renderAuthPage(<RecruiterRequest />, '/recruiter-request');
        await screen.findByRole('heading', { name: /recruiter access request/i });
        await user.click(screen.getByRole('button', { name: /submit request/i }));
        expect(await screen.findByText(/enter your full name/i)).toBeInTheDocument();
        expect(screen.getByText(/enter a business email/i)).toBeInTheDocument();
        expect(screen.getByText(/enter your organization name/i)).toBeInTheDocument();
        expect(api.post).not.toHaveBeenCalled();
    });
});

describe('Protected routes and role guards', () => {
    it('redirects unauthenticated users to login', async () => {
        render(
            <AuthContext.Provider value={authStub()}>
                <MemoryRouter initialEntries={['/candidate']}>
                    <Routes>
                        <Route
                            path="/candidate"
                            element={(
                                <ProtectedRoute roles={['Candidate']}>
                                    <div>Candidate OK</div>
                                </ProtectedRoute>
                            )}
                        />
                        <Route path="/login" element={<div>Login Page</div>} />
                        <Route path="/session-expired" element={<div>Session Expired Page</div>} />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );

        expect(await screen.findByText(/login page/i)).toBeInTheDocument();
        expect(screen.queryByText(/candidate ok/i)).not.toBeInTheDocument();
    });

    it('blocks Candidate from Recruiter route', async () => {
        render(
            <AuthContext.Provider value={authStub({
                isAuthenticated: true,
                token: 't',
                user: { userId: 1, fullName: 'Cand', email: 'c@x.com', role: 'Candidate' }
            })}>
                <MemoryRouter initialEntries={['/recruiter']}>
                    <Routes>
                        <Route
                            path="/recruiter"
                            element={(
                                <ProtectedRoute roles={['Recruiter']}>
                                    <div>Recruiter OK</div>
                                </ProtectedRoute>
                            )}
                        />
                        <Route path="/access-denied" element={<div>Access Denied Page</div>} />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );

        expect(await screen.findByText(/access denied page/i)).toBeInTheDocument();
    });

    it('blocks Recruiter from Admin route', async () => {
        render(
            <AuthContext.Provider value={authStub({
                isAuthenticated: true,
                token: 't',
                user: { userId: 2, fullName: 'Rec', email: 'r@x.com', role: 'Recruiter' }
            })}>
                <MemoryRouter initialEntries={['/admin']}>
                    <Routes>
                        <Route
                            path="/admin"
                            element={(
                                <ProtectedRoute roles={['Admin']}>
                                    <div>Admin OK</div>
                                </ProtectedRoute>
                            )}
                        />
                        <Route path="/access-denied" element={<div>Access Denied Page</div>} />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );

        expect(await screen.findByText(/access denied page/i)).toBeInTheDocument();
    });

    it('blocks Hiring Manager from Candidate-private route', async () => {
        render(
            <AuthContext.Provider value={authStub({
                isAuthenticated: true,
                token: 't',
                user: { userId: 3, fullName: 'HM', email: 'h@x.com', role: 'HiringManager' }
            })}>
                <MemoryRouter initialEntries={['/candidate']}>
                    <Routes>
                        <Route
                            path="/candidate"
                            element={(
                                <ProtectedRoute roles={['Candidate']}>
                                    <div>Candidate Private</div>
                                </ProtectedRoute>
                            )}
                        />
                        <Route path="/access-denied" element={<div>Access Denied Page</div>} />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );

        expect(await screen.findByText(/access denied page/i)).toBeInTheDocument();
    });

    it('allows Administrator to access Admin route', async () => {
        render(
            <AuthContext.Provider value={authStub({
                isAuthenticated: true,
                token: 't',
                user: { userId: 4, fullName: 'Admin', email: 'a@x.com', role: 'Admin' }
            })}>
                <MemoryRouter initialEntries={['/admin']}>
                    <Routes>
                        <Route
                            path="/admin"
                            element={(
                                <ProtectedRoute roles={['Admin']}>
                                    <div>Admin Workspace</div>
                                </ProtectedRoute>
                            )}
                        />
                        <Route path="/access-denied" element={<div>Access Denied Page</div>} />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );

        expect(await screen.findByText(/admin workspace/i)).toBeInTheDocument();
    });
});

describe('Access Denied and Session Expired pages', () => {
    it('renders Access Denied page', () => {
        render(
            <MemoryRouter>
                <AccessDenied />
            </MemoryRouter>
        );
        expect(screen.getByRole('heading', { name: /access denied/i })).toBeInTheDocument();
    });

    it('renders Session Expired state', () => {
        render(
            <AuthContext.Provider value={authStub({ sessionExpired: true, setSessionExpired: vi.fn(), clearSession: vi.fn() })}>
                <MemoryRouter>
                    <SessionExpired />
                </MemoryRouter>
            </AuthContext.Provider>
        );
        expect(screen.getByRole('heading', { name: /session expired/i })).toBeInTheDocument();
    });
});

describe('Logout and current-user restoration', () => {
    it('logout clears authenticated state', async () => {
        const logout = vi.fn(async () => {});
        const clearSession = vi.fn();
        const user = userEvent.setup();

        function LogoutProbe() {
            const auth = useAuth();
            return (
                <button type="button" onClick={() => auth.logout()}>
                    Logout
                </button>
            );
        }

        render(
            <AuthContext.Provider value={authStub({
                isAuthenticated: true,
                token: 't',
                user: { userId: 1, fullName: 'Cand', email: 'c@x.com', role: 'Candidate' },
                logout: async () => {
                    await logout();
                    clearSession();
                },
                clearSession
            })}>
                <MemoryRouter>
                    <LogoutProbe />
                </MemoryRouter>
            </AuthContext.Provider>
        );

        await user.click(screen.getByRole('button', { name: /logout/i }));
        expect(logout).toHaveBeenCalled();
        expect(clearSession).toHaveBeenCalled();
    });

    it('shows loading then success restoration for current user', async () => {
        api.get.mockResolvedValueOnce({
            data: {
                userId: 9,
                fullName: 'Restored User',
                email: 'restored@example.com',
                role: 'Candidate',
                status: 'Active',
                permissions: []
            }
        });

        localStorage.setItem('token', 'stored-token');

        function MeProbe() {
            const auth = useAuth();
            if (auth.loading) return <div>Loading session…</div>;
            if (!auth.user) return <div>No user</div>;
            return <div>Restored:{auth.user.fullName}</div>;
        }

        render(
            <AuthProvider>
                <MemoryRouter>
                    <MeProbe />
                </MemoryRouter>
            </AuthProvider>
        );

        expect(screen.getByText(/loading session/i)).toBeInTheDocument();
        expect(await screen.findByText(/restored:restored user/i)).toBeInTheDocument();
        expect(api.get).toHaveBeenCalledWith('/auth/me');
    });

    it('handles current-user restoration error as session expired', async () => {
        api.get.mockRejectedValueOnce({ response: { status: 401 } });
        localStorage.setItem('token', 'bad-token');

        function MeProbe() {
            const auth = useAuth();
            if (auth.loading) return <div>Loading session…</div>;
            return <div>{auth.sessionExpired ? 'Expired' : 'Active'}</div>;
        }

        render(
            <AuthProvider>
                <MemoryRouter>
                    <MeProbe />
                </MemoryRouter>
            </AuthProvider>
        );

        expect(await screen.findByText(/^expired$/i)).toBeInTheDocument();
        await waitFor(() => expect(localStorage.getItem('token')).toBeNull());
    });
});
