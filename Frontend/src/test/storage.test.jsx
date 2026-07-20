import React from 'react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import AdminStoragePage from '../pages/admin/AdminStoragePage';
import { AuthContext } from '../auth/auth-context';
import { authStub } from './authStub';

vi.mock('../api/axios', () => ({
    default: {
        get: vi.fn(),
        post: vi.fn()
    }
}));

import api from '../api/axios';

describe('Phase 8.3 storage UI', () => {
    beforeEach(() => vi.clearAllMocks());

    it('shows storage provider status and antivirus NotConfigured', async () => {
        api.get.mockResolvedValueOnce({
            data: [
                { name: 'Local development storage', status: 'Healthy', detail: 'Private App_Data' },
                { name: 'Azure Blob cloud', status: 'NotConfigured', detail: 'No credentials' },
                { name: 'Antivirus', status: 'NotConfigured', detail: 'No scanner', quarantinedDocumentCount: 0 }
            ]
        });

        render(
            <AuthContext.Provider value={authStub({
                user: { fullName: 'Admin', role: 'Admin', email: 'a@example.com', userId: 2 },
                token: 'tok',
                isAuthenticated: true
            })}>
                <MemoryRouter initialEntries={['/admin/storage']}>
                    <Routes>
                        <Route path="/admin/storage" element={<AdminStoragePage />} />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );

        expect(await screen.findByRole('heading', { name: /storage providers/i })).toBeInTheDocument();
        expect(screen.getAllByText(/NotConfigured/i).length).toBeGreaterThan(0);
        fireEvent.click(screen.getByRole('button', { name: /dry-run/i }));
        expect(api.post).toHaveBeenCalledWith('/admin/storage/migrations/dry-run');
    });
});
