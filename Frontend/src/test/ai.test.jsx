import React from 'react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import CandidateResumeAnalysisPage from '../pages/candidate/CandidateResumeAnalysisPage';
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

describe('Candidate resume analysis AI page', () => {
    beforeEach(() => vi.clearAllMocks());

    it('shows NotConfigured external AI and parse control', async () => {
        api.get
            .mockResolvedValueOnce({ data: null })
            .mockResolvedValueOnce({
                data: {
                    allowExternalAiProcessing: false,
                    deterministicProviderStatus: 'Healthy',
                    externalAiProviderStatus: 'NotConfigured',
                    humanReviewNotice: 'AI-generated insight. Final recruitment decisions must be reviewed by authorized users.'
                }
            });
        render(
            <AuthContext.Provider value={authStub({
                user: { fullName: 'Cand', role: 'Candidate', email: 'c@example.com', userId: 1 },
                token: 'tok',
                isAuthenticated: true
            })}>
                <MemoryRouter initialEntries={['/candidate/resumes/9/analysis']}>
                    <Routes>
                        <Route path="/candidate/resumes/:id/analysis" element={<CandidateResumeAnalysisPage />} />
                    </Routes>
                </MemoryRouter>
            </AuthContext.Provider>
        );
        expect(await screen.findByRole('heading', { name: /resume analysis/i })).toBeInTheDocument();
        expect(screen.getByText(/external ai: notconfigured/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /parse resume/i })).toBeInTheDocument();
        expect(screen.getByText(/ai-generated insight/i)).toBeInTheDocument();
    });
});
