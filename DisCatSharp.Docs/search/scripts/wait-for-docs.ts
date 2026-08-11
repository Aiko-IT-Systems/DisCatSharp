import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { setTimeout as delay } from "node:timers/promises";
import { createBuildId } from "../src/search-service";

interface SearchArtifact {
  generatedAt: string;
  sourceCommit: string;
}

const artifactPath = resolve(process.argv[2] ?? "../obj/search/search-index.json");
const baseUrl = (process.env.DCS_DOCS_BASE_URL ?? "https://docs.dcs.aitsys.dev").replace(/\/$/u, "");
const artifact = JSON.parse(await readFile(artifactPath, "utf8")) as SearchArtifact;
const expectedBuild = createBuildId(artifact.generatedAt, artifact.sourceCommit);
const deadline = Date.now() + 300_000;
let attempt = 0;
let lastResponse = "";

while (Date.now() < deadline) {
  attempt++;
  try {
    const response = await fetch(`${baseUrl}/search-build.json?build=${encodeURIComponent(expectedBuild)}&attempt=${attempt}`, {
      headers: { "Cache-Control": "no-cache" },
      signal: AbortSignal.timeout(10_000),
    });
    lastResponse = await response.text();
    if (response.ok && isExpectedMarker(lastResponse, artifact.sourceCommit, expectedBuild)) {
      console.log(`Documentation build '${expectedBuild}' became live after ${attempt} attempt(s).`);
      process.exit(0);
    }
  } catch (error) {
    lastResponse = error instanceof Error ? `${error.name}: ${error.message}` : "Unknown request failure.";
  }

  console.log(`Waiting for documentation build '${expectedBuild}' (attempt ${attempt})...`);
  if (Date.now() < deadline) await delay(5_000);
}

console.error(`Documentation build '${expectedBuild}' did not become live within 300 seconds.`);
console.error(`Last marker response (first 1 KiB):\n${lastResponse.slice(0, 1_024)}`);
process.exit(1);

function isExpectedMarker(body: string, sourceCommit: string, build: string): boolean {
  try {
    const marker = JSON.parse(body) as { commit?: unknown; build?: unknown };
    return marker.commit === sourceCommit && marker.build === build;
  } catch {
    return false;
  }
}
