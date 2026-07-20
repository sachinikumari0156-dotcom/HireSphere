# HireSphere — Data Dictionary

**Last updated:** 2026-07-20 (Phase 2)
**Provider:** Microsoft SQL Server (EF Core 8.0.11)
**Schema source:** `Backend/HireSphere.API/Models` + `Data/Configurations`

---

## Privacy and sensitive fields

| Field | Table | Classification | API exposure |
|-------|-------|----------------|--------------|
| `PasswordHash` | Users | Secret | Never returned in DTOs; BCrypt hashed on create/update/register |
| `NormalizedEmail` | Users | Internal | Used for uniqueness; not exposed in `UserDto` |
| `ResumePath` | CandidateProfiles, Resumes | PII / document reference | Controlled by profile/resume APIs (Phase 4+) |
| `PhoneNumber`, `Address` | CandidateProfiles | PII | Profile APIs only |
| `MeetingLink` | Interviews | Operational | Interview APIs only |
| JWT signing key | Configuration | Secret | Environment / user-secrets only |

---

## Identity and access

### Users

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | int | PK, identity | |
| FullName | nvarchar(200) | required | |
| Email | nvarchar(256) | required, indexed | Display email |
| NormalizedEmail | nvarchar(256) | required, **unique** | Uppercase normalized lookup |
| PasswordHash | nvarchar(500) | required | BCrypt hash |
| Role | nvarchar(50) | required | Legacy string role (RBAC expansion in Phase 3) |
| Status | enum | required | Active, Inactive, PendingApproval, Suspended |
| CreatedAtUtc | datetime2 | required | |
| UpdatedAtUtc | datetime2 | nullable | |

**FKs:** One-to-one optional profiles (CandidateProfile, RecruiterProfile, HiringManagerProfile); one-to-many Jobs (RecruiterId, Restrict), Applications (CandidateId, Restrict)

### Roles

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| Name | nvarchar | **unique** |
| Description | nvarchar | nullable |

### Permissions

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| Code | nvarchar | **unique** |
| Name | nvarchar | required |

### UserRoles

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| UserId | int | FK → Users, cascade delete |
| RoleId | int | FK → Roles, restrict delete |

**Unique:** (UserId, RoleId)

### RolePermissions

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| RoleId | int | FK → Roles, cascade |
| PermissionId | int | FK → Permissions, restrict |

**Unique:** (RoleId, PermissionId)

---

## Organization structure

### Organizations

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| Name | nvarchar | required |
| Description | nvarchar | nullable |
| Website | nvarchar | nullable |

### Departments

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| OrganizationId | int | FK → Organizations, restrict |
| Name | nvarchar | required |

---

## Profiles

### CandidateProfiles

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| UserId | int | FK → Users, cascade, **unique** (one profile per user) |
| FullName | nvarchar(200) | required |
| PhoneNumber | nvarchar(50) | required |
| Address | nvarchar(500) | required |
| Skills | nvarchar(4000) | required |
| ResumePath | nvarchar(500) | required |
| Summary | nvarchar(4000) | nullable |
| Location | nvarchar(200) | nullable |
| YearsOfExperience | int | nullable |

**Child tables (cascade delete):** WorkExperiences, Educations, CandidateSkills, Certifications, Resumes, CandidateDocuments

### RecruiterProfiles / HiringManagerProfiles

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| UserId | int | FK → Users, cascade, **unique** |
| OrganizationId | int | FK → Organizations, set null |
| DepartmentId | int | FK → Departments, set null |

---

## Skills and matching

### Skills

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| Name | nvarchar | **unique** |

### CandidateSkills

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| CandidateProfileId | int | FK, cascade |
| SkillId | int | FK → Skills, restrict |

**Unique:** (CandidateProfileId, SkillId)

### JobSkills

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| JobId | int | FK → Jobs, cascade |
| SkillId | int | FK → Skills, restrict |
| MinProficiencyLevel | nvarchar(50) | |

**Unique:** (JobId, SkillId)

### CandidateJobMatches

Links candidates to recommended jobs (AI matching — Phase 8). FKs to CandidateProfile and Job (restrict).

---

## Jobs and applications

### Jobs

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| Title | nvarchar(200) | indexed |
| Description | nvarchar(4000) | |
| RequiredSkills | nvarchar(4000) | |
| Location | nvarchar(200) | |
| JobType | nvarchar(50) | |
| PostedDate | datetime2 | |
| RecruiterId | int | FK → Users, restrict |
| OrganizationId | int | FK, set null |
| DepartmentId | int | FK, set null |
| Status | enum | indexed |
| EmploymentType | enum | |
| WorkArrangement | enum | |

**Delete behavior:** JobSkills, ScreeningQuestions cascade; **Applications restrict** (history preserved)

### Applications

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| CandidateId | int | FK → Users, restrict |
| JobId | int | FK → Jobs, restrict |
| AppliedDate | datetime2 | |
| Status | enum | indexed |
| CoverLetter | nvarchar(4000) | |

**Unique:** (CandidateId, JobId)

**Children:** ApplicationAnswers (cascade), ApplicationStatusHistory (cascade), Interviews via Application (restrict on delete)

### ApplicationStatusHistory

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| ApplicationId | int | FK, cascade |
| ChangedByUserId | int | FK → Users, set null |
| Notes | nvarchar(4000) | nullable |

---

## Interviews and hiring

### Interviews

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK |
| ApplicationId | int | FK → Applications, **restrict** |
| InterviewDate | datetime2 | |
| InterviewType | nvarchar(100) | |
| MeetingLink | nvarchar(500) | |
| Status | enum | |

**Children:** InterviewParticipants, InterviewFeedbacks (cascade)

### CandidateEvaluations / HiringDecisions

Evaluation and decision records with FKs to Application, Job, and evaluator users (mostly restrict delete).

---

## Assessments

### SkillAssessments → AssessmentQuestions → AssessmentAttempts → AssessmentResults

Assessment pipeline for candidate skill verification. AssessmentResult has **unique** AssessmentAttemptId (one result per attempt).

---

## AI, documents, audit

### ResumeAnalyses / AIInsights

AI output linked to resumes, applications, or profiles. Nullable FKs with set-null delete where configured.

### Notifications

User notifications; cascade delete with user where configured.

### AuditLogs

Immutable-style audit trail; FK to Users with restrict delete.

---

## Seed data

Development seed data is applied by `DbSeeder` when SQL Server is reachable. Demo credentials are documented only in comments inside `DbSeeder.cs` (not repeated here).
