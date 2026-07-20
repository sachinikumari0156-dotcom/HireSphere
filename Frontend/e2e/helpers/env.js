export function e2eEnv() {
  const password =
    process.env.HIRESPHERE_E2E_CANDIDATE_PASSWORD || "CandidateE2ePass123!";

  return {
    frontendUrl: process.env.HIRESPHERE_E2E_FRONTEND_URL || "http://localhost:5173",
    apiUrl: process.env.HIRESPHERE_E2E_API_URL || "http://localhost:5167",
    apiBase: `${process.env.HIRESPHERE_E2E_API_URL || "http://localhost:5167"}/api`,
    candidatePassword: password,
    uniqueEmail: () => `candidate-e2e-${Date.now()}@example.test`
  };
}
