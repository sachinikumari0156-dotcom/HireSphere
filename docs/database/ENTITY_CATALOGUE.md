# Entity catalogue

**Source:** `Backend/HireSphere.API/Data/ApplicationDbContext.cs`

| Entity | Purpose |
|--------|---------|
| User | Account credentials, status, profile linkage |
| Role / Permission / UserRole / RolePermission | RBAC |
| Organization / Department | Multi-org structure |
| RecruiterProfile / HiringManagerProfile / CandidateProfile | Role profiles |
| WorkExperience / Education / CandidateSkill / Certification | Candidate CV sections |
| Skill / JobSkill | Shared skill taxonomy |
| Resume / CandidateDocument / ResumeAnalysis / ExtractedSkill | Documents and AI parse artefacts |
| Job / ScreeningQuestion | Vacancies |
| Application / ApplicationAnswer / ApplicationStatusHistory / ApplicationNote / ApplicationMessage | Applications and communication |
| JobReviewComment / RankingReview | Human review of AI ranking |
| SkillAssessment / AssessmentQuestion / AssessmentAssignment / AssessmentAttempt / AssessmentAnswer / AssessmentResult | Assessments |
| Interview / InterviewParticipant / InterviewFeedback | Interviews |
| CandidateEvaluation / HiringDecision | Manager evaluation and decisions |
| CandidateJobMatch / AIInsight | Matching and insights |
| Notification / NotificationOutbox / UserNotificationPreference | Messaging |
| AuditLog | Security and governance audit |
| RecruiterAccessRequest | Recruiter onboarding requests |

Entity names match code; do not invent tables not present in DbContext.
