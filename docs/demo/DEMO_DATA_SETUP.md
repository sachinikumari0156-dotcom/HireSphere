# Demo data setup

## Principles

- Idempotent seed via enabled seed flags / E2E seed **only on local machines**
- Generated test identities preferred
- **Do not commit passwords**

## Suggested local approach

1. Enable seed through environment variables or user secrets (see README).  
2. Or use `api/e2e` seed endpoints when `HIRESPHERE_E2E_SEED_ENABLED` is true in Development.  
3. Create one org with Recruiter + Hiring Manager + Admin + Candidate.  
4. Publish one job, one application, one assessment assignment, one interview as needed for the script.

## Cleanup

Prefer API-driven cleanup or disposable LocalDB database recreate — avoid hand-editing production-like data.
