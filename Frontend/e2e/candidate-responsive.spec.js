import { test, expect } from "@playwright/test";
import { e2eEnv } from "./helpers/env.js";
import { registerCandidate } from "./helpers/api.js";
import { captureEvidence } from "./helpers/evidence.js";

const viewports = [
  { name: "desktop", width: 1440, height: 900 },
  { name: "tablet", width: 768, height: 1024 },
  { name: "mobile", width: 390, height: 844 }
];

async function assertNoHorizontalOverflow(page) {
  const overflow = await page.evaluate(() => {
    const doc = document.documentElement;
    const body = document.body;
    return {
      scrollWidth: Math.max(doc.scrollWidth, body.scrollWidth),
      clientWidth: doc.clientWidth,
      overflow: Math.max(doc.scrollWidth, body.scrollWidth) > doc.clientWidth + 8
    };
  });
  expect(overflow.overflow, JSON.stringify(overflow)).toBeFalsy();
}

test.describe("Candidate responsive layouts", () => {
  for (const vp of viewports) {
    test(`dashboard usable at ${vp.name} ${vp.width}x${vp.height}`, async ({ page, request }) => {
      const env = e2eEnv();
      const email = env.uniqueEmail();
      const password = env.candidatePassword;
      const reg = await registerCandidate(request, { email, password });
      expect(reg.ok).toBeTruthy();

      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.goto("/login");
      await page.locator('.field:has(label:text-is("Email")) input').fill(email);
      await page.locator('.field:has(label:text-is("Password")) input').fill(password);
      await page.getByRole("button", { name: /sign in/i }).click();
      await expect(page).toHaveURL(/\/candidate/, { timeout: 30_000 });

      await expect(page.getByRole("heading", { name: /Candidate dashboard/i })).toBeVisible();
      await expect(page.locator(".navbar")).toBeVisible();
      await expect(page.getByRole("link", { name: /Profile|Browse jobs|Applications/i }).first()).toBeVisible();
      await assertNoHorizontalOverflow(page);

      await page.goto("/candidate/profile");
      await expect(page.getByRole("button", { name: /save profile/i })).toBeVisible();
      await assertNoHorizontalOverflow(page);

      if (vp.name === "desktop") {
        await captureEvidence(page, "playwright-summary.png");
      }
    });
  }
});
