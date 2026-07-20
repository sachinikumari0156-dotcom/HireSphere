import React from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, within } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import userEvent from "@testing-library/user-event";
import RoleShell from "../components/layout/RoleShell";
import Navbar from "../components/Navbar";
import FilterDrawer from "../components/ui/FilterDrawer";
import AdminAnalyticsPage from "../pages/admin/AdminAnalyticsPage";
import AdminIntegrationsPage from "../pages/admin/AdminIntegrationsPage";
import AdminUsersPage from "../pages/admin/AdminUsersPage";
import { AuthContext } from "../auth/auth-context";
import { authStub } from "./authStub";
import { friendlyStatus } from "../utils/statusLabels";

vi.mock("../api/axios", () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn()
  }
}));

import api from "../api/axios";

describe("Phase 9.2 responsive portal UX", () => {
  beforeEach(() => vi.clearAllMocks());

  it("public navigation exposes an accessible menu control", () => {
    render(
      <AuthContext.Provider value={authStub({ isAuthenticated: false })}>
        <MemoryRouter>
          <Navbar />
        </MemoryRouter>
      </AuthContext.Provider>
    );
    expect(screen.getByRole("button", { name: /open menu/i })).toBeInTheDocument();
  });

  it("candidate, recruiter, hiring manager and administrator shells expose labelled navigation", () => {
    const roles = [
      { title: "Candidate portal", label: "Candidate", links: [{ to: "/candidate", label: "Dashboard", end: true }] },
      { title: "Recruiter portal", label: "Recruiter", links: [{ to: "/recruiter", label: "Dashboard", end: true }] },
      { title: "Hiring Manager portal", label: "Hiring Manager", links: [{ to: "/hiring-manager", label: "Dashboard", end: true }] },
      { title: "Administrator portal", label: "Administrator", links: [{ to: "/admin", label: "Dashboard", end: true }] }
    ];
    for (const role of roles) {
      const { unmount } = render(
        <MemoryRouter>
          <RoleShell title={role.title} navLabel={role.label} links={role.links}>
            <p>Body</p>
          </RoleShell>
        </MemoryRouter>
      );
      expect(screen.getByRole("navigation", { name: new RegExp(role.label, "i") })).toBeInTheDocument();
      expect(screen.getByRole("button", { name: /open navigation menu/i })).toBeInTheDocument();
      unmount();
    }
  });

  it("mobile filter drawer toggles open and closed", async () => {
    const user = userEvent.setup();
    render(
      <FilterDrawer title="job filters">
        <label>
          Keyword
          <input aria-label="Keyword" />
        </label>
      </FilterDrawer>
    );
    const toggle = screen.getByRole("button", { name: /show job filters/i });
    expect(toggle).toHaveAttribute("aria-expanded", "false");
    await user.click(toggle);
    expect(screen.getByRole("button", { name: /hide job filters/i })).toHaveAttribute("aria-expanded", "true");
    expect(screen.getByLabelText(/keyword/i)).toBeInTheDocument();
  });

  it("admin users list includes filter drawer and responsive table cells", async () => {
    api.get.mockResolvedValueOnce({
      data: {
        items: [
          {
            userId: 9,
            fullName: "Sam Candidate",
            email: "sam@example.com",
            role: "Candidate",
            status: "Active"
          }
        ],
        totalCount: 1
      }
    });
    render(
      <AuthContext.Provider
        value={authStub({
          user: { fullName: "Admin", role: "Admin", email: "a@example.com", userId: 1 },
          token: "t",
          isAuthenticated: true
        })}
      >
        <MemoryRouter>
          <AdminUsersPage />
        </MemoryRouter>
      </AuthContext.Provider>
    );
    expect(await screen.findByRole("table")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /show user filters/i })).toBeInTheDocument();
    expect(screen.getByText(/sam candidate/i)).toBeInTheDocument();
    expect(document.querySelector('td[data-label="Name"]')).toBeTruthy();
  });

  it("analytics renders accessible summary and table alternative", async () => {
    api.get
      .mockResolvedValueOnce({
        data: {
          shortlisted: 2,
          rejected: 1,
          hired: 1,
          unavailableMetricsNote: "Descriptive only",
          applicationsByStatus: [{ name: "Submitted", count: 3 }]
        }
      })
      .mockResolvedValueOnce({ data: { skillDemandFromJobs: [] } })
      .mockResolvedValueOnce({ data: { jobsByDepartment: [] } });

    render(<AdminAnalyticsPage />);

    expect(await screen.findByText(/summary:/i)).toBeInTheDocument();
    expect(screen.getByRole("table")).toBeInTheDocument();
    expect(within(screen.getByRole("table")).getByText(/submitted/i)).toBeInTheDocument();
  });

  it("provider Not Configured presentation is visible", async () => {
    api.get
      .mockResolvedValueOnce({
        data: [{ name: "Google Calendar", status: "NotConfigured", detail: "OAuth missing" }]
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

  it("status labels are user friendly", () => {
    expect(friendlyStatus("UnderReview")).toBe("Under review");
    expect(friendlyStatus("InterviewScheduled")).toBe("Interview scheduled");
    expect(friendlyStatus("NotConfigured")).toBe("Not configured");
    expect(friendlyStatus("HiringManager")).toBe("Hiring Manager");
  });

  it("candidate navbar does not show administrator links", async () => {
    const user = userEvent.setup();
    render(
      <AuthContext.Provider
        value={authStub({
          user: { fullName: "Cand", role: "Candidate", email: "c@example.com", userId: 2 },
          token: "t",
          isAuthenticated: true
        })}
      >
        <MemoryRouter>
          <Navbar />
        </MemoryRouter>
      </AuthContext.Provider>
    );
    await user.click(screen.getByRole("button", { name: /open menu/i }));
    expect(screen.queryByRole("link", { name: /^users$/i })).not.toBeInTheDocument();
    expect(screen.queryByRole("link", { name: /audit/i })).not.toBeInTheDocument();
    expect(screen.getByRole("link", { name: /my profile/i })).toBeInTheDocument();
  });

  it("role shell menu toggles with accessible name", async () => {
    const user = userEvent.setup();
    render(
      <MemoryRouter>
        <RoleShell title="Recruiter portal" navLabel="Recruiter" links={[{ to: "/recruiter", label: "Jobs" }]}>
          <p>Content</p>
        </RoleShell>
      </MemoryRouter>
    );
    await user.click(screen.getByRole("button", { name: /open navigation menu/i }));
    expect(screen.getByRole("button", { name: /close navigation menu/i })).toBeInTheDocument();
  });
});
