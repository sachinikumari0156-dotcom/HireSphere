import React from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { AuthContext } from "../auth/auth-context";
import { authStub } from "./authStub";
import AccessDenied from "../pages/AccessDenied";
import SessionExpired from "../pages/SessionExpired";
import ProtectedRoute from "../components/ProtectedRoute";
import AdminIntegrationsPage from "../pages/admin/AdminIntegrationsPage";
import { ErrorState } from "../components/ui/primitives";

vi.mock("../api/axios", () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn()
  }
}));

import api from "../api/axios";

describe("Phase 10.1 resilience and provider states", () => {
  beforeEach(() => vi.clearAllMocks());

  it("shows Access Denied for unauthorized role", () => {
    render(
      <AuthContext.Provider
        value={authStub({
          user: { fullName: "Cand", role: "Candidate", email: "c@example.com", userId: 1 },
          token: "t",
          isAuthenticated: true
        })}
      >
        <MemoryRouter initialEntries={["/admin"]}>
          <Routes>
            <Route
              path="/admin"
              element={
                <ProtectedRoute roles={["Admin"]}>
                  <div>Admin secret</div>
                </ProtectedRoute>
              }
            />
            <Route path="/access-denied" element={<AccessDenied />} />
          </Routes>
        </MemoryRouter>
      </AuthContext.Provider>
    );
    expect(screen.getByRole("heading", { name: /access denied/i })).toBeInTheDocument();
    expect(screen.queryByText(/admin secret/i)).not.toBeInTheDocument();
  });

  it("renders session expired recovery action", () => {
    render(
      <AuthContext.Provider value={authStub({ sessionExpired: true, clearSession: vi.fn(), setSessionExpired: vi.fn() })}>
        <MemoryRouter>
          <SessionExpired />
        </MemoryRouter>
      </AuthContext.Provider>
    );
    expect(screen.getByRole("heading", { name: /session expired/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /sign in/i })).toBeInTheDocument();
  });

  it("ErrorState offers retry recovery", async () => {
    const onRetry = vi.fn();
    render(
      <ErrorState title="Backend unavailable" onRetry={onRetry}>
        <p>Could not reach the API.</p>
      </ErrorState>
    );
    expect(screen.getByRole("alert")).toBeInTheDocument();
    screen.getByRole("button", { name: /try again/i }).click();
    expect(onRetry).toHaveBeenCalled();
  });

  it("provider Not Configured is shown for integrations failures", async () => {
    api.get
      .mockResolvedValueOnce({
        data: [{ name: "Production SMTP", status: "NotConfigured", detail: "No credentials" }]
      })
      .mockResolvedValueOnce({ data: [] });

    render(
      <MemoryRouter>
        <AdminIntegrationsPage />
      </MemoryRouter>
    );

    expect(await screen.findByText(/provider not configured/i)).toBeInTheDocument();
    expect(screen.getAllByText(/not configured/i).length).toBeGreaterThan(0);
  });

  it("handles integrations API 500 with error message", async () => {
    api.get.mockRejectedValue({ response: { data: { message: "Temporary server error." }, status: 500 } });
    render(
      <MemoryRouter>
        <AdminIntegrationsPage />
      </MemoryRouter>
    );
    expect(await screen.findByText(/temporary server error|could not load integrations/i)).toBeInTheDocument();
  });
});
