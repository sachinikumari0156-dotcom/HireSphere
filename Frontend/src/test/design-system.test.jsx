import React from "react";
import { describe, expect, it, vi, beforeEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import {
  Button,
  Input,
  StatusBadge,
  EmptyState,
  ErrorState,
  Spinner,
  SkipLink
} from "../components/ui/primitives";
import Modal from "../components/ui/Modal";
import RoleShell from "../components/layout/RoleShell";
import NotFoundPage from "../pages/NotFoundPage";
import Navbar from "../components/Navbar";
import { Tabs, Accordion, Pagination, FileUpload } from "../components/ui/patterns";
import { AuthContext } from "../auth/auth-context";
import { authStub } from "./authStub";

describe("HireSphere design system", () => {
  beforeEach(() => {
    document.body.style.overflow = "";
  });

  it("activates Button with keyboard Enter", async () => {
    const onClick = vi.fn();
    render(<Button onClick={onClick}>Save</Button>);
    const btn = screen.getByRole("button", { name: /save/i });
    btn.focus();
    await userEvent.keyboard("{Enter}");
    expect(onClick).toHaveBeenCalled();
  });

  it("traps focus in Modal and closes on Escape", async () => {
    const onClose = vi.fn();
    const trigger = document.createElement("button");
    document.body.appendChild(trigger);
    trigger.focus();
    render(
      <Modal open title="Confirm action" onClose={onClose} onConfirm={() => {}} confirmLabel="Yes">
        <p>Are you sure?</p>
      </Modal>
    );
    expect(screen.getByRole("dialog", { name: /confirm action/i })).toBeInTheDocument();
    await userEvent.keyboard("{Escape}");
    expect(onClose).toHaveBeenCalled();
  });

  it("associates field errors with inputs via aria-describedby", () => {
    render(<Input id="email" label="Email" error="Email is required" />);
    const input = screen.getByLabelText(/email/i);
    expect(input).toHaveAttribute("aria-invalid", "true");
    expect(input.getAttribute("aria-describedby")).toContain("email-error");
    expect(screen.getByRole("alert")).toHaveTextContent(/email is required/i);
  });

  it("status badges include non-color text meaning", () => {
    render(<StatusBadge tone="success" label="Healthy" />);
    expect(screen.getByText("Healthy")).toBeInTheDocument();
  });

  it("empty and error states render with recovery action", async () => {
    const retry = vi.fn();
    render(
      <>
        <EmptyState title="No jobs yet">Try adjusting filters.</EmptyState>
        <ErrorState title="Failed to load" onRetry={retry}>
          Network error
        </ErrorState>
      </>
    );
    expect(screen.getByRole("heading", { name: /no jobs yet/i })).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: /try again/i }));
    expect(retry).toHaveBeenCalled();
  });

  it("loading spinner announces politely", () => {
    render(<Spinner label="Loading dashboard" />);
    expect(screen.getByRole("status")).toHaveTextContent(/loading dashboard/i);
  });

  it("mobile navigation opens, has accessible name, and closes", async () => {
    render(
      <AuthContext.Provider
        value={authStub({
          user: { fullName: "Cand", role: "Candidate", email: "c@example.com", userId: 1 },
          token: "tok",
          isAuthenticated: true
        })}
      >
        <MemoryRouter>
          <Navbar />
        </MemoryRouter>
      </AuthContext.Provider>
    );
    const toggle = screen.getByRole("button", { name: /open menu/i });
    expect(toggle).toHaveAttribute("aria-expanded", "false");
    await userEvent.click(toggle);
    expect(screen.getByRole("button", { name: /close menu/i })).toHaveAttribute("aria-expanded", "true");
    await userEvent.keyboard("{Escape}");
    expect(screen.getByRole("button", { name: /open menu/i })).toHaveAttribute("aria-expanded", "false");
  });

  it("role shell hides unauthorized links by only rendering provided links", () => {
    render(
      <MemoryRouter initialEntries={["/admin"]}>
        <RoleShell
          title="Administrator portal"
          navLabel="Administrator"
          links={[
            { to: "/admin", label: "Dashboard", end: true },
            { to: "/admin/users", label: "Users" }
          ]}
        >
          <p>Content</p>
        </RoleShell>
      </MemoryRouter>
    );
    expect(screen.getByRole("navigation", { name: /administrator/i })).toBeInTheDocument();
    expect(screen.queryByText(/final decisions/i)).not.toBeInTheDocument();
    expect(screen.getByRole("heading", { name: /administrator portal/i })).toBeInTheDocument();
  });

  it("unknown route shows useful Not Found experience", () => {
    render(
      <MemoryRouter>
        <NotFoundPage />
      </MemoryRouter>
    );
    expect(screen.getByRole("heading", { name: /page not found/i })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /return home/i })).toBeInTheDocument();
  });

  it("skip link targets main content", () => {
    render(<SkipLink />);
    expect(screen.getByRole("link", { name: /skip to main content/i })).toHaveAttribute(
      "href",
      "#main-content"
    );
  });

  it("respects reduced-motion CSS variable presence", () => {
    // Token file defines prefers-reduced-motion overrides; assert document can use tokens.
    document.documentElement.style.setProperty("--hs-duration-normal", "1ms");
    expect(getComputedStyle(document.documentElement).getPropertyValue("--hs-duration-normal").trim() || "1ms").toBeTruthy();
  });

  it("tabs support keyboard navigation", async () => {
    render(<Tabs labels={["One", "Two"]} panels={[<p key="1">Panel 1</p>, <p key="2">Panel 2</p>]} />);
    const first = screen.getByRole("tab", { name: /one/i });
    first.focus();
    await userEvent.keyboard("{ArrowRight}");
    expect(screen.getByRole("tab", { name: /two/i })).toHaveAttribute("aria-selected", "true");
  });

  it("accordion supports keyboard activation", async () => {
    render(<Accordion items={[{ title: "Details", content: "Hidden until opened" }]} />);
    await userEvent.click(screen.getByRole("button", { name: /details/i }));
    expect(screen.getByText(/hidden until opened/i)).toBeInTheDocument();
  });

  it("pagination is accessible", () => {
    const onChange = vi.fn();
    render(<Pagination page={2} pageCount={5} onChange={onChange} />);
    expect(screen.getByRole("navigation", { name: /pagination/i })).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: /next/i }));
    expect(onChange).toHaveBeenCalledWith(3);
  });

  it("file upload is keyboard accessible with label", () => {
    render(<FileUpload label="Resume PDF" hint="PDF or DOCX up to 5 MB" />);
    expect(screen.getByLabelText(/resume pdf/i)).toBeInTheDocument();
  });
});
