using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<RecruiterProfile> RecruiterProfiles => Set<RecruiterProfile>();
    public DbSet<HiringManagerProfile> HiringManagerProfiles => Set<HiringManagerProfile>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<CandidateDocument> CandidateDocuments => Set<CandidateDocument>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobSkill> JobSkills => Set<JobSkill>();
    public DbSet<ScreeningQuestion> ScreeningQuestions => Set<ScreeningQuestion>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ApplicationAnswer> ApplicationAnswers => Set<ApplicationAnswer>();
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories => Set<ApplicationStatusHistory>();
    public DbSet<ApplicationNote> ApplicationNotes => Set<ApplicationNote>();
    public DbSet<JobReviewComment> JobReviewComments => Set<JobReviewComment>();
    public DbSet<ApplicationMessage> ApplicationMessages => Set<ApplicationMessage>();
    public DbSet<RankingReview> RankingReviews => Set<RankingReview>();
    public DbSet<SkillAssessment> SkillAssessments => Set<SkillAssessment>();
    public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();
    public DbSet<AssessmentAssignment> AssessmentAssignments => Set<AssessmentAssignment>();
    public DbSet<AssessmentAttempt> AssessmentAttempts => Set<AssessmentAttempt>();
    public DbSet<AssessmentAnswer> AssessmentAnswers => Set<AssessmentAnswer>();
    public DbSet<AssessmentResult> AssessmentResults => Set<AssessmentResult>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<InterviewParticipant> InterviewParticipants => Set<InterviewParticipant>();
    public DbSet<InterviewFeedback> InterviewFeedbacks => Set<InterviewFeedback>();
    public DbSet<CandidateEvaluation> CandidateEvaluations => Set<CandidateEvaluation>();
    public DbSet<HiringDecision> HiringDecisions => Set<HiringDecision>();
    public DbSet<CandidateJobMatch> CandidateJobMatches => Set<CandidateJobMatch>();
    public DbSet<ResumeAnalysis> ResumeAnalyses => Set<ResumeAnalysis>();
    public DbSet<AIInsight> AIInsights => Set<AIInsight>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RecruiterAccessRequest> RecruiterAccessRequests => Set<RecruiterAccessRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
