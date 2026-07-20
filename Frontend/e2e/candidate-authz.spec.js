import { test, expect } from "@playwright/test";
import { e2eEnv } from "./helpers/env.js";
import {
  ensureCatalog,
  prepareCandidateJourney,
  registerCandidate,
  login,
  apiRequest
} from "./helpers/api.js";

test.describe("Candidate authorization boundaries", () => {
  test("blocks cross-candidate and privileged APIs; omits secrets", async ({ request }) => {
    const env = e2eEnv();
    const password = env.candidatePassword;
    const emailA = env.uniqueEmail();
    const emailB = `candidate-e2e-b-${Date.now()}@example.test`;

    const catalog = await ensureCatalog(request);
    expect(catalog.ok).toBeTruthy();

    const regA = await registerCandidate(request, { email: emailA, password, firstName: "Alpha" });
    expect(regA.ok, JSON.stringify(regA.body)).toBeTruthy();
    const tokenA = regA.body.token;

    const regB = await registerCandidate(request, { email: emailB, password, firstName: "Beta" });
    expect(regB.ok).toBeTruthy();
    const tokenB = regB.body.token;

    const profileB = await apiRequest(request, "GET", "/candidate/profile", { token: tokenB });
    expect(profileB.ok).toBeTruthy();

    // Prepare journey for B so we have application/assessment/interview/resume targets
    await prepareCandidateJourney(request, emailB);
    const appsB = await apiRequest(request, "GET", "/candidate/applications", { token: tokenB });
    expect(appsB.ok).toBeTruthy();
    const appId = Array.isArray(appsB.body)
      ? appsB.body[0]?.id
      : appsB.body?.items?.[0]?.id || appsB.body?.[0]?.id;

    const resumesB = await apiRequest(request, "GET", "/candidate/resumes", { token: tokenB });
    const resumeId = Array.isArray(resumesB.body) ? resumesB.body[0]?.id : undefined;

    const assessmentsB = await apiRequest(request, "GET", "/candidate/assessments", { token: tokenB });
    const assignmentId = Array.isArray(assessmentsB.body)
      ? assessmentsB.body[0]?.id || assessmentsB.body[0]?.assignmentId
      : assessmentsB.body?.items?.[0]?.id;

    const interviewsB = await apiRequest(request, "GET", "/candidate/interviews", { token: tokenB });
    const interviewId = Array.isArray(interviewsB.body)
      ? interviewsB.body[0]?.id
      : interviewsB.body?.items?.[0]?.id;

    // Cross-candidate application
    if (appId) {
      const crossApp = await apiRequest(request, "GET", `/candidate/applications/${appId}`, { token: tokenA });
      expect([403, 404]).toContain(crossApp.status);
    }

    // Cross-candidate resume/document
    if (resumeId) {
      const crossResume = await apiRequest(request, "DELETE", `/candidate/resumes/${resumeId}`, { token: tokenA });
      expect([403, 404]).toContain(crossResume.status);
    }

    // Unassigned assessment
    if (assignmentId) {
      const crossAssess = await apiRequest(request, "GET", `/candidate/assessments/${assignmentId}`, { token: tokenA });
      expect([403, 404]).toContain(crossAssess.status);
    }

    // Cross interview
    if (interviewId) {
      const crossInt = await apiRequest(request, "GET", `/candidate/interviews/${interviewId}`, { token: tokenA });
      expect([403, 404]).toContain(crossInt.status);
    }

    // Privileged APIs
    for (const route of ["/admin/users", "/Jobs/MyJobs"]) {
      const denied = await apiRequest(request, "GET", route, { token: tokenA });
      expect(denied.status, route).toBe(403);
    }

    // me DTO no password hash
    const me = await apiRequest(request, "GET", "/auth/me", { token: tokenA });
    expect(me.ok).toBeTruthy();
    expect(me.body.passwordHash).toBeUndefined();
    expect(JSON.stringify(me.body)).not.toMatch(/passwordHash/i);

    // Resume metadata no absolute path
    const resumesA = await apiRequest(request, "GET", "/candidate/resumes", { token: tokenA });
    expect(JSON.stringify(resumesA.body)).not.toMatch(/[A-Za-z]:\\\\/);
    expect(JSON.stringify(resumesA.body)).not.toMatch(/C:\\\\Users/i);

    // Assessment payload no answer keys
    if (assignmentId) {
      const detail = await apiRequest(request, "GET", `/candidate/assessments/${assignmentId}`, { token: tokenB });
      expect(JSON.stringify(detail.body)).not.toMatch(/CorrectAnswerKey/i);
    }

    // Duplicate application rejection
    const apply1 = await apiRequest(request, "POST", "/candidate/applications", {
      token: tokenA,
      data: {
        jobId: catalog.body.jobId,
        coverLetter: "First",
        termsAccepted: true,
        screeningAnswers: []
      }
    });
    // May fail if screening required — fetch apply-options
    const options = await apiRequest(request, "GET", `/candidate/jobs/${catalog.body.jobId}/apply-options`, {
      token: tokenA
    });
    expect(options.ok).toBeTruthy();
    const screeningAnswers = (options.body.screeningQuestions || []).map((q) => ({
      screeningQuestionId: q.id,
      answerText: "E2E"
    }));
    const first = await apiRequest(request, "POST", "/candidate/applications", {
      token: tokenA,
      data: {
        jobId: catalog.body.jobId,
        coverLetter: "First apply",
        termsAccepted: true,
        screeningAnswers
      }
    });
    expect([200, 201].includes(first.status) || first.ok).toBeTruthy();
    const dup = await apiRequest(request, "POST", "/candidate/applications", {
      token: tokenA,
      data: {
        jobId: catalog.body.jobId,
        coverLetter: "Dup",
        termsAccepted: true,
        screeningAnswers
      }
    });
    expect(dup.ok).toBeFalsy();
    expect([400, 409]).toContain(dup.status);

    // Closed job rejection — use prepare then close via raw not available; skip if no admin job API
    // Unsupported document rejection covered in browser test.

    // CORS rejects unapproved origin
    const cors = await request.fetch(`${env.apiUrl}/api/auth/me`, {
      method: "OPTIONS",
      headers: {
        Origin: "https://evil.example",
        "Access-Control-Request-Method": "GET"
      }
    });
    const allowOrigin = cors.headers()["access-control-allow-origin"];
    expect(allowOrigin === undefined || allowOrigin !== "https://evil.example").toBeTruthy();
  });
});
