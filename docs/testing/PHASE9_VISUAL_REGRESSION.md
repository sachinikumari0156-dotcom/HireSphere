# Phase 9 visual regression

**Date:** 2026-07-21  
**Spec:** `Frontend/e2e/phase9-visual.spec.js`  
**Snapshots:** `Frontend/e2e/phase9-visual.spec.js-snapshots/`  

## Scope

- Public landing hero
- Login card
- Candidate / Recruiter / Hiring Manager / Administrator dashboards

## Threshold

`maxDiffPixelRatio` 0.02–0.03 (small, defensible).

## Update process

```
npx playwright test e2e/phase9-visual.spec.js --update-snapshots
```

Review diff before committing. Do not raise thresholds to hide layout defects.

## Result

**PASS** on Chromium for documented snapshots (2026-07-21).
