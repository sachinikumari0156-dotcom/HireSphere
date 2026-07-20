# Report generation notes

**Date:** 2026-07-21  

| Output | Path | Status |
|--------|------|--------|
| Markdown source | `docs/report/HIRESPHERE_FINAL_REPORT.md` | COMPLETE |
| DOCX | `artifacts/report/HireSphere_Final_Coursework_Report.docx` | GENERATED locally via Microsoft Word (plain InsertFile from Markdown; ~12 pages) |
| PDF | `artifacts/report/HireSphere_Final_Coursework_Report.pdf` | GENERATED locally via Word ExportAsFixedFormat |

`artifacts/` remains gitignored (Visual Studio gitignore). Files are included in the local submission ZIP when `scripts/build-submission-package.ps1` runs.

**Formatting caveat:** Word import of Markdown is not fully typeset (headings/code may need manual polish before final hand-in). User should open DOCX/PDF and adjust title-page placeholders.

**Placeholders still requiring user input:** Student ID, lecturer/module details, submission date.
