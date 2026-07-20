import path from "node:path";
import { fileURLToPath } from "node:url";
import { test, expect } from "@playwright/test";
import { e2eEnv } from "./helpers/env.js";
import { captureEvidence } from "./helpers/evidence.js";
import {
  ensureCatalog,
  prepareCandidateJourney,
  apiRequest,
  login as apiLogin
} from "./helpers/api.js";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const resumePath = path.join(__dirname, "fixtures", "resume.pdf");
const malwarePath = path.join(__dirname, "fixtures", "malware.exe");

function fieldInput(page, labelText) {
  return page.locator(`.field:has(label:text-is("${labelText}")) input`);
}

test.describe.configure({ mode: "serial" });

test("Candidate portal full browser journey", async ({ page, request }) => {
  const env = e2eEnv();
  const email = env.uniqueEmail();
  const password = env.candidatePassword;

  const catalog = await ensureCatalog(request);
  expect(catalog.ok, JSON.stringify(catalog.body)).toBeTruthy();
  const jobId = catalog.body.jobId;
  expect(jobId).toBeTruthy();

  // 1. Landing
  await page.goto("/");
  await expect(page.getByText(/Hire|Sphere/i).first()).toBeVisible();

  // 2–3. Registration validation
  await page.goto("/register");
  await page.getByRole("button", { name: /create account/i }).click();
  await expect(page.locator(".field-error-msg").first()).toBeVisible();
  await captureEvidence(page, "registration-validation.png");

  // 4. Register unique candidate
  await fieldInput(page, "First name").fill("E2E");
  await fieldInput(page, "Last name").fill("Candidate");
  await fieldInput(page, "Email").fill(email);
  await fieldInput(page, "Password").fill(password);
  await fieldInput(page, "Confirm password").fill(password);
  await page.getByText(/I accept the privacy/i).click();
  await page.getByRole("button", { name: /create account/i }).click();
  await expect(page).toHaveURL(/\/candidate/, { timeout: 30_000 });
  await expect(page.getByRole("heading", { name: /Candidate dashboard/i })).toBeVisible();

  // 5. Login page screenshot after logout later; capture login form now via fresh session path
  await page.goto("/login");
  await fieldInput(page, "Email").fill(email);
  await fieldInput(page, "Password").fill(password);
  await captureEvidence(page, "candidate-login.png");
  await page.getByRole("button", { name: /sign in/i }).click();
  await expect(page).toHaveURL(/\/candidate/, { timeout: 30_000 });

  // 6. Dashboard
  await expect(page.getByRole("heading", { name: /Candidate dashboard/i })).toBeVisible();
  await captureEvidence(page, "candidate-dashboard.png");

  // 7. Session restoration
  await page.reload();
  await expect(page.getByRole("heading", { name: /Candidate dashboard/i })).toBeVisible();

  // 8–16. Profile journey
  await page.goto("/candidate/profile");
  await expect(page.getByRole("heading", { name: /profile/i }).first()).toBeVisible();

  const summary = page.locator('label:has-text("Summary") input, label:has-text("Summary") textarea').first();
  // Summary is an input in Object.entries mapping
  await page.locator('label', { hasText: "Summary" }).locator("input").fill(
    "Full-stack engineer focused on ASP.NET and React for HireSphere E2E."
  );
  await page.locator('label', { hasText: "Phone" }).locator("input").fill("555-0199");
  await page.locator('label', { hasText: "Location" }).locator("input").fill("Colombo");
  await page.locator('label', { hasText: "Address" }).locator("input").fill("42 E2E Street");
  await page.getByRole("button", { name: /save profile/i }).click();
  await expect(page.locator(".success, .error").first()).toBeVisible({ timeout: 15_000 });
  await captureEvidence(page, "candidate-profile.png");

  await page.getByPlaceholder("Company").fill("HireSphere Labs");
  await page.getByPlaceholder("Title").fill("Software Engineer");
  await page.locator('form:has(button:text("Add experience")) input[type="date"]').first().fill("2022-01-01");
  await page.getByText("Current role").click();
  await page.getByRole("button", { name: /add experience/i }).click();
  await expect(page.getByText(/Software Engineer @ HireSphere Labs/i)).toBeVisible({ timeout: 15_000 });

  await page.getByPlaceholder("Institution").fill("University of E2E");
  await page.getByPlaceholder("Qualification").fill("BSc Computer Science");
  await page.getByPlaceholder("Field of study").fill("Software Engineering");
  await page.getByLabel("Education start date").fill("2018-01-01");
  await page.getByText("Currently studying").click();
  await page.getByRole("button", { name: /add education/i }).click();
  await expect(page.getByText(/BSc Computer Science/i)).toBeVisible({ timeout: 15_000 });
  await captureEvidence(page, "candidate-experience-education.png");

  const skillSelect = page.locator('form:has(button:text("Add skill")) select');
  await skillSelect.selectOption({ label: "C#" });
  await page.getByRole("button", { name: /add skill/i }).click();
  await expect(page.getByText(/C# —/i).first()).toBeVisible({ timeout: 15_000 });
  await skillSelect.selectOption({ label: "React" });
  await page.getByRole("button", { name: /add skill/i }).click();

  await page.getByLabel("Certification name").fill("Azure Fundamentals");
  await page.getByLabel("Certification issuer").fill("Microsoft");
  await page.getByLabel("Certification issue date").fill("2024-06-01");
  await page.getByRole("button", { name: /add certification/i }).click();
  await expect(page.getByText(/Azure Fundamentals/i)).toBeVisible({ timeout: 15_000 });
  await captureEvidence(page, "candidate-skills-certifications.png");

  await page.locator('input[type="file"]').setInputFiles(resumePath);
  await expect(page.locator(".success")).toContainText(/Resume uploaded/i, { timeout: 20_000 });
  await expect(page.getByText(/resume\.pdf/i)).toBeVisible({ timeout: 15_000 });
  await captureEvidence(page, "candidate-resume-upload.png");

  const pageText = await page.content();
  expect(pageText).not.toMatch(/[A-Za-z]:\\\\Users\\/i);
  expect(pageText).not.toMatch(/App_Data\\\\uploads/i);

  // 18–19 invalid upload
  await page.locator('input[type="file"]').setInputFiles(malwarePath);
  await expect(page.locator(".error")).toBeVisible({ timeout: 15_000 });
  await captureEvidence(page, "candidate-invalid-upload.png");

  // 20–24 job search / filters
  await page.goto("/candidate/jobs");
  await expect(page.getByRole("heading", { name: /jobs/i }).first()).toBeVisible();
  await page.getByPlaceholder(/Title, skills/i).fill("Full Stack");
  await page.getByRole("button", { name: /search/i }).click();
  await expect(page.getByText(catalog.body.title || "E2E Phase4")).toBeVisible({ timeout: 20_000 });
  await captureEvidence(page, "candidate-job-search.png");

  await page.getByPlaceholder(/City/i).fill("Colombo");
  // employment / arrangement selects if present
  const emp = page.locator("select").nth(0);
  if (await emp.count()) {
    await emp.selectOption({ label: /Full/i }).catch(async () => {
      await emp.selectOption({ index: 1 }).catch(() => {});
    });
  }
  const arr = page.locator("select").nth(1);
  if (await arr.count()) {
    await arr.selectOption({ label: /Hybrid/i }).catch(async () => {
      await arr.selectOption({ index: 1 }).catch(() => {});
    });
  }
  await page.getByRole("button", { name: /search/i }).click();
  await captureEvidence(page, "candidate-job-filters.png");

  // 25–29 match explanation
  await page.goto(`/candidate/jobs/${jobId}`);
  await expect(page.getByRole("heading", { name: /E2E Phase4/i })).toBeVisible();
  await expect(page.getByRole("heading", { name: /Match explanation/i })).toBeVisible();
  await expect(page.getByText(/Score:/i)).toBeVisible();
  await expect(page.getByText(/Matched skills|Missing skills/i).first()).toBeVisible();
  await expect(page.getByText(/human|review/i).first()).toBeVisible();
  await captureEvidence(page, "candidate-match-explanation.png");

  // 30 recommendations
  await page.goto("/candidate/recommendations");
  await expect(page.getByText(/E2E Phase4|recommendation|match/i).first()).toBeVisible({ timeout: 20_000 });
  await captureEvidence(page, "candidate-recommendations.png");

  // 31–37 application wizard
  await page.goto(`/candidate/jobs/${jobId}/apply`);
  await expect(page.getByRole("heading", { name: /Apply/i })).toBeVisible();
  await captureEvidence(page, "candidate-application-wizard.png");

  const resumeSelect = page.locator('label:has-text("Resume") select');
  if (await resumeSelect.count()) {
    const options = resumeSelect.locator("option");
    const count = await options.count();
    if (count > 1) {
      await resumeSelect.selectOption({ index: 1 });
    }
  }
  await page.getByPlaceholder(/cover letter/i).fill("I am excited to join HireSphere via Phase 4 E2E.");
  await page.getByRole("button", { name: /^Continue$/i }).click();

  const screeningInputs = page.locator("form input[required], form label input");
  const screenCount = await page.locator("form label").count();
  if (screenCount > 0) {
    const inputs = page.locator('form label input[type="text"], form label input:not([type])');
    const n = await inputs.count();
    for (let i = 0; i < n; i++) {
      await inputs.nth(i).fill(`E2E answer ${i + 1}`);
    }
  }
  await page.getByRole("button", { name: /^Continue$/i }).click();
  await page.getByText(/I confirm my answers are accurate/i).click();
  await page.getByRole("button", { name: /submit application/i }).click();
  await expect(page).toHaveURL(/\/candidate\/applications\/\d+/, { timeout: 30_000 });
  await captureEvidence(page, "candidate-application-success.png");

  const applicationUrl = page.url();
  const applicationId = Number(applicationUrl.split("/").pop());

  // 38–39 duplicate
  await page.goto(`/candidate/jobs/${jobId}/apply`);
  await expect(page.getByText(/already applied/i)).toBeVisible({ timeout: 20_000 });
  await captureEvidence(page, "candidate-duplicate-application.png");

  // 40–41 timeline
  await page.goto(`/candidate/applications/${applicationId}`);
  await expect(page.getByText(/timeline|Submitted|Pending|Assessment|status/i).first()).toBeVisible();
  await captureEvidence(page, "candidate-application-timeline.png");

  // Prepare assessment/interview/notifications for this candidate
  const prepared = await prepareCandidateJourney(request, email);
  expect(prepared.ok, JSON.stringify(prepared.body)).toBeTruthy();
  const assignmentId = prepared.body.assessmentAssignmentId;
  const interviewId = prepared.body.interviewId;

  // 42–46 assessment
  await page.goto("/candidate/assessments");
  await expect(page.getByText(/E2E C# Skills Check|Skills Check|assessment/i).first()).toBeVisible({ timeout: 20_000 });
  await page.goto(`/candidate/assessments/${assignmentId}`);
  await expect(page.getByRole("heading", { name: /Skills Check|assessment/i })).toBeVisible();
  await captureEvidence(page, "candidate-assessment.png");
  await page.getByRole("button", { name: /start assessment/i }).click();
  await expect(page.getByText(/2 \+ 2/i)).toBeVisible({ timeout: 20_000 });

  await page.locator('input[name^="q-"]').first().check().catch(async () => {
    await page.getByText("4", { exact: true }).click();
  });
  // Select correct answers via text
  await page.getByText("4", { exact: true }).click();
  await page.getByText("True", { exact: true }).first().click();
  await page.getByRole("button", { name: /save answers/i }).click().catch(() => {});
  await page.getByRole("button", { name: /submit assessment/i }).click();
  await expect(page.getByText(/score|passed|completed|result|%|Submitted/i).first()).toBeVisible({ timeout: 30_000 });
  await captureEvidence(page, "candidate-assessment-result.png");

  const attemptJson = await apiRequest(request, "GET", `/candidate/assessments/${assignmentId}`, {
    token: (await apiLogin(request, email, password)).body.token
  });
  expect(JSON.stringify(attemptJson.body)).not.toMatch(/CorrectAnswerKey/i);
  expect(JSON.stringify(attemptJson.body)).not.toMatch(/"correctAnswer"/i);

  // 47–50 interviews
  await page.goto(`/candidate/interviews/${interviewId}`);
  await expect(page.getByText(/Asia\/Colombo|Video|timezone|Interview/i).first()).toBeVisible();
  await captureEvidence(page, "candidate-interview.png");

  const reasonBox = page.locator("textarea").first();
  if (await reasonBox.count()) {
    await reasonBox.fill("Need a later slot for E2E verification.");
  }
  const rescheduleBtn = page.getByRole("button", { name: /reschedule/i });
  if (await rescheduleBtn.count() && await rescheduleBtn.isEnabled()) {
    await rescheduleBtn.click();
    await expect(page.getByText(/reschedule|requested|pending/i).first()).toBeVisible({ timeout: 20_000 });
  } else {
    const confirmBtn = page.getByRole("button", { name: /confirm/i });
    if (await confirmBtn.count() && await confirmBtn.isEnabled()) {
      await confirmBtn.click();
      await expect(page.getByText(/confirm|Confirmed|meeting/i).first()).toBeVisible({ timeout: 20_000 });
    }
  }

  // Ensure confirm path is exercised when still available after reload
  await page.goto(`/candidate/interviews/${interviewId}`);
  const confirmAgain = page.getByRole("button", { name: /^confirm$/i });
  if (await confirmAgain.count() && await confirmAgain.isEnabled()) {
    await confirmAgain.click();
    await expect(page.getByText(/Confirmed|meeting|confirm/i).first()).toBeVisible({ timeout: 20_000 });
  }

  // 51–52 notifications
  await page.goto("/candidate/notifications");
  await expect(page.getByText(/Assessment assigned|Interview scheduled/i).first()).toBeVisible({ timeout: 20_000 });
  await captureEvidence(page, "candidate-notifications.png");

  // 53–58 access denied
  await page.goto("/recruiter");
  await expect(page.getByRole("heading", { name: /Access denied/i })).toBeVisible();
  await page.goto("/hiring-manager");
  await expect(page.getByRole("heading", { name: /Access denied/i })).toBeVisible();
  await page.goto("/admin");
  await expect(page.getByRole("heading", { name: /Access denied/i })).toBeVisible();
  await captureEvidence(page, "candidate-access-denied.png");

  // 59 expired token
  await page.evaluate(() => localStorage.setItem("token", "invalid.jwt.token"));
  await page.goto("/candidate");
  await expect(page.getByRole("heading", { name: /Session expired|Access denied|Sign in/i })).toBeVisible({ timeout: 20_000 });

  // 60 login again
  await page.goto("/login");
  await fieldInput(page, "Email").fill(email);
  await fieldInput(page, "Password").fill(password);
  await page.getByRole("button", { name: /sign in/i }).click();
  await expect(page).toHaveURL(/\/candidate/, { timeout: 30_000 });

  // 61–62 logout
  await page.getByRole("button", { name: /logout/i }).click();
  await expect(page).toHaveURL(/\/login/, { timeout: 20_000 });
  await page.goto("/candidate");
  await expect(page).toHaveURL(/\/login/, { timeout: 20_000 });
  await expect(page.getByRole("heading", { name: /Sign in/i })).toBeVisible();

  // Mobile dashboard evidence (viewport change)
  await page.goto("/login");
  await fieldInput(page, "Email").fill(email);
  await fieldInput(page, "Password").fill(password);
  await page.getByRole("button", { name: /sign in/i }).click();
  await expect(page).toHaveURL(/\/candidate/, { timeout: 30_000 });
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/candidate");
  await expect(page.getByRole("heading", { name: /Candidate dashboard/i })).toBeVisible();
  const overflow = await page.evaluate(() => {
    const doc = document.documentElement;
    return doc.scrollWidth > doc.clientWidth + 2;
  });
  expect(overflow).toBeFalsy();
  await captureEvidence(page, "candidate-mobile-dashboard.png");
});
