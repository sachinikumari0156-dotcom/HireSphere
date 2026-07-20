# Resume parsing

- Formats: PDF, DOCX
- Ownership: Candidate may only parse their own resumes
- Extracted fields stored on `ResumeAnalysis` / `ExtractedSkill` — profile skills change only after explicit confirm
- Provider: Deterministic by default; External AI only with consent **and** configuration
- Prompt-injection: document text is sanitized; fixed system behavior; output schema is controlled server-side
- Logs omit full resume text and secrets
