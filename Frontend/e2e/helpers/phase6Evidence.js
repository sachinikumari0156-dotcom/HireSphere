import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const evidenceDir = path.resolve(
  __dirname,
  "../../../docs/evidence/phase6-hiring-manager"
);

export function ensureEvidenceDir() {
  fs.mkdirSync(evidenceDir, { recursive: true });
  return evidenceDir;
}

export async function captureEvidence(page, filename) {
  ensureEvidenceDir();
  const target = path.join(evidenceDir, filename);
  await page.screenshot({ path: target, fullPage: true });
  return target;
}
