# Screenshot Index

## Phase 10 — Quality / UAT

**Date:** 2026-07-21
**Folder:** `docs/evidence/phase10-quality/`
**Source:** Playwright Phase 10 UAT evidence + measured summary pages

| File | Scenario |
|------|----------|
| `public-uat-summary.png` | Public landing |
| `candidate-uat-summary.png` | Candidate dashboard |
| `recruiter-uat-summary.png` | Recruiter dashboard |
| `manager-uat-summary.png` | Hiring Manager dashboard |
| `admin-uat-summary.png` | Administrator dashboard |
| `cross-role-denial.png` | Candidate denied Recruiter |
| `ai-provider-results.png` | Integrations / AI status UI |
| `notification-provider-results.png` | Integrations |
| `calendar-provider-results.png` | Integrations |
| `storage-provider-results.png` | Storage status |
| `migration-verification.png` | Migration summary (measured) |
| `swagger-verification.png` | Swagger UI |
| `postman-run-summary.png` | Postman collection note |
| `backend-test-summary.png` | Backend 132 PASS summary |
| `frontend-test-summary.png` | Frontend Vitest summary |
| `playwright-summary.png` | Admin capture |
| `accessibility-summary.png` | Regression capture |
| `security-verification-summary.png` | Security capture |
| `dependency-audit-summary.png` | Audit capture |
| `performance-smoke-summary.png` | Perf capture |
| `defect-log-summary.png` | Defect disposition capture |
| `usability-task-sheet.png` | Usability pending note |
| `heuristic-evaluation-summary.png` | Heuristic capture |
| `responsive-regression-summary.png` | Responsive capture |

## Phase 9 — Responsive UI / Accessibility

**Date:** 2026-07-21
**Folder:** `docs/evidence/phase9-ui/`
**Source:** Playwright Chromium live run (`e2e/phase9-ui.spec.js`)

| File | Scenario |
|------|----------|
| `public-desktop.png` | Landing 1440×900 |
| `public-mobile.png` | Landing 390×844 |
| `login-desktop.png` | Login desktop |
| `login-mobile.png` | Login mobile |
| `candidate-dashboard-desktop.png` | Candidate dashboard desktop |
| `candidate-dashboard-tablet.png` | Candidate dashboard tablet |
| `candidate-dashboard-mobile.png` | Candidate dashboard mobile |
| `candidate-job-filters-mobile.png` | Candidate jobs mobile |
| `candidate-application-mobile.png` | Applications mobile |
| `candidate-assessment-mobile.png` | Assessments mobile |
| `recruiter-dashboard-desktop.png` | Recruiter dashboard |
| `recruiter-dashboard-mobile.png` | Recruiter mobile |
| `recruiter-pipeline-desktop.png` | Pipeline desktop |
| `recruiter-pipeline-mobile.png` | Pipeline mobile |
| `recruiter-job-form-mobile.png` | Job form mobile |
| `manager-dashboard-desktop.png` | Hiring Manager dashboard |
| `manager-dashboard-mobile.png` | Hiring Manager mobile |
| `manager-evaluation-mobile.png` | Evaluation / vacancies mobile |
| `admin-dashboard-desktop.png` | Administrator dashboard |
| `admin-dashboard-mobile.png` | Administrator mobile |
| `admin-user-list-mobile.png` | Users mobile |
| `admin-role-matrix-tablet.png` | Roles tablet |
| `admin-monitoring-mobile.png` | Monitoring |
| `accessible-dialog-focus.png` | Mobile menu focus |
| `keyboard-focus-visible.png` | Keyboard focus |
| `accessible-form-errors.png` | Login validation errors |
| `chart-accessible-summary.png` | Analytics summary |
| `provider-not-configured-state.png` | Integrations Not Configured |
| `axe-summary.png` | End-of-a11y capture |
| `responsive-playwright-summary.png` | Responsive summary capture |
| `visual-regression-summary.png` | Visual summary capture |
| `mobile-navigation.png` | Public mobile nav open |
| `mobile-dashboard.png` | Candidate mobile dashboard alias |

## Phase 8 — AI, Integrations, Calendar, Storage

**Date:** 2026-07-21  
**Folder:** `docs/evidence/phase8-platform/`  
**Source:** Playwright Chromium live run against LocalDB `HireSphereDev`

| File | Scenario |
|------|----------|
| `ai-resume-upload.png` | Candidate resume upload |
| `ai-resume-processing.png` | Resume analysis page before/at parse |
| `ai-resume-analysis.png` | Completed/extracted analysis |
| `ai-extracted-skills-review.png` | Extracted skills review |
| `ai-skill-confirmation.png` | Skill accept/confirm control |
| `ai-job-match-explanation.png` | Job match explanation surface |
| `ai-job-recommendations.png` | Recommendations |
| `recruiter-ranking.png` | Recruiter ranking |
| `recruiter-ranking-explanation.png` | Ranking explanation |
| `recruiter-human-override.png` | Human review/override UI |
| `ai-provider-status.png` | Admin AI/provider status |
| `notification-preferences.png` | Notification preferences |
| `email-mailhog-delivery.png` | Integrations board (SMTP NotConfigured without MailHog) |
| `sms-development-mock.png` | Preferences / SMS consent area |
| `notification-delivery-status.png` | Delivery preference surface |
| `notification-failed-retry.png` | Admin failed-delivery area |
| `interview-ics-download.png` | Interview ICS controls |
| `calendar-provider-status.png` | Calendar sync status |
| `google-calendar-not-configured.png` | Google NotConfigured on dashboard |
| `outlook-calendar-not-configured.png` | Outlook NotConfigured on dashboard |
| `candidate-secure-documents.png` | Candidate documents/resumes |
| `invalid-document-rejected.png` | Executable upload rejected |
| `authorized-document-download.png` | Admin monitoring (authorized context) |
| `unauthorized-document-blocked.png` | Unauthorized download blocked context |
| `storage-provider-status.png` | Storage provider status |
| `storage-migration-dry-run.png` | Migration dry-run result |
| `antivirus-not-configured.png` | Antivirus NotConfigured |
| `admin-integration-dashboard.png` | Admin integrations dashboard |
| `phase8-mobile-view.png` | Mobile 390×844 integrations |
| `phase8-playwright-summary.png` | End-of-journey capture |


No passwords, JWTs, API keys, OAuth tokens, SAS tokens, storage keys, or raw prompts are present.

## Phase 7 — Administrator Portal

**Date:** 2026-07-20
**Folder:** `docs/evidence/phase7-admin/`
**Source:** Playwright Chromium live browser run against LocalDB `HireSphereDev`

| File | Scenario |
|------|----------|
| `admin-login.png` | Administrator login |
| `admin-dashboard.png` | Live governance metrics |
| `admin-user-list.png` | User list / filters |
| `admin-user-detail.png` | User detail |
| `admin-user-role-assignment.png` | Role / org assignment UI |
| `admin-self-disable-blocked.png` | Self-disable blocked |
| `admin-last-admin-protection.png` | Last-Administrator protection |
| `admin-recruiter-requests.png` | Recruiter request list |
| `admin-recruiter-approval.png` | Approve request |
| `admin-recruiter-rejection.png` | Reject with reason |
| `admin-organization-list.png` | Organizations |
| `admin-organization-form.png` | Create organization |
| `admin-department-list.png` | Departments |
| `admin-department-form.png` | Create department |
| `admin-role-permission-matrix.png` | Roles / permissions |
| `admin-audit-log.png` | Audit logs |
| `admin-audit-filters.png` | Audit filters |
| `admin-monitoring.png` | Monitoring summary |
| `admin-provider-not-configured.png` | Phase 8 providers NotConfigured |
| `admin-recruitment-analytics.png` | Recruitment analytics |
| `admin-department-analytics.png` | Department analytics view |
| `admin-skill-demand.png` | Skill demand |
| `admin-final-decision-review.png` | Final decision review |
| `admin-final-decision-success.png` | FinalHire recorded |
| `admin-duplicate-decision-blocked.png` | Duplicate final blocked |
| `admin-access-denied.png` | Candidate denied `/admin` |
| `admin-mobile-dashboard.png` | Mobile 390×844 |
| `phase7-playwright-summary.png` | End-of-journey capture |

No passwords, JWTs, connection strings, or assessment answer keys are present in these images.

---

## Phase 6 — Hiring Manager Portal

**Date:** 2026-07-20
**Folder:** `docs/evidence/phase6-hiring-manager/`
**Source:** Playwright Chromium live browser run against LocalDB `HireSphereDev`

| File | Scenario |
|------|----------|
| `manager-login.png` | Hiring Manager login |
| `manager-dashboard.png` | Live assigned-vacancy metrics |
| `manager-assigned-vacancies.png` | Assigned vacancy list |
| `manager-vacancy-detail.png` | Vacancy detail (read-only job fields) |
| `manager-candidate-list.png` | Candidates for assigned vacancy |
| `manager-candidate-review.png` | Candidate review workspace |
| `manager-resume-review.png` | Resume metadata (no absolute paths) |
| `manager-ranking-explanation.png` | Ranking + human-review notice |
| `manager-candidate-comparison.png` | Same-vacancy comparison |
| `manager-interview-list.png` | Assigned interviews |
| `manager-interview-detail.png` | Interview detail (timezone + response) |
| `manager-feedback-form.png` | Structured feedback form |
| `manager-feedback-submitted.png` | Feedback saved |
| `manager-evaluation-draft.png` | Evaluation Draft |
| `manager-evaluation-submitted.png` | Evaluation Submitted |
| `manager-recommendation.png` | Recommendation form |
| `manager-decision-history.png` | Decision history (`isFinal=false`) |
| `manager-unassigned-access-denied.png` | Unassigned HM denied |
| `manager-candidate-private-comments-hidden.png` | Candidate UI without private panel text |
| `manager-mobile-dashboard.png` | Mobile 390Ã—844 dashboard |
| `phase6-playwright-summary.png` | End-of-journey capture |

No passwords, JWTs, connection strings, or assessment answer keys are present in these images.

---

## Phase 5 â€” Recruiter Portal

**Date:** 2026-07-20
**Folder:** `docs/evidence/phase5-recruiter/`
**Source:** Playwright Chromium live browser run against LocalDB `HireSphereDev`

| File | Scenario |
|------|----------|
| `recruiter-request.png` | Recruiter access request page |
| `admin-recruiter-approval.png` | Admin area after login (seeded approval path) |
| `recruiter-dashboard.png` | Recruiter dashboard live metrics |
| `recruiter-job-list.png` | Job list |
| `recruiter-create-job.png` | Create job form |
| `recruiter-job-skills.png` | Required/preferred skills |
| `recruiter-screening-questions.png` | Screening questions on job |
| `recruiter-published-job.png` | Published job detail |
| `recruiter-applicant-pipeline.png` | Applicant pipeline |
| `recruiter-application-detail.png` | Application review |
| `recruiter-candidate-comparison.png` | Candidate comparison |
| `recruiter-ranking-explanation.png` | Ranking + human-review notice |
| `recruiter-screening.png` | Screening queue |
| `recruiter-assessment-builder.png` | Assessment builder (keys recruiter-only) |
| `recruiter-assessment-assignment.png` | Assessment assignment context |
| `recruiter-assessment-result.png` | Recruiter result review |
| `recruiter-message-thread.png` | Application message thread |
| `recruiter-interview-schedule.png` | Schedule interview form |
| `recruiter-conflict-warning.png` | Interview conflict warning |
| `recruiter-interview-status.png` | Interview detail/status |
| `recruiter-reports.png` | Reports dashboard |
| `recruiter-report-filters.png` | Report filters applied |
| `recruiter-csv-export.png` | CSV export action |
| `recruiter-mobile-dashboard.png` | Mobile 390Ã—844 dashboard |
| `recruiter-access-denied.png` | Candidate denied recruiter route |
| `phase5-playwright-summary.png` | End-of-journey capture |

No passwords, JWT values, connection strings, or assessment answer keys intended for Candidate are present in these images.

---

## Phase 4 â€” Candidate Portal

**Date:** 2026-07-20
**Folder:** `docs/evidence/phase4-candidate/`
**Source:** Playwright Chromium live browser run against LocalDB `HireSphereDev`

| File | Scenario |
|------|----------|
| `registration-validation.png` | Empty registration validation messages |
| `candidate-login.png` | Candidate login form filled (password not disclosed in docs) |
| `candidate-dashboard.png` | Candidate dashboard |
| `candidate-profile.png` | Profile after summary/contact update |
| `candidate-experience-education.png` | Experience + education entries |
| `candidate-skills-certifications.png` | Skills + certification |
| `candidate-resume-upload.png` | Resume metadata after PDF upload |
| `candidate-invalid-upload.png` | Rejected executable upload |
| `candidate-job-search.png` | Job keyword search |
| `candidate-job-filters.png` | Location / employment / arrangement filters |
| `candidate-match-explanation.png` | Match score, skills, human-review notice |
| `candidate-recommendations.png` | Recommendations list |
| `candidate-application-wizard.png` | Application wizard |
| `candidate-application-success.png` | Submitted application detail |
| `candidate-duplicate-application.png` | Duplicate apply blocked |
| `candidate-application-timeline.png` | Application status timeline |
| `candidate-assessment.png` | Assigned assessment |
| `candidate-assessment-result.png` | Server-calculated result (no answer keys) |
| `candidate-interview.png` | Scheduled interview detail |
| `candidate-notifications.png` | In-app notifications |
| `candidate-access-denied.png` | Candidate denied admin/recruiter areas |
| `candidate-mobile-dashboard.png` | Mobile 390Ã—844 dashboard |
| `playwright-summary.png` | Desktop profile responsive capture |

No passwords, JWT values, or connection strings are present in these images.
