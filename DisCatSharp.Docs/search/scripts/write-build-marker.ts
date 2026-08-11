import { readFile, writeFile } from "node:fs/promises";
import { resolve } from "node:path";
import { createBuildId } from "../src/search-service";

interface SearchArtifact {
  generatedAt: string;
  sourceCommit: string;
}

const artifactPath = resolve(process.argv[2] ?? "../obj/search/search-index.json");
const markerPath = resolve(process.argv[3] ?? "../_site/search-build.json");
const artifact = JSON.parse(await readFile(artifactPath, "utf8")) as SearchArtifact;
const marker = {
  build: createBuildId(artifact.generatedAt, artifact.sourceCommit),
  commit: artifact.sourceCommit,
};

await writeFile(markerPath, JSON.stringify(marker) + "\n", "utf8");
console.log(JSON.stringify({ marker: markerPath, ...marker }));
