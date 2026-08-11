import { spawnSync } from "node:child_process";
import { mkdir, readFile, rm, writeFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";

interface ArtifactSource { path: string; startLine: number; endLine: number }
interface ArtifactSymbol {
  id: string; uid: string; name: string; displayName: string; qualifiedName: string; fullName: string; kind: string;
  namespace: string | null; module: string | null; parentUid: string | null; summary: string; signature: string; content: string;
  url: string; source: ArtifactSource | null; relatedUids: string[]; contentHash: string;
}
interface ArtifactDocument {
  id: string; documentKey: string; family: string; kind: string; title: string; description: string; content: string;
  url: string; module: string | null; sourcePath: string; contentHash: string;
}
interface ArtifactChunk {
  id: string; path: string; language: string; startLine: number; endLine: number; content: string; contentHash: string;
}
interface SearchArtifact {
  schemaVersion: number; sourceCommit: string; generatedAt: string; modules: string[]; types: string[];
  symbols: ArtifactSymbol[]; documents: ArtifactDocument[]; sourceChunks: ArtifactChunk[];
}

const artifactPath = resolve(process.argv[2] ?? "../obj/search/search-index.json");
const artifact = JSON.parse(await readFile(artifactPath, "utf8")) as SearchArtifact;
if (artifact.schemaVersion !== 1) throw new Error(`Unsupported search artifact schema version ${artifact.schemaVersion}; expected 1.`);
const sqlPath = resolve("dist-local/search-seed.sql");
await mkdir(dirname(sqlPath), { recursive: true });

const statements: string[] = [
  `INSERT INTO sync_state (id, schema_version, source_commit, generated_at, completed_at, ready, symbol_count, document_count, source_chunk_count, modules_json, types_json)
   VALUES (1, ${sql(artifact.schemaVersion)}, ${sql(artifact.sourceCommit)}, ${sql(artifact.generatedAt)}, NULL, 0, 0, 0, 0, ${sql(JSON.stringify(artifact.modules))}, ${sql(JSON.stringify(artifact.types))})
   ON CONFLICT(id) DO UPDATE SET schema_version=excluded.schema_version, source_commit=excluded.source_commit, generated_at=excluded.generated_at,
   completed_at=NULL, ready=0, symbol_count=0, document_count=0, source_chunk_count=0, modules_json=excluded.modules_json, types_json=excluded.types_json;`,
  "DELETE FROM source_chunks;",
  "DELETE FROM documents;",
  "DELETE FROM symbols;",
];

for (const record of artifact.symbols) {
  statements.push(`INSERT INTO symbols (record_id, uid, name, display_name, qualified_name, full_name, kind, namespace, module, parent_uid, summary, signature, content, url, source_path, source_start_line, source_end_line, related_json, content_hash)
    VALUES (${values([record.id, record.uid, record.name, record.displayName, record.qualifiedName, record.fullName, record.kind, record.namespace,
      record.module, record.parentUid, record.summary, record.signature, record.content, record.url, record.source?.path ?? null,
      record.source?.startLine ?? null, record.source?.endLine ?? null, JSON.stringify(record.relatedUids), record.contentHash])});`);
}
for (const record of artifact.documents) {
  statements.push(`INSERT INTO documents (record_id, document_key, family, kind, title, description, content, url, module, source_path, content_hash)
    VALUES (${values([record.id, record.documentKey, record.family, record.kind, record.title, record.description, record.content, record.url,
      record.module, record.sourcePath, record.contentHash])});`);
}
for (const record of artifact.sourceChunks) {
  statements.push(`INSERT INTO source_chunks (record_id, path, language, start_line, end_line, content, content_hash)
    VALUES (${values([record.id, record.path, record.language, record.startLine, record.endLine, record.content, record.contentHash])});`);
}
statements.push(`UPDATE sync_state SET completed_at=${sql(new Date().toISOString())}, ready=1, symbol_count=${artifact.symbols.length},
  document_count=${artifact.documents.length}, source_chunk_count=${artifact.sourceChunks.length} WHERE id=1;`);

// Keep comfortably below the 4 MiB synchronization request target while
// avoiding dozens of Wrangler process startups for a first local seed.
const batches = partitionStatements(statements, 3 * 1024 * 1024);
console.log(`Seeding local D1 in ${batches.length} bounded batches...`);
try {
  for (let index = 0; index < batches.length; index++) {
    await writeFile(sqlPath, batches[index]!.join("\n"), "utf8");
    runWrangler(["d1", "execute", "discatsharp-docs-search", "--local", "--file", sqlPath], false);
    console.log(`Applied local D1 batch ${index + 1}/${batches.length}.`);
  }
  runWrangler(["d1", "execute", "discatsharp-docs-search", "--local", "--command",
    "SELECT ready, symbol_count, document_count, source_chunk_count FROM sync_state WHERE id = 1;"]);
  console.log(`Seeded local D1 from ${artifactPath}.`);
} finally {
  await rm(sqlPath, { force: true });
}

function values(items: Array<string | number | null>): string {
  return items.map(sql).join(", ");
}

function sql(value: string | number | null): string {
  if (value === null) return "NULL";
  if (typeof value === "number") {
    if (!Number.isFinite(value)) throw new Error("Cannot serialize a non-finite number to local D1.");
    return String(value);
  }
  if (value.includes("\0")) throw new Error("The search artifact contains a NUL character that cannot be seeded safely.");
  return `'${value.replaceAll("'", "''")}'`;
}

function partitionStatements(input: string[], maximumBytes: number): string[][] {
  const batches: string[][] = [];
  let batch: string[] = [];
  let bytes = 0;
  for (const statement of input) {
    const statementBytes = Buffer.byteLength(statement) + 1;
    if (statementBytes > maximumBytes) throw new Error(`A local D1 statement exceeds the ${maximumBytes}-byte batch limit.`);
    if (batch.length > 0 && bytes + statementBytes > maximumBytes) {
      batches.push(batch);
      batch = [];
      bytes = 0;
    }
    batch.push(statement);
    bytes += statementBytes;
  }
  if (batch.length > 0) batches.push(batch);
  return batches;
}

function runWrangler(args: string[], inheritOutput = true): void {
  const executable = resolve("node_modules/wrangler/bin/wrangler.js");
  const result = spawnSync(process.execPath, [executable, ...args], { stdio: inheritOutput ? "inherit" : "pipe", windowsHide: true, encoding: "utf8" });
  if (result.error !== undefined) throw result.error;
  if (result.status !== 0) {
    const details = inheritOutput ? "" : `\n${result.stderr || result.stdout}`;
    throw new Error(`Wrangler exited with code ${result.status ?? "unknown"}.${details}`);
  }
}
