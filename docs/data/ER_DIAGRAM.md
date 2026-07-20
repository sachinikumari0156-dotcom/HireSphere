# HireSphere — Entity Relationship Diagram

**Last updated:** 2026-07-20 (Phase 2)
**Scope:** Core implemented entities in `ApplicationDbContext`

```mermaid
erDiagram
    Users ||--o| CandidateProfiles : "has"
    Users ||--o| RecruiterProfiles : "has"
    Users ||--o| HiringManagerProfiles : "has"
    Users ||--o{ UserRoles : "assigned"
    Roles ||--o{ UserRoles : "includes"
    Roles ||--o{ RolePermissions : "grants"
    Permissions ||--o{ RolePermissions : "referenced"

    Organizations ||--o{ Departments : "contains"
    Organizations ||--o{ Jobs : "posts"
    Departments ||--o{ Jobs : "owns"
    Users ||--o{ Jobs : "recruits"

    CandidateProfiles ||--o{ WorkExperiences : "has"
    CandidateProfiles ||--o{ Educations : "has"
    CandidateProfiles ||--o{ CandidateSkills : "has"
    CandidateProfiles ||--o{ Certifications : "has"
    CandidateProfiles ||--o{ Resumes : "has"
    CandidateProfiles ||--o{ CandidateDocuments : "has"
    Skills ||--o{ CandidateSkills : "tagged"
    Skills ||--o{ JobSkills : "required"

    Jobs ||--o{ JobSkills : "requires"
    Jobs ||--o{ ScreeningQuestions : "asks"
    Jobs ||--o{ Applications : "receives"
    Users ||--o{ Applications : "submits"

    Applications ||--o{ ApplicationAnswers : "answers"
    Applications ||--o{ ApplicationStatusHistory : "tracks"
    Applications ||--o{ Interviews : "schedules"
    Interviews ||--o{ InterviewParticipants : "includes"
    Interviews ||--o{ InterviewFeedback : "collects"

    Applications ||--o{ CandidateEvaluations : "evaluated"
    Applications ||--o{ HiringDecisions : "decided"
    Jobs ||--o{ CandidateJobMatches : "matched"
    CandidateProfiles ||--o{ CandidateJobMatches : "recommended"

    SkillAssessments ||--o{ AssessmentQuestions : "contains"
    SkillAssessments ||--o{ AssessmentAttempts : "attempted"
    AssessmentAttempts ||--o| AssessmentResults : "produces"

    Resumes ||--o{ ResumeAnalyses : "analyzed"
    Applications ||--o{ AIInsights : "insights"
    Users ||--o{ Notifications : "notified"
    Users ||--o{ AuditLogs : "audited"
```

## Delete-behavior highlights (Phase 2)

| Relationship | On delete | Rationale |
|--------------|-----------|-----------|
| Job → Applications | **Restrict** | Preserve application history if job retired |
| Application → Interviews | **Restrict** | Preserve interview records |
| Application → StatusHistory | Cascade | History removed only when application hard-deleted |
| User → CandidateProfile | Cascade | Profile is owned by user account |

## Diagram notes

- String `Users.Role` coexists with normalized `Roles` / `UserRoles` during RBAC transition (Phase 3).
- Optional org/department FKs on jobs and staff profiles support multi-tenant expansion.
- AI and assessment subgraphs are modeled but not fully exposed via API until later phases.
