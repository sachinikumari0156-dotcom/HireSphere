import { defineConfig, devices } from "@playwright/test";

const frontendUrl = process.env.HIRESPHERE_E2E_FRONTEND_URL || "http://localhost:5173";
const apiUrl = process.env.HIRESPHERE_E2E_API_URL || "http://localhost:5167";

export default defineConfig({
  testDir: "./e2e",
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: 0,
  workers: 1,
  timeout: 180_000,
  expect: { timeout: 20_000 },
  reporter: [
    ["list"],
    ["html", { open: "never", outputFolder: "playwright-report" }]
  ],
  use: {
    baseURL: frontendUrl,
    trace: "on-first-retry",
    screenshot: "only-on-failure",
    video: "off",
    actionTimeout: 20_000
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"], viewport: { width: 1440, height: 900 } }
    }
  ],
  metadata: {
    frontendUrl,
    apiUrl
  }
});
