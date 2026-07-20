import React from 'react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import CandidateHome from '../pages/candidate/CandidateHome';
import CandidateProfilePage from '../pages/candidate/CandidateProfilePage';
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

function renderWithAuth(ui, authOverrides = {}) {
    return render(
        <AuthContext.Provider value={authStub({
            user: { fullName: 'Test Candidate', role: 'Candidate', email: 'c@example.com' },
            token: 'tok',
            isAuthenticated: true,
            ...authOverrides
        })}>
            <MemoryRouter>{ui}</MemoryRouter>
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
