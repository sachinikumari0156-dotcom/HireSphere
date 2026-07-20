# Automated test inventory — Phase 10.1

**Date:** 2026-07-21  
**Backend:** 130+ xUnit tests (includes Phase10QualityTests + MigrationVerificationTests)  
**Frontend Vitest:** 84 baseline + resilience suite  
**Playwright:** 13 journeys  

| Area | Level | Automated | Evidence |
|------|-------|-----------|----------|
| Auth register/login/me | BE unit/integration | Yes | AuthControllerTests |
| Role Forbidden matrix | BE + FE | Yes | Auth + portal + auth.test.jsx |
| Candidate/Recruiter/HM/Admin portals | BE + E2E | Yes | Phase*Tests + e2e specs |
| AI deterministic | BE + E2E | Yes | AiPortalPhase81 + phase8 |
| Integrations outbox/ICS | BE + E2E | Yes | IntegrationsPhase82 + phase8 |
| Storage validation | BE + E2E | Yes | StoragePhase83 + phase8 |
| CSV formula escape | BE unit | Yes | Phase10QualityTests |
| Performance smoke | BE | Yes | Phase10QualityTests (&lt;1.5s local) |
| Swagger OpenAPI | BE | Yes | Phase10QualityTests |
| Migrations LocalDB | BE + script | Yes | MigrationVerificationTests, verify-migrations.ps1 |
| UI a11y/responsive/visual | E2E | Yes | phase9 specs |
| Postman | Manual/API | Collection only | postman/ |

Requirement IDs map through `docs/audit/COURSEWORK_REQUIREMENT_MATRIX.md`.
