import React from 'react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import NotificationPreferencesPage from '../pages/NotificationPreferencesPage';
import AdminIntegrationsPage from '../pages/admin/AdminIntegrationsPage';
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

describe('Phase 8.2 integrations UI', () => {
    beforeEach(() => vi.clearAllMocks());

    it('renders preference toggles and consent validation', async () => {
        api.get
            .mockResolvedValueOnce({
                data: {
                    emailEnabled: true,
                    smsEnabled: true,
                    interviewReminders: true,
                    applicationUpdates: true,
                    assessmentReminders: true,
                    smsConsent: false
                }
            })
            .mockResolvedValueOnce({ data: [] });

        render(
            <AuthContext.Provider value={authStub({
                user: { fullName: 'Cand', role: 'Candidate', email: 'c@example.com', userId: 1 },
                token: 'tok',
                isAuthenticated: true
            })}>
                <MemoryRouter initialEntries={['/notification-preferences']}>
                    <Routes>
                        <Route path="/notification-preferences" element={<NotificationPreferencesPage />} />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );

        expect(await screen.findByRole('heading', { name: /notification preferences/i })).toBeInTheDocument();
        expect(screen.getByText(/consent is required before sms/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/email notifications/i)).toBeInTheDocument();
    });

    it('shows provider Not Configured on admin integrations dashboard', async () => {
        api.get
            .mockResolvedValueOnce({
                data: [
                    { name: 'SMTP Email', status: 'NotConfigured', detail: 'Production SMTP Not Configured.' },
                    { name: 'Google Calendar', status: 'NotConfigured', detail: 'OAuth credentials not configured.' },
                    { name: 'Outlook Calendar', status: 'NotConfigured', detail: 'OAuth credentials not configured.' }
                ]
            })
            .mockResolvedValueOnce({
                data: [{ id: 3, notificationType: 'Test', channel: 'Email', status: 'Failed', safeFailureCode: 'smtp_not_configured' }]
            });

        render(
            <AuthContext.Provider value={authStub({
                user: { fullName: 'Admin', role: 'Admin', email: 'a@example.com', userId: 2 },
                token: 'tok',
                isAuthenticated: true
            })}>
                <MemoryRouter initialEntries={['/admin/integrations']}>
                    <Routes>
                        <Route path="/admin/integrations" element={<AdminIntegrationsPage />} />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );

        expect(await screen.findByRole('heading', { name: /integration providers/i })).toBeInTheDocument();
        expect(screen.getAllByText(/NotConfigured/i).length).toBeGreaterThan(0);
        expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument();
        fireEvent.click(screen.getByRole('button', { name: /retry/i }));
        expect(api.post).toHaveBeenCalled();
    });
});
