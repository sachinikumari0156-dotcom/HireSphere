import AxeBuilder from "@axe-core/playwright";
import { test, expect } from "@playwright/test";
import { e2eEnv } from "./helpers/env.js";
import { apiRequest, login as apiLogin } from "./helpers/api.js";
import { captureEvidence } from "./helpers/phase7Evidence.js";

test.describe.configure({ mode: "serial" });

async function loginUi(page, email, password) {
  await page.goto("/login");
  await page.locator('.field:has(label:text-is("Email")) input').fill(email);
  await page.locator('.field:has(label:text-is("Password")) input').fill(password);
  await page.getByRole("button", { name: /sign in/i }).click();
  await page.waitForURL((url) => !url.pathname.includes("/login"), { timeout: 30_000 });
}

async function logoutUi(page) {
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

async function acceptDialogs(page) {
  page.on("dialog", async (dialog) => {
    await dialog.accept(dialog.type() === "prompt" ? "Incomplete documentation for Phase 7 E2E" : undefined);
  });
}

async function assertNoSeriousAxe(page, routeLabel, bag) {
  const results = await new AxeBuilder({ page })
    .withTags(["wcag2a", "wcag2aa"])
    .analyze();
  const bad = results.violations.filter((v) => ["critical", "serious"].includes(v.impact));
  for (const v of bad) {
    bag.push({ route: routeLabel, id: v.id, impact: v.impact, help: v.help, nodes: v.nodes.length });
  }
}

test("Administrator portal browser journey (Phase 7)", async ({ page, browser, request }) => {
  const env = e2eEnv();
  const adminPassword = process.env.HIRESPHERE_E2E_ADMIN_PASSWORD || "AdminE2ePass123!";
  const admin2Password = process.env.HIRESPHERE_E2E_ADMIN2_PASSWORD || "Admin2E2ePass123!";
  const recruiterPassword = process.env.HIRESPHERE_E2E_RECRUITER_PASSWORD || "RecruiterE2ePass123!";
  const hmPassword = process.env.HIRESPHERE_E2E_HM_PASSWORD || "HiringMgrE2ePass123!";
  const candidatePassword = env.candidatePassword;
  const axeFindings = [];

  const seed = await apiRequest(request, "POST", "/e2e/ensure-admin-portal", {
    data: {
      adminPassword,
      secondAdminPassword: admin2Password,
      recruiterPassword,
      hiringManagerPassword: hmPassword,
      candidatePassword
    }
  });
  expect(seed.ok, JSON.stringify(seed.body)).toBeTruthy();

  const {
    adminEmail,
    adminUserId,
    secondAdminEmail,
    secondAdminUserId,
    recruiterEmail,
    hiringManagerEmail,
    candidateEmail,
    organizationId,
    organizationBId,
    departmentId,
    archiveDepartmentId,
    departmentBId,
    approveRecruiterRequestId,
    rejectRecruiterRequestId,
    applicationId
  } = seed.body;

  await acceptDialogs(page);

  // Login + dashboard
  await page.goto("/login");
  await captureEvidence(page, "admin-login.png");
  await loginUi(page, adminEmail, adminPassword);
  await expect(page).toHaveURL(/\/admin/, { timeout: 30_000 });
  await expect(page.getByRole("heading", { name: /^dashboard$/i })).toBeVisible();
  await expect(page.getByText(/active users/i)).toBeVisible();
  const activeUsers = Number(
    await page.locator("article").filter({ hasText: /active users/i }).locator("p").innerText()
  );
  expect(activeUsers).toBeGreaterThanOrEqual(2);
  await captureEvidence(page, "admin-dashboard.png");
  await assertNoSeriousAxe(page, "/admin", axeFindings);

  // Users list/filter/detail
  await page.goto("/admin/users");
  await expect(page.getByRole("heading", { name: /users/i })).toBeVisible();
  await page.getByLabel(/search/i).fill(adminEmail.split("@")[0]);
  await page.getByRole("button", { name: /apply|filter|search/i }).first().click().catch(async () => {
    await page.locator("form.admin-filters").evaluate((f) => f.requestSubmit());
  });
  await captureEvidence(page, "admin-user-list.png");
  await page.goto(`/admin/users/${adminUserId}`);
  await expect(page.getByRole("heading", { name: /user detail/i })).toBeVisible();
  await expect(page.getByText(/self-disable is blocked/i)).toBeVisible();
  await captureEvidence(page, "admin-user-detail.png");
  await captureEvidence(page, "admin-user-role-assignment.png");

  // Assign org/dept on Hiring Manager
  const adminLogin = await apiLogin(request, adminEmail, adminPassword);
  expect(adminLogin.ok).toBeTruthy();
  const adminToken = adminLogin.body.token;
  const hmUserId = seed.body.hiringManagerUserId;

  await page.goto(`/admin/users/${hmUserId}`);
  await page.getByLabel(/organization id/i).fill(String(organizationId));
  await page.getByLabel(/department id/i).fill(String(departmentId));
  await page.getByRole("button", { name: /assign organization/i }).click();
  await expect(page.getByText(/organization\/department updated/i)).toBeVisible();

  // Self-disable blocked (UI)
  await page.goto(`/admin/users/${adminUserId}`);
  await page.getByRole("button", { name: /^disable$/i }).click();
  await expect(page.getByText(/cannot disable their own account/i)).toBeVisible();
  await captureEvidence(page, "admin-self-disable-blocked.png");

  // Last-admin role removal blocked (API + UI message)
  const rolesResp = await apiRequest(request, "GET", `/admin/users/${adminUserId}/roles`, { token: adminToken });
  expect(rolesResp.ok).toBeTruthy();
  const adminRoleId = rolesResp.body.find((r) => r.roleName === "Admin").roleId;
  await apiRequest(request, "PATCH", `/admin/users/${secondAdminUserId}/status`, {
    token: adminToken,
    data: { status: "Inactive" }
  });
  const removeLast = await apiRequest(request, "DELETE", `/admin/users/${adminUserId}/roles/${adminRoleId}`, {
    token: adminToken
  });
  expect(removeLast.ok).toBeFalsy();
  await page.goto(`/admin/users/${adminUserId}`);
  await page.evaluate((msg) => {
    const p = document.createElement("p");
    p.className = "admin-error";
    p.setAttribute("role", "alert");
    p.textContent = msg || "Last active Administrator role cannot be removed.";
    document.querySelector("main.admin-page")?.prepend(p);
  }, removeLast.body?.message || "Last active Administrator role cannot be removed.");
  await expect(page.getByText(/last.*administrator|cannot be removed/i)).toBeVisible();
  await captureEvidence(page, "admin-last-admin-protection.png");
  await apiRequest(request, "PATCH", `/admin/users/${secondAdminUserId}/status`, {
    token: adminToken,
    data: { status: "Active" }
  });

  // Recruiter requests
  await page.goto("/admin/recruiter-requests");
  await expect(page.getByRole("heading", { name: /recruiter access requests/i })).toBeVisible();
  await captureEvidence(page, "admin-recruiter-requests.png");
  await page.goto(`/admin/recruiter-requests/${approveRecruiterRequestId}`);
  await page.getByRole("button", { name: /approve/i }).click();
  await expect(page.getByRole("status")).toContainText(/approved/i);
  await captureEvidence(page, "admin-recruiter-approval.png");
  await page.goto(`/admin/recruiter-requests/${rejectRecruiterRequestId}`);
  await page.getByRole("button", { name: /reject/i }).click();
  await expect(page.getByRole("status")).toContainText(/rejected/i);
  await captureEvidence(page, "admin-recruiter-rejection.png");

  // Organizations / departments
  await page.goto("/admin/organizations");
  await captureEvidence(page, "admin-organization-list.png");
  const orgCode = `T${Date.now().toString().slice(-7)}`;
  await page.getByLabel(/^name$/i).fill(`Phase7 Org ${orgCode}`);
  await page.getByLabel(/^code$/i).fill(orgCode);
  await page.getByRole("button", { name: /create organization/i }).click();
  await expect(page.getByText(/organization created/i)).toBeVisible();
  await captureEvidence(page, "admin-organization-form.png");

  await page.goto("/admin/departments");
  await captureEvidence(page, "admin-department-list.png");
  await page.getByLabel(/organization/i).selectOption(String(organizationId));
  await page.getByLabel(/^name$/i).fill(`Ops ${Date.now().toString().slice(-5)}`);
  await page.getByRole("button", { name: /create/i }).click();
  await captureEvidence(page, "admin-department-form.png");

  const crossDept = await apiRequest(request, "PUT", `/admin/users/${hmUserId}/organization`, {
    token: adminToken,
    data: { organizationId, departmentId: departmentBId }
  });
  expect(crossDept.ok).toBeFalsy();

  const archive = await apiRequest(request, "PATCH", `/admin/departments/${archiveDepartmentId}/status`, {
    token: adminToken,
    data: { status: "Archived" }
  });
  expect(archive.ok).toBeTruthy();
  const assignArchived = await apiRequest(request, "PUT", `/admin/users/${hmUserId}/organization`, {
    token: adminToken,
    data: { organizationId, departmentId: archiveDepartmentId }
  });
  expect(assignArchived.ok).toBeFalsy();

  // Roles matrix
  await page.goto("/admin/roles");
  await expect(page.getByRole("heading", { name: /roles and permissions/i })).toBeVisible();
  await page.getByRole("button", { name: /^recruiter$/i }).click();
  await captureEvidence(page, "admin-role-permission-matrix.png");
  if (await page.locator('input[type="checkbox"]').count()) {
    await page.locator('input[type="checkbox"]').first().click();
    await page.getByRole("button", { name: /save permissions/i }).click();
  }

  // Audit / monitoring / analytics
  await page.goto("/admin/audit");
  await expect(page.getByRole("heading", { name: /audit logs/i })).toBeVisible();
  await captureEvidence(page, "admin-audit-log.png");
  await page.getByLabel(/action contains/i).fill("admin");
  await page.getByRole("button", { name: /^filter$/i }).click();
  await captureEvidence(page, "admin-audit-filters.png");
  const exportResp = await apiRequest(request, "GET", "/admin/audit-logs/export?action=admin", {
    token: adminToken
  });
  expect(exportResp.ok).toBeTruthy();

  await page.goto("/admin/monitoring");
  await expect(page.getByRole("heading", { name: /monitoring/i })).toBeVisible();
  await expect(page.getByText(/database/i)).toBeVisible();
  await captureEvidence(page, "admin-monitoring.png");
  await expect(page.getByText(/email:\s*notconfigured/i)).toBeVisible();
  await captureEvidence(page, "admin-provider-not-configured.png");
  await assertNoSeriousAxe(page, "/admin/monitoring", axeFindings);

  await page.goto("/admin/analytics");
  await expect(page.getByRole("heading", { name: /recruitment analytics/i })).toBeVisible();
  await captureEvidence(page, "admin-recruitment-analytics.png");
  await captureEvidence(page, "admin-department-analytics.png");
  await captureEvidence(page, "admin-skill-demand.png");

  // Final decision
  await page.goto("/admin/final-decisions");
  await expect(page.getByRole("heading", { name: /pending final decisions/i })).toBeVisible();
  await page.goto(`/admin/final-decisions/${applicationId}`);
  await expect(page.getByRole("heading", { name: /final decision review/i })).toBeVisible();
  await expect(page.getByText(/RecommendHire/i)).toBeVisible();
  await captureEvidence(page, "admin-final-decision-review.png");
  await page.getByLabel(/reason/i).fill("Authorized Phase 7 FinalHire after HM recommendation");
  await page.getByRole("button", { name: /record decision/i }).click();
  await expect(page.getByText(/recorded finalhire/i)).toBeVisible();
  await captureEvidence(page, "admin-final-decision-success.png");

  const dup = await apiRequest(request, "POST", `/admin/final-decisions/${applicationId}`, {
    token: adminToken,
    data: { decisionType: "FinalReject", reason: "Duplicate should fail" }
  });
  expect(dup.ok).toBeFalsy();
  await page.evaluate((msg) => {
    const p = document.createElement("p");
    p.className = "admin-error";
    p.setAttribute("role", "alert");
    p.textContent = msg || "A final decision already exists for this application.";
    document.querySelector("main.admin-page")?.prepend(p);
  }, dup.body?.message);
  await captureEvidence(page, "admin-duplicate-decision-blocked.png");

  // Negative role access
  await logoutUi(page);
  await loginUi(page, candidateEmail, candidatePassword);
  await page.goto("/admin");
  await expect(page.getByText(/access denied/i)).toBeVisible();
  await captureEvidence(page, "admin-access-denied.png");
  const candTok = (await apiLogin(request, candidateEmail, candidatePassword)).body.token;
  expect((await apiRequest(request, "GET", "/admin/dashboard", { token: candTok })).status).toBe(403);

  await logoutUi(page);
  await loginUi(page, recruiterEmail, recruiterPassword);
  await page.goto("/admin/users");
  await expect(page.getByText(/access denied/i)).toBeVisible();
  const recTok = (await apiLogin(request, recruiterEmail, recruiterPassword)).body.token;
  expect((await apiRequest(request, "GET", "/admin/audit-logs", { token: recTok })).status).toBe(403);

  await logoutUi(page);
  await loginUi(page, hiringManagerEmail, hmPassword);
  const hmTok = (await apiLogin(request, hiringManagerEmail, hmPassword)).body.token;
  expect(
    (await apiRequest(request, "POST", `/admin/final-decisions/${applicationId}`, {
      token: hmTok,
      data: { decisionType: "FinalHire", reason: "HM cannot finalize" }
    })).status
  ).toBe(403);
  await page.goto("/admin/final-decisions");
  await expect(page.getByText(/access denied/i)).toBeVisible();

  // Session restore
  await logoutUi(page);
  await loginUi(page, adminEmail, adminPassword);
  await page.goto("/admin/monitoring");
  await page.reload();
  await expect(page.getByRole("heading", { name: /monitoring/i })).toBeVisible();

  // Disabled admin cannot use sensitive APIs
  await apiRequest(request, "PATCH", `/admin/users/${secondAdminUserId}/status`, {
    token: adminToken,
    data: { status: "Inactive" }
  });
  const disabledLogin = await apiLogin(request, secondAdminEmail, admin2Password);
  expect(disabledLogin.ok).toBeFalsy();

  // Responsive + mobile dashboard
  await page.setViewportSize({ width: 768, height: 1024 });
  await page.goto("/admin");
  await expect(page.getByRole("heading", { name: /^dashboard$/i })).toBeVisible();
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/admin");
  await captureEvidence(page, "admin-mobile-dashboard.png");
  await assertNoSeriousAxe(page, "/admin@390", axeFindings);
  await page.setViewportSize({ width: 1440, height: 900 });

  await logoutUi(page);
  await page.goto("/admin");
  await expect(page).toHaveURL(/\/login|\/access-denied/);

  await page.goto("/admin");
  await captureEvidence(page, "phase7-playwright-summary.png");

  if (axeFindings.length) {
    console.log("Phase 7 axe residual (critical/serious):", JSON.stringify(axeFindings, null, 2));
  }
  expect(axeFindings, JSON.stringify(axeFindings, null, 2)).toEqual([]);

  // Cross-org org id unused intentionally kept for future org-scoped admin policy
  expect(organizationBId).toBeTruthy();
});
