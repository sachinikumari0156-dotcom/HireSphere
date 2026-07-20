/** User-friendly labels for application / job lifecycle statuses shown in UI. */
const LABELS = {
  Pending: "Pending",
  UnderReview: "Under review",
  ManualReview: "Manual review",
  Assessment: "Assessment",
  Shortlisted: "Shortlisted",
  InterviewScheduled: "Interview scheduled",
  Rejected: "Rejected",
  Withdrawn: "Withdrawn",
  Draft: "Draft",
  Published: "Published",
  Paused: "Paused",
  Closed: "Closed",
  Active: "Active",
  Inactive: "Inactive",
  Suspended: "Suspended",
  PendingApproval: "Pending approval",
  NotConfigured: "Not configured",
  Healthy: "Healthy",
  Degraded: "Degraded",
  Failed: "Failed",
  Submitted: "Submitted"
};

export function friendlyStatus(value) {
  if (value == null || value === "") return "—";
  return LABELS[value] || String(value).replace(/([a-z])([A-Z])/g, "$1 $2");
}
