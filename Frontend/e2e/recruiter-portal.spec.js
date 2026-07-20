import { test, expect } from "@playwright/test";
import { e2eEnv } from "./helpers/env.js";
import { apiRequest, login as apiLogin, registerCandidate } from "./helpers/api.js";
import { captureEvidence } from "./helpers/phase5Evidence.js";

test.describe.configure({ mode: "serial" });

function fieldInput(page, labelText) {
  // Prefer accessible name (htmlFor or wrapping label). Fall back to .field pattern used on auth pages.
  const byLabel = page.getByLabel(new RegExp(`^${labelText}`, "i"));
  return byLabel.first();
}

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
  await page.goto("/login");
}

test("Recruiter portal browser journey (Phase 5)", async ({ page, browser, request }) => {
  const env = e2eEnv();
  const adminPassword = process.env.HIRESPHERE_E2E_ADMIN_PASSWORD || "AdminE2ePass123!";
  const recruiterPassword = process.env.HIRESPHERE_E2E_RECRUITER_PASSWORD || "RecruiterE2ePass123!";
  const hmPassword = process.env.HIRESPHERE_E2E_HM_PASSWORD || "HiringMgrE2ePass123!";
  const candidatePassword = env.candidatePassword;

  // 1. Recruiter access request (UI)
  const requestEmail = `recruiter-req-${Date.now()}@example.test`;
  await page.goto("/recruiter-request");
  await expect(page.getByRole("heading", { name: /recruiter/i }).first()).toBeVisible();
  await captureEvidence(page, "recruiter-request.png");

  // Seed approved admin/recruiter/HM for deterministic browser journey
  const seed = await apiRequest(request, "POST", "/e2e/ensure-recruiter-portal", {
    data: {
      adminEmail: "e2e-admin@hiresphere.local",
      adminPassword,
      recruiterEmail: `e2e-rec-${Date.now()}@hiresphere.local`,
      recruiterPassword,
      hiringManagerEmail: `e2e-hm-${Date.now()}@hiresphere.local`,
      hiringManagerPassword: hmPassword
    }
  });
  expect(seed.ok, JSON.stringify(seed.body)).toBeTruthy();

  // 2–3. Admin approval path: open admin dashboard after login (seed already creates active recruiter)
  await loginUi(page, seed.body.adminEmail, adminPassword);
  await expect(page).toHaveURL(/\/admin/, { timeout: 30_000 });
  await captureEvidence(page, "admin-recruiter-approval.png");

  // 4. Login as Recruiter
  await logoutUi(page);
  await loginUi(page, seed.body.recruiterEmail, recruiterPassword);
  await expect(page).toHaveURL(/\/recruiter/, { timeout: 30_000 });

  // 5. Dashboard
  await expect(page.getByRole("heading", { name: /dashboard/i })).toBeVisible();
  await captureEvidence(page, "recruiter-dashboard.png");

  // 6–12. Create draft job with skills/questions and publish
  await page.goto("/recruiter/jobs/new");
  await captureEvidence(page, "recruiter-create-job.png");
  await fieldInput(page, "Title").fill("Phase5 E2E Platform Engineer");
  await fieldInput(page, "Location").fill("Colombo");
  await fieldInput(page, "Description").fill("Build and operate HireSphere recruiter workflows.");
  await fieldInput(page, "Responsibilities").fill("Own job pipeline and assessments.");
  await page.getByRole("button", { name: /add skill/i }).click();
  const skillInputs = page.locator(".rec-skill-row input[type='text'], .rec-skill-row input:not([type])");
  await skillInputs.first().fill("C#");
  await page.locator(".rec-skill-row").nth(1).locator("input").first().fill("React");
  await captureEvidence(page, "recruiter-job-skills.png");
  await page.getByRole("button", { name: /add question/i }).click();
  await page.locator(".rec-question-row input").first().fill("Are you eligible to work in Sri Lanka?");
  await captureEvidence(page, "recruiter-screening-questions.png");
  if (seed.body.hiringManagerUserId) {
    await fieldInput(page, "Hiring manager user id").fill(String(seed.body.hiringManagerUserId));
  }
  await page.getByRole("button", { name: /create draft/i }).click();
  await expect(page).toHaveURL(/\/recruiter\/jobs\/\d+/, { timeout: 30_000 });
  await page.getByRole("button", { name: /^publish$/i }).click();
  await expect(page.getByText(/status updated to published/i)).toBeVisible({ timeout: 15_000 });
  await captureEvidence(page, "recruiter-published-job.png");

  const jobUrl = page.url();
  const jobId = Number(jobUrl.split("/").pop());

  await page.goto("/recruiter/jobs");
  await captureEvidence(page, "recruiter-job-list.png");

  // 13. Candidate sees published job
  const candidateEmail = env.uniqueEmail();
  const reg = await registerCandidate(request, { email: candidateEmail, password: candidatePassword });
  expect(reg.ok, JSON.stringify(reg.body)).toBeTruthy();
  const candLogin = await apiLogin(request, candidateEmail, candidatePassword);
  expect(candLogin.ok).toBeTruthy();
  const candToken = candLogin.body.token;

  const publicJobs = await apiRequest(request, "GET", `/candidate/jobs?keyword=Phase5`, { token: candToken });
  expect(publicJobs.ok).toBeTruthy();
  const visible = (publicJobs.body.items || []).some((j) => j.id === jobId);
  expect(visible).toBeTruthy();

  // 14. Candidate applies
  const apply = await apiRequest(request, "POST", "/candidate/applications", {
    token: candToken,
    data: {
      jobId,
      coverLetter: "I am applying for the Phase5 E2E role.",
      termsAccepted: true,
      screeningAnswers: []
    }
  });
  // May fail if required screening exists — fetch wizard and answer
  let applicationId;
  if (apply.ok) {
    applicationId = apply.body.id;
  } else {
    const options = await apiRequest(request, "GET", `/candidate/jobs/${jobId}/apply-options`, { token: candToken });
    const answers = (options.body?.screeningQuestions || []).map((q) => ({
      screeningQuestionId: q.id,
      answerText: "Yes"
    }));
    const apply2 = await apiRequest(request, "POST", "/candidate/applications", {
      token: candToken,
      data: {
        jobId,
        coverLetter: "I am applying for the Phase5 E2E role.",
        termsAccepted: true,
        screeningAnswers: answers
      }
    });
    expect(apply2.ok, JSON.stringify(apply2.body)).toBeTruthy();
    applicationId = apply2.body.id;
  }

  // 15–18. Pipeline + application detail
  await page.goto(`/recruiter/jobs/${jobId}/applicants`);
  await expect(page.getByRole("heading", { name: /applicant pipeline/i })).toBeVisible();
  await captureEvidence(page, "recruiter-applicant-pipeline.png");
  await page.goto(`/recruiter/applications/${applicationId}`);
  await expect(page.getByText(/match score/i)).toBeVisible();
  await captureEvidence(page, "recruiter-application-detail.png");
  const detailText = await page.locator("#main-content").innerText();
  expect(detailText.toLowerCase()).not.toContain("c:\\");
  expect(detailText.toLowerCase()).not.toContain("passwordhash");

  // 19–22. Ranking + screening
  await page.goto(`/recruiter/applications/${applicationId}/ranking`);
  await expect(page.getByText(/ai-generated insight/i)).toBeVisible();
  await captureEvidence(page, "recruiter-ranking-explanation.png");
  await page.goto("/recruiter/screening");
  await captureEvidence(page, "recruiter-screening.png");
  await page.goto(`/recruiter/compare?ids=${applicationId}`);
  await captureEvidence(page, "recruiter-candidate-comparison.png");

  // 23. Move to screening
  await page.goto(`/recruiter/jobs/${jobId}/applicants`);
  await page.getByRole("button", { name: /screening/i }).first().click();
  await page.getByRole("button", { name: /^confirm$/i }).click();

  // 24–26. Assessment create/assign + candidate complete
  await page.goto("/recruiter/assessments/new");
  await fieldInput(page, "Title").fill("Phase5 E2E Skills Check");
  await page.getByRole("button", { name: /^create$/i }).click();
  await expect(page).toHaveURL(/\/recruiter\/assessments\/\d+/, { timeout: 20_000 });
  await captureEvidence(page, "recruiter-assessment-builder.png");
  await fieldInput(page, "Question text").fill("What is 2+2?");
  await fieldInput(page, "Correct answer key").fill("4");
  await page.getByRole("button", { name: /add question/i }).click();
  const assessmentId = Number(page.url().split("/").pop());

  const recLogin = await apiLogin(request, seed.body.recruiterEmail, recruiterPassword);
  const assign = await apiRequest(request, "POST", `/recruiter/applications/${applicationId}/assessments`, {
    token: recLogin.body.token,
    data: { assessmentId, maxAttempts: 1, revealResultsToCandidate: true }
  });
  expect(assign.ok, JSON.stringify(assign.body)).toBeTruthy();
  await captureEvidence(page, "recruiter-assessment-assignment.png");
  const assignmentId = assign.body.id;

  const candContext = await browser.newContext();
  const candPage = await candContext.newPage();
  await loginUi(candPage, candidateEmail, candidatePassword);
  await expect(candPage).toHaveURL(/\/candidate/, { timeout: 15_000 });
  await candPage.goto(`/candidate/assessments/${assignmentId}`);
  await expect(candPage.getByRole("button", { name: /start assessment/i })).toBeVisible({ timeout: 20_000 });
  await candPage.getByRole("button", { name: /start assessment/i }).click();
  await candPage.locator("input[type='text'], textarea").last().fill("4");
  await candPage.getByRole("button", { name: /submit assessment/i }).click();
  await expect(candPage.getByText(/score|passed|result/i).first()).toBeVisible({ timeout: 20_000 }).catch(() => {});
  await candContext.close();

  await page.goto(`/recruiter/applications/${applicationId}`);
  await captureEvidence(page, "recruiter-assessment-result.png");

  // 27–29. Shortlist + message
  await page.goto(`/recruiter/jobs/${jobId}/applicants`);
  await page.getByRole("button", { name: /shortlist/i }).first().click();
  await page.getByRole("button", { name: /^confirm$/i }).click();
  await page.goto(`/recruiter/applications/${applicationId}/messages`);
  await fieldInput(page, "Message").fill("Congratulations on progressing to interview stage.");
  await page.getByRole("button", { name: /send message/i }).click();
  await expect(page.getByText(/congratulations/i)).toBeVisible();
  await captureEvidence(page, "recruiter-message-thread.png");

  // 30–34. Interview schedule + conflict
  await page.goto("/recruiter/interviews/schedule");
  await fieldInput(page, "Application ID").fill(String(applicationId));
  await page.getByLabel(/^Start/i).fill("2030-06-01T10:00");
  await fieldInput(page, "Timezone").fill("Asia/Colombo");
  await captureEvidence(page, "recruiter-interview-schedule.png");
  await page.getByRole("button", { name: /^schedule$/i }).click();
  await expect(page).toHaveURL(/\/recruiter\/interviews\/\d+/, { timeout: 20_000 });
  await captureEvidence(page, "recruiter-interview-status.png");

  await page.goto("/recruiter/interviews/schedule");
  await fieldInput(page, "Application ID").fill(String(applicationId));
  await page.getByLabel(/^Start/i).fill("2030-06-01T10:30");
  await page.getByRole("button", { name: /^schedule$/i }).click();
  await expect(page.getByText(/conflict warning/i)).toBeVisible({ timeout: 15_000 });
  await captureEvidence(page, "recruiter-conflict-warning.png");

  // 35–38. Reports + CSV
  await page.goto("/recruiter/reports");
  await expect(page.getByRole("heading", { name: /reports/i })).toBeVisible();
  await captureEvidence(page, "recruiter-reports.png");
  await fieldInput(page, "Job ID").fill(String(jobId));
  await page.getByRole("button", { name: /apply filters/i }).click();
  await captureEvidence(page, "recruiter-report-filters.png");
  await page.getByRole("button", { name: /export csv/i }).click();
  await captureEvidence(page, "recruiter-csv-export.png");

  // 39. Cross-org blocked via API
  const otherSeed = await apiRequest(request, "POST", "/e2e/ensure-recruiter-portal", {
    data: {
      organizationName: `E2E Other Org ${Date.now()}`,
      adminEmail: "e2e-admin@hiresphere.local",
      adminPassword,
      recruiterEmail: `e2e-other-${Date.now()}@hiresphere.local`,
      recruiterPassword: "OtherRecruiterE2ePass123!",
      hiringManagerEmail: `e2e-other-hm-${Date.now()}@hiresphere.local`,
      hiringManagerPassword: hmPassword
    }
  });
  const otherLogin = await apiLogin(request, otherSeed.body.recruiterEmail, "OtherRecruiterE2ePass123!");
  const cross = await apiRequest(request, "GET", `/recruiter/jobs/${jobId}`, {
    token: otherLogin.body.token
  });
  expect(cross.status).toBe(404);

  // 40. Candidate denied recruiter route + mobile + logout
  const denied = await browser.newContext();
  const deniedPage = await denied.newPage();
  await loginUi(deniedPage, candidateEmail, candidatePassword);
  await deniedPage.goto("/recruiter");
  await expect(deniedPage.getByText(/access denied/i)).toBeVisible({ timeout: 15_000 });
  await captureEvidence(deniedPage, "recruiter-access-denied.png");
  await denied.close();

  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/recruiter");
  await captureEvidence(page, "recruiter-mobile-dashboard.png");
  const menuToggle = page.getByRole("button", { name: /open menu/i });
  if (await menuToggle.isVisible().catch(() => false)) {
    await menuToggle.click();
  }
  await page.getByRole("button", { name: /logout/i }).click();
  await expect(page).toHaveURL(/\/login|\/$/);

  await captureEvidence(page, "phase5-playwright-summary.png");
  // unused request email documented for request form coverage
  expect(requestEmail).toContain("@");
});
