import { test, expect } from "@playwright/test";
import { e2eEnv } from "./helpers/env.js";
import { apiRequest } from "./helpers/api.js";
import { ensureEvidenceDir } from "./helpers/phase9Evidence.js";

/**
 * Visual regression foundation — stable viewports, small threshold.
 * Update intentionally with: npx playwright test e2e/phase9-visual.spec.js --update-snapshots
 */
test.describe("Phase 9 visual regression", () => {
  test("public landing and login snapshots", async ({ page }) => {
    ensureEvidenceDir();
    await page.setViewportSize({ width: 1280, height: 720 });
    await page.goto("/");
    await expect(page.locator(".home-hero")).toHaveScreenshot("public-landing.png", {
      maxDiffPixelRatio: 0.02
    });
    await page.goto("/login");
    await expect(page.locator(".reg-card").first()).toHaveScreenshot("login-card.png", {
      maxDiffPixelRatio: 0.02
    });
  });

  test("role dashboards snapshots", async ({ page, request }) => {
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

    async function login(email, password) {
      await page.goto("/login");
      await page.locator('.field:has(label:text-is("Email")) input').fill(email);
      await page.locator('.field:has(label:text-is("Password")) input').fill(password);
      await page.getByRole("button", { name: /sign in/i }).click();
      await page.waitForURL((url) => !url.pathname.includes("/login"), { timeout: 30_000 });
    }

    await page.setViewportSize({ width: 1280, height: 720 });

    await login(seed.body.candidateEmail, env.candidatePassword);
    await page.goto("/candidate");
    await expect(page.locator("main, .dash-page, .hs-shell__body").first()).toHaveScreenshot(
      "candidate-dashboard.png",
      { maxDiffPixelRatio: 0.03 }
    );

    await page.evaluate(() => {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
    });
    await login(seed.body.recruiterEmail, recruiterPassword);
    await page.goto("/recruiter");
    await expect(page.locator("main, .rec-page, .hs-shell__body").first()).toHaveScreenshot(
      "recruiter-dashboard.png",
      { maxDiffPixelRatio: 0.03 }
    );

    await page.evaluate(() => {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
    });
    await login(seed.body.assignedHiringManagerEmail, hmPassword);
    await page.goto("/hiring-manager");
    await expect(page.locator("main, .hm-page, .hs-shell__body").first()).toHaveScreenshot(
      "manager-dashboard.png",
      { maxDiffPixelRatio: 0.03 }
    );

    await page.evaluate(() => {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
    });
    await login(seed.body.adminEmail, adminPassword);
    await page.goto("/admin");
    await expect(page.locator("main, .admin-page, .hs-shell__body").first()).toHaveScreenshot(
      "admin-dashboard.png",
      { maxDiffPixelRatio: 0.03 }
    );
  });
});
