# HireSphere Postman Collection Guide

**Date:** 2026-07-21  
**Collection:** `postman/HireSphere.postman_collection.json`  
**Environment example:** `postman/HireSphere.local.example.postman_environment.json`

## Setup

1. Import the collection and environment into Postman.
2. Copy the example environment and set `baseUrl` (default `http://localhost:5167/api`).
3. Set role emails/passwords via your local ignored secrets — **never commit real passwords**.
4. Run **Authentication → Login Admin** (or Candidate/Recruiter/HM) to populate `accessToken`.

## Folders

- Authentication
- Candidate
- Recruiter
- Hiring Manager
- Administrator
- AI
- Notifications
- Calendar
- Storage
- Health

## Notes

- Bearer token uses `{{accessToken}}`.
- Provider endpoints report truthful NotConfigured statuses when credentials are absent.
- Do not export environments containing live tokens into git.
