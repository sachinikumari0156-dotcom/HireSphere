export function authStub(overrides = {}) {
    return {
        user: null,
        token: null,
        loading: false,
        sessionExpired: false,
        isAuthenticated: false,
        roleHome: (role) => ({
            Candidate: '/candidate',
            Recruiter: '/recruiter',
            HiringManager: '/hiring-manager',
            Admin: '/admin'
        }[role] || '/'),
        login: async () => ({}),
        registerCandidate: async () => ({}),
        logout: async () => {},
        clearSession: () => {},
        setSessionExpired: () => {},
        ...overrides
    };
}
