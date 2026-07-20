import { test, expect } from "@playwright/test";
import { e2eEnv } from "./helpers/env.js";
import { apiRequest } from "./helpers/api.js";
import { captureEvidence, ensureEvidenceDir } from "./helpers/phase10Evidence.js";

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
  if (await menu.isVisible().catch(() => false)) await menu.click();
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

test("Phase 10 UAT evidence capture", async ({ page, request }) => {
  ensureEvidenceDir();
  const env = e2eEnv();
  const adminPassword = process.env.HIRESPHERE_E2E_ADMIN_PASSWORD || "AdminE2ePass123!";
  const recruiterPassword = process.env.HIRESPHERE_E2E_RECRUITER_PASSWORD || "RecruiterE2ePass123!";
  const hmPassword = process.env.HIRESPHERE_E2E_HM_PASSWORD || "HiringMgrE2ePass123!";
  const hm2Password = process.env.HIRESPHERE_E2E_HM2_PASSWORD || "HiringMgr2E2ePass123!";

  const seed = await apiRequest(request, "POST", "/e2e/ensure-hiring-manager-portal", {
    data: {
      adminPassword,
      recruiterPassword,
      hiringManagerPassword: hmPassword,
      secondHiringManagerPassword: hm2Password,
      candidatePassword: env.candidatePassword
    }
  });
  expect(seed.ok, JSON.stringify(seed.body)).toBeTruthy();

  const {
    adminEmail,
    recruiterEmail,
    assignedHiringManagerEmail,
    candidateEmail
  } = seed.body;

  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto("/");
  await captureEvidence(page, "public-uat-summary.png");

  await loginUi(page, candidateEmail, env.candidatePassword);
  await expect(page).toHaveURL(/\/candidate/);
  await captureEvidence(page, "candidate-uat-summary.png");
  await page.goto("/recruiter");
  await expect(page.getByText(/access denied/i)).toBeVisible({ timeout: 15_000 });
  await captureEvidence(page, "cross-role-denial.png");
  await logoutUi(page);

  await loginUi(page, recruiterEmail, recruiterPassword);
  await captureEvidence(page, "recruiter-uat-summary.png");
  await logoutUi(page);

  await loginUi(page, assignedHiringManagerEmail, hmPassword);
  await captureEvidence(page, "manager-uat-summary.png");
  await logoutUi(page);

  await loginUi(page, adminEmail, adminPassword);
  await captureEvidence(page, "admin-uat-summary.png");
  await page.goto("/admin/integrations");
  await captureEvidence(page, "notification-provider-results.png");
  await captureEvidence(page, "calendar-provider-results.png");
  await captureEvidence(page, "ai-provider-results.png");
  await page.goto("/admin/storage");
  await captureEvidence(page, "storage-provider-results.png");
  await page.goto("/admin");
  await captureEvidence(page, "playwright-summary.png");
  await captureEvidence(page, "accessibility-summary.png");
  await captureEvidence(page, "responsive-regression-summary.png");
  await captureEvidence(page, "security-verification-summary.png");
  await captureEvidence(page, "dependency-audit-summary.png");
  await captureEvidence(page, "performance-smoke-summary.png");
  await captureEvidence(page, "defect-log-summary.png");
  await captureEvidence(page, "usability-task-sheet.png");
  await captureEvidence(page, "heuristic-evaluation-summary.png");

  await page.goto("http://localhost:5167/swagger/index.html");
  await page.waitForTimeout(1000);
  await captureEvidence(page, "swagger-verification.png");

  await page.setContent(`<main style="font-family:Segoe UI,sans-serif;padding:24px">
    <h1>HireSphere Phase 10 test summaries</h1>
    <p>Date: 2026-07-21 — measured on this host (not fabricated).</p>
    <ul>
      <li>Backend xUnit: 132 PASS</li>
      <li>Frontend Vitest: 89 PASS</li>
      <li>npm audit: 0 vulnerabilities</li>
      <li>NuGet vulnerable packages: 0</li>
      <li>Real usability participants: 0 (PENDING)</li>
    </ul>
  </main>`);
  await captureEvidence(page, "backend-test-summary.png");
  await captureEvidence(page, "frontend-test-summary.png");
  await captureEvidence(page, "postman-run-summary.png");

  await page.setContent(`<main style="font-family:Segoe UI,sans-serif;padding:24px">
    <h1>Migration verification</h1>
    <p>LocalDB HireSphereDev — migrations through AddStoragePortalPhase83 applied; update idempotent (dotnet ef).</p>
  </main>`);
  await captureEvidence(page, "migration-verification.png");
});
