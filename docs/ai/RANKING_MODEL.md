# Deterministic Recruiter Ranking Model

**Provider:** `Deterministic` (`DeterministicJobMatchingProvider` + recruiter ranking wrapper)  
**Model/version:** `recruiter-rank-v1`  
**Last updated:** 2026-07-20

## Notice

> AI-generated insight. Final recruitment decisions must be reviewed by authorized users.

This is a **rules-based deterministic** scorer. It is not an external AI provider call unless a separately configured provider succeeds (none in Phase 5).

## Factors and approximate weights

| Factor | Contribution | Notes |
|--------|--------------|-------|
| Required skills match | up to ~45 | Ratio of matched required job skills |
| Preferred skills match | up to ~10 | Ratio of matched preferred skills |
| Experience | variable | From years of experience / work history |
| Education | variable | From education history count/quality heuristics |
| Location / work arrangement | variable | Soft preference factors |
| Assessment (when completed) | up to ~15 additive display factor | From latest completed attempt percent |
| Profile completeness | up to ~10 | Summary, skills, education, experience presence |

Exact job-match totals are clamped to 0–100 by the deterministic matcher.

## Missing data

- Missing years of experience lowers confidence and experience factor.
- Missing skills increase missing-required list; score is not used alone to reject.
- No assessment attempt → assessment factor omitted.

## Fairness and prohibited inputs

The ranking **does not** use gender, race, religion, disability, marital status, or age.

## Human override

Authorized recruiters can record an audited `RankingReview` (decision + notes + optional override score). Audit action: `RankingHumanReview`.

## Limitations

- Not a hiring decision.
- Sensitive to incomplete profiles and skill vocabulary mismatch.
- Assessment factor only appears after a completed attempt.
- External LLM/vendor ranking is out of scope for Phase 5.
