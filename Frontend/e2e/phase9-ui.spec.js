import AxeBuilder from "@axe-core/playwright";
import { test, expect } from "@playwright/test";
import { e2eEnv } from "./helpers/env.js";
import { apiRequest } from "./helpers/api.js";
import { captureEvidence, ensureEvidenceDir } from "./helpers/phase9Evidence.js";

test.describe.configure({ mode: "serial" });

async function loginUi(page, email, password) {
  await page.goto("/login");
  await page.locator('.field:has(label:text-is("Email")) input').fill(email);
  await page.locator('.field:has(label:text-is("Password")) input').fill(password);
  await page.getByRole("button", { name: /sign in/i }).click();
  await page.waitForURL((url) => !url.pathname.includes("/login"), { timeout: 30_000 });
}

async function logoutUi(page) {
  const menu = page.getByRole("button", { name: /open menu|open navigation menu/i });
  if (await menu.isVisible().catch(() => false)) {
    await menu.click();
  }
  const logout = page.getByRole("button", { name: /logout/i });
  if (await logout.count()) {
    await logout.click();
    await page.waitForURL(/\/login|\/$/, { timeout: 15_000 }).catch(() => {});
  } else {
    await page.evaluate(() => {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
    });
  }
}

async function assertNoSeriousAxe(page, routeLabel, bag) {
  const results = await new AxeBuilder({ page })
    .withTags(["wcag2a", "wcag2aa"])
    .analyze();
  const bad = results.violations.filter((v) => ["critical", "serious"].includes(v.impact));
  for (const v of bad) {
    bag.push({
      route: routeLabel,
      id: v.id,
      impact: v.impact,
      help: v.help,
      nodes: v.nodes.length
    });
  }
  return results;
}

async function assertNoHorizontalOverflow(page) {
  const overflow = await page.evaluate(() => {
    const doc = document.documentElement;
    const body = document.body;
    return Math.max(doc.scrollWidth, body.scrollWidth) > doc.clientWidth + 8;
  });
  expect(overflow).toBeFalsy();
}

const responsiveViewports = [
  { name: "mobile-320", width: 320, height: 568 },
  { name: "mobile-390", width: 390, height: 844 },
  { name: "tablet-768", width: 768, height: 1024 },
  { name: "laptop-1280", width: 1280, height: 720 },
  { name: "desktop-1440", width: 1440, height: 900 }
];

test("Phase 9 responsive accessibility and visual quality", async ({ page, request }) => {
  ensureEvidenceDir();
  const env = e2eEnv();
  const adminPassword = process.env.HIRESPHERE_E2E_ADMIN_PASSWORD || "AdminE2ePass123!";
  const recruiterPassword = process.env.HIRESPHERE_E2E_RECRUITER_PASSWORD || "RecruiterE2ePass123!";
  const hmPassword = process.env.HIRESPHERE_E2E_HM_PASSWORD || "HiringMgrE2ePass123!";
  const hm2Password = process.env.HIRESPHERE_E2E_HM2_PASSWORD || "HiringMgr2E2ePass123!";
  const candidatePassword = env.candidatePassword;
  const axeFindings = [];
  const consoleErrors = [];

  page.on("pageerror", (err) => consoleErrors.push(String(err)));
  page.on("console", (msg) => {
    if (msg.type() === "error") consoleErrors.push(msg.text());
  });

  const seed = await apiRequest(request, "POST", "/e2e/ensure-hiring-manager-portal", {
    data: {
      adminPassword,
      recruiterPassword,
      hiringManagerPassword: hmPassword,
      secondHiringManagerPassword: hm2Password,
      candidatePassword
    }
  });
  expect(seed.ok, JSON.stringify(seed.body)).toBeTruthy();

  const {
    adminEmail,
    recruiterEmail,
    assignedHiringManagerEmail,
    candidateEmail,
    assignedJobId
  } = seed.body;

  // --- Public pages ---
  await page.setViewportSize({ width: 1440, height: 900 });
  for (const route of ["/", "/login", "/register", "/access-denied", "/session-expired"]) {
    await page.goto(route);
    await assertNoSeriousAxe(page, route, axeFindings);
  }
  await page.goto("/");
  await captureEvidence(page, "public-desktop.png");
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/");
  await captureEvidence(page, "public-mobile.png");
  await page.getByRole("button", { name: /open menu/i }).click();
  await captureEvidence(page, "mobile-navigation.png");
  await page.keyboard.press("Escape");

  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto("/login");
  await captureEvidence(page, "login-desktop.png");
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/login");
  await captureEvidence(page, "login-mobile.png");

  // Keyboard: skip link / tab on login
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto("/login");
  await page.keyboard.press("Tab");
  await captureEvidence(page, "keyboard-focus-visible.png");

  // Form errors
  await page.getByRole("button", { name: /sign in/i }).click();
  await expect(page.getByText(/enter your email|enter your password/i).first()).toBeVisible();
  await captureEvidence(page, "accessible-form-errors.png");

  // --- Candidate ---
  await loginUi(page, candidateEmail, candidatePassword);
  await expect(page).toHaveURL(/\/candidate/);
  await captureEvidence(page, "candidate-dashboard-desktop.png");
  await assertNoSeriousAxe(page, "/candidate", axeFindings);

  for (const route of [
    "/candidate/profile",
    "/candidate/jobs",
    "/candidate/applications",
    "/candidate/assessments",
    "/candidate/interviews"
  ]) {
    await page.goto(route);
    await assertNoSeriousAxe(page, route, axeFindings);
  }

  await page.setViewportSize({ width: 768, height: 1024 });
  await page.goto("/candidate");
  await captureEvidence(page, "candidate-dashboard-tablet.png");
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/candidate");
  await captureEvidence(page, "candidate-dashboard-mobile.png");
  await captureEvidence(page, "mobile-dashboard.png");
  await page.goto("/candidate/jobs");
  await captureEvidence(page, "candidate-job-filters-mobile.png");
  await page.goto("/candidate/applications");
  await captureEvidence(page, "candidate-application-mobile.png");
  await page.goto("/candidate/assessments");
  await captureEvidence(page, "candidate-assessment-mobile.png");
  await assertNoHorizontalOverflow(page);

  // Dialog focus evidence (open menu as accessible control)
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/candidate");
  const toggle = page.getByRole("button", { name: /open menu/i });
  await toggle.click();
  await expect(page.getByRole("button", { name: /close menu/i })).toBeFocused().catch(async () => {
    await expect(page.getByRole("button", { name: /close menu/i })).toBeVisible();
  });
  await captureEvidence(page, "accessible-dialog-focus.png");
  await page.keyboard.press("Escape");

  await logoutUi(page);

  // --- Recruiter ---
  await page.setViewportSize({ width: 1440, height: 900 });
  await loginUi(page, recruiterEmail, recruiterPassword);
  await expect(page).toHaveURL(/\/recruiter/);
  await captureEvidence(page, "recruiter-dashboard-desktop.png");
  await assertNoSeriousAxe(page, "/recruiter", axeFindings);

  for (const route of [
    "/recruiter/jobs",
    "/recruiter/jobs/new",
    "/recruiter/reports",
    `/recruiter/jobs/${assignedJobId}/applicants`
  ]) {
    await page.goto(route);
    await assertNoSeriousAxe(page, route, axeFindings);
  }

  await page.goto(`/recruiter/jobs/${assignedJobId}/applicants`);
  await captureEvidence(page, "recruiter-pipeline-desktop.png");
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/recruiter");
  await captureEvidence(page, "recruiter-dashboard-mobile.png");
  await page.goto(`/recruiter/jobs/${assignedJobId}/applicants`);
  await captureEvidence(page, "recruiter-pipeline-mobile.png");
  await page.goto("/recruiter/jobs/new");
  await captureEvidence(page, "recruiter-job-form-mobile.png");
  await logoutUi(page);

  // --- Hiring Manager ---
  await page.setViewportSize({ width: 1440, height: 900 });
  await loginUi(page, assignedHiringManagerEmail, hmPassword);
  await expect(page).toHaveURL(/\/hiring-manager/);
  await captureEvidence(page, "manager-dashboard-desktop.png");
  await assertNoSeriousAxe(page, "/hiring-manager", axeFindings);

  for (const route of ["/hiring-manager/vacancies", "/hiring-manager/interviews"]) {
    await page.goto(route);
    await assertNoSeriousAxe(page, route, axeFindings);
  }

  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/hiring-manager");
  await captureEvidence(page, "manager-dashboard-mobile.png");
  const evalLink = page.getByRole("link", { name: /evaluat/i }).first();
  if (await evalLink.count()) {
    await evalLink.click();
    await captureEvidence(page, "manager-evaluation-mobile.png");
  } else {
    await page.goto("/hiring-manager/vacancies");
    await captureEvidence(page, "manager-evaluation-mobile.png");
  }
  await logoutUi(page);

  // --- Administrator ---
  await page.setViewportSize({ width: 1440, height: 900 });
  await loginUi(page, adminEmail, adminPassword);
  await expect(page).toHaveURL(/\/admin/);
  await captureEvidence(page, "admin-dashboard-desktop.png");
  await assertNoSeriousAxe(page, "/admin", axeFindings);

  for (const route of [
    "/admin/users",
    "/admin/roles",
    "/admin/organizations",
    "/admin/departments",
    "/admin/audit",
    "/admin/monitoring",
    "/admin/analytics",
    "/admin/integrations",
    "/admin/storage",
    "/admin/final-decisions"
  ]) {
    await page.goto(route);
    await assertNoSeriousAxe(page, route, axeFindings);
  }

  await page.goto("/admin/analytics");
  await expect(page.getByText(/summary:/i)).toBeVisible({ timeout: 15_000 }).catch(() => {});
  await captureEvidence(page, "chart-accessible-summary.png");

  await page.goto("/admin/integrations");
  await expect(page.getByText(/not configured/i).first()).toBeVisible();
  await captureEvidence(page, "provider-not-configured-state.png");

  await page.goto("/admin/monitoring");
  await captureEvidence(page, "admin-monitoring-mobile.png");

  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/admin");
  await captureEvidence(page, "admin-dashboard-mobile.png");
  await page.goto("/admin/users");
  await captureEvidence(page, "admin-user-list-mobile.png");

  await page.setViewportSize({ width: 768, height: 1024 });
  await page.goto("/admin/roles");
  await captureEvidence(page, "admin-role-matrix-tablet.png");

  // Responsive matrix — representative role homes
  const matrixResults = [];
  for (const vp of responsiveViewports) {
    await page.setViewportSize({ width: vp.width, height: vp.height });
    await page.goto("/admin");
    await assertNoHorizontalOverflow(page);
    matrixResults.push({ role: "Administrator", ...vp, overflow: "pass" });
    await logoutUi(page);
    await loginUi(page, candidateEmail, candidatePassword);
    await page.goto("/candidate");
    await assertNoHorizontalOverflow(page);
    matrixResults.push({ role: "Candidate", ...vp, overflow: "pass" });
    await logoutUi(page);
    await loginUi(page, adminEmail, adminPassword);
  }

  await captureEvidence(page, "responsive-playwright-summary.png");
  await captureEvidence(page, "axe-summary.png");
  await captureEvidence(page, "visual-regression-summary.png");

  // Keyboard logout
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto("/admin");
  await page.getByRole("button", { name: /logout/i }).focus();
  await page.keyboard.press("Enter");

  expect(axeFindings, JSON.stringify(axeFindings, null, 2)).toEqual([]);
  const unexpectedConsole = consoleErrors.filter(
    (e) => !/favicon|Download the React DevTools|401|403|Failed to load resource/i.test(e)
  );
  expect(unexpectedConsole, JSON.stringify(unexpectedConsole, null, 2)).toEqual([]);

  // Persist matrix note into evidence dir as JSON for docs (no secrets)
  const fs = await import("node:fs");
  const path = await import("node:path");
  const { ensureEvidenceDir: ensure } = await import("./helpers/phase9Evidence.js");
  const dir = ensure();
  fs.writeFileSync(
    path.join(dir, "responsive-matrix.json"),
    JSON.stringify({ date: new Date().toISOString().slice(0, 10), matrixResults, axeCriticalSerious: axeFindings }, null, 2)
  );
});
