import AxeBuilder from "@axe-core/playwright";
import { test, expect } from "@playwright/test";
import { e2eEnv } from "./helpers/env.js";
import { registerCandidate } from "./helpers/api.js";

test.describe("Candidate accessibility", () => {
  test("critical pages have no serious/critical axe violations", async ({ page, request }) => {
    const env = e2eEnv();
    const email = env.uniqueEmail();
    const password = env.candidatePassword;
    const reg = await registerCandidate(request, { email, password });
    expect(reg.ok).toBeTruthy();

    await page.goto("/login");
    await page.locator('.field:has(label:text-is("Email")) input').fill(email);
    await page.locator('.field:has(label:text-is("Password")) input').fill(password);
    await page.getByRole("button", { name: /sign in/i }).click();
    await expect(page).toHaveURL(/\/candidate/, { timeout: 30_000 });

    const serious = [];
    const authenticatedPages = ["/candidate", "/candidate/profile", "/candidate/jobs", "/access-denied"];

    for (const route of authenticatedPages) {
      await page.goto(route);
      await page.keyboard.press("Tab");
      const focused = await page.evaluate(() => document.activeElement?.tagName || null);
      expect(focused).toBeTruthy();

      const results = await new AxeBuilder({ page })
        .withTags(["wcag2a", "wcag2aa"])
        .analyze();

      const bad = results.violations.filter((v) =>
        ["critical", "serious"].includes(v.impact)
      );
      for (const v of bad) {
        serious.push({ route, id: v.id, impact: v.impact, help: v.help, nodes: v.nodes.length });
      }
    }

    await page.evaluate(() => {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
    });

    for (const route of ["/register", "/login"]) {
      await page.goto(route);
      await page.keyboard.press("Tab");
      const results = await new AxeBuilder({ page })
        .withTags(["wcag2a", "wcag2aa"])
        .analyze();
      const bad = results.violations.filter((v) =>
        ["critical", "serious"].includes(v.impact)
      );
      for (const v of bad) {
        serious.push({ route, id: v.id, impact: v.impact, help: v.help, nodes: v.nodes.length });
      }
    }

    // Do not suppress color-contrast or other serious/critical rules.
    expect(serious, JSON.stringify(serious, null, 2)).toEqual([]);
  });
});
