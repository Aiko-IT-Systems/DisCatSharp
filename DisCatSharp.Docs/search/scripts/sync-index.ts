import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { setTimeout as delay } from "node:timers/promises";
import { createActivationStatements, isSameRelease, type ReleaseStatement, type StagedReleaseState } from "../src/release-sync";
import { diffHashes, toD1JsonScalar, toD1Scalar, type ExistingHashRow } from "../src/sync-plan";

type Scalar = string;
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
interface ApiEnvelope<T> { success: boolean; result: T; errors: Array<{ message: string }> }
interface QueryResult { results?: Array<Record<string, unknown>>; meta?: { rows_written?: number; rows_read?: number }; success?: boolean }
interface Statement extends ReleaseStatement { params?: Scalar[] }
interface TableDelta { table: string; upserts: Statement[]; deletionMarkers: Statement[]; changed: number; deleted: number; unchanged: number; resumed: number }

const accountId = requireEnvironment("CLOUDFLARE_ACCOUNT_ID");
const apiToken = requireEnvironment("CLOUDFLARE_API_TOKEN");
const databaseName = process.env.CLOUDFLARE_D1_DATABASE_NAME ?? "discatsharp-docs-search";
const requestedMode = process.argv[2];
const mode = requestedMode === "activate" ? "activate" : "stage";
const artifactArgument = requestedMode === "stage" || requestedMode === "activate" ? process.argv[3] : requestedMode;
const artifactPath = resolve(artifactArgument ?? "../obj/search/search-index.json");
const artifact = JSON.parse(await readFile(artifactPath, "utf8")) as SearchArtifact;
if (artifact.schemaVersion !== 1) throw new Error(`Unsupported search artifact schema version ${artifact.schemaVersion}; expected 1.`);
const databaseId = await findDatabaseId(databaseName);
let rowsWritten = 0;
let rowsRead = 0;

if (mode === "activate") {
  await activateRelease();
} else {
  await stageRelease();
}

async function stageRelease(): Promise<void> {
  const existingState = await query("SELECT ready FROM sync_state WHERE id = 1");
  if ((existingState[0]?.results?.length ?? 0) === 0) {
    await executeBatch([{
      sql: `INSERT INTO sync_state (id, schema_version, source_commit, generated_at, ready, modules_json, types_json)
            VALUES (1, ?, ?, ?, 0, ?, ?)`,
      params: [toD1Scalar(artifact.schemaVersion), artifact.sourceCommit, artifact.generatedAt, JSON.stringify(artifact.modules), JSON.stringify(artifact.types)],
    }]);
  }

  const stagedResult = await query("SELECT complete, source_commit, generated_at FROM staged_sync_state WHERE id = 1");
  const stagedState = stagedResult[0]?.results?.[0] as unknown as StagedReleaseState | undefined;
  if (!isSameRelease(stagedState, artifact.sourceCommit, artifact.generatedAt)) {
    await executeBatch([
      { sql: "DELETE FROM staged_symbols" },
      { sql: "DELETE FROM staged_documents" },
      { sql: "DELETE FROM staged_source_chunks" },
      { sql: "DELETE FROM staged_deletions" },
      { sql: "DELETE FROM staged_sync_state" },
      {
        sql: `INSERT INTO staged_sync_state (id, schema_version, source_commit, generated_at, complete, symbol_count, document_count,
              source_chunk_count, modules_json, types_json) VALUES (1, ?, ?, ?, 0, ?, ?, ?, ?, ?)`,
        params: [toD1Scalar(artifact.schemaVersion), artifact.sourceCommit, artifact.generatedAt, toD1Scalar(artifact.symbols.length),
          toD1Scalar(artifact.documents.length), toD1Scalar(artifact.sourceChunks.length), JSON.stringify(artifact.modules), JSON.stringify(artifact.types)],
      },
    ]);
  } else if (stagedState?.complete === 1) {
    console.log(JSON.stringify({ database: databaseName, artifact: artifactPath, phase: "staged", resumed: true, rowsRead, rowsWritten }));
    return;
  }

  const deltas = [
    await createTableDelta("symbols", artifact.symbols, symbolStatement),
    await createTableDelta("documents", artifact.documents, documentStatement),
    await createTableDelta("source_chunks", artifact.sourceChunks, sourceChunkStatement),
  ];
  await executeBatches(deltas.flatMap((delta) => delta.upserts));
  await executeBatches(deltas.flatMap((delta) => delta.deletionMarkers));
  await executeBatch([{
    sql: "UPDATE staged_sync_state SET completed_at = ?, complete = 1 WHERE id = 1 AND source_commit = ? AND generated_at = ?",
    params: [new Date().toISOString(), artifact.sourceCommit, artifact.generatedAt],
  }]);

  for (const delta of deltas) {
    console.log(JSON.stringify({ table: delta.table, changed: delta.changed, deleted: delta.deleted, unchanged: delta.unchanged, resumed: delta.resumed }));
  }
  console.log(JSON.stringify({ database: databaseName, artifact: artifactPath, phase: "staged", symbols: artifact.symbols.length,
    documents: artifact.documents.length, sourceChunks: artifact.sourceChunks.length, rowsRead, rowsWritten }));
}

async function activateRelease(): Promise<void> {
  const stagedResult = await query("SELECT complete, source_commit, generated_at FROM staged_sync_state WHERE id = 1");
  const stagedState = stagedResult[0]?.results?.[0] as unknown as StagedReleaseState | undefined;
  if (!isSameRelease(stagedState, artifact.sourceCommit, artifact.generatedAt) || stagedState?.complete !== 1) {
    throw new Error(`The complete staged index does not match '${artifact.sourceCommit}' generated at '${artifact.generatedAt}'.`);
  }

  await executeBatch(createActivationStatements());
  const activeResult = await query("SELECT ready, source_commit, generated_at FROM sync_state WHERE id = 1");
  const active = activeResult[0]?.results?.[0] as { ready?: number; source_commit?: string; generated_at?: string } | undefined;
  if (active?.ready !== 1 || active.source_commit !== artifact.sourceCommit || active.generated_at !== artifact.generatedAt) {
    throw new Error("D1 activation completed without publishing the expected index metadata.");
  }
  console.log(JSON.stringify({ database: databaseName, artifact: artifactPath, phase: "activated", sourceCommit: artifact.sourceCommit,
    generatedAt: artifact.generatedAt, rowsRead, rowsWritten }));
}

async function createTableDelta<T extends { id: string; contentHash: string }>(table: string, records: T[], createStatement: (record: T) => Statement): Promise<TableDelta> {
  const current = await query(`SELECT record_id, content_hash FROM ${table}`);
  const delta = diffHashes((current[0]?.results ?? []) as unknown as ExistingHashRow[], records);
  const staged = await query(`SELECT record_id, content_hash FROM staged_${table}`);
  const pending = diffHashes((staged[0]?.results ?? []) as unknown as ExistingHashRow[], delta.changedRecords);
  const markedResult = await query(`SELECT record_id FROM staged_deletions WHERE table_name = '${table}'`);
  const marked = new Set((markedResult[0]?.results ?? []).map((row) => String(row.record_id)));
  const deletionMarkers = delta.staleIds.filter((recordId) => !marked.has(recordId)).map((recordId) => ({
    sql: "INSERT INTO staged_deletions (table_name, record_id) VALUES (?, ?) ON CONFLICT(table_name, record_id) DO NOTHING",
    params: [table, recordId],
  }));
  return { table, upserts: pending.changedRecords.map(createStatement), deletionMarkers, changed: delta.changedRecords.length,
    deleted: delta.staleIds.length, unchanged: delta.unchanged, resumed: pending.unchanged + marked.size };
}

function symbolStatement(record: ArtifactSymbol): Statement {
  return {
    sql: `INSERT INTO staged_symbols (record_id, uid, name, display_name, qualified_name, full_name, kind, namespace, module, parent_uid, summary, signature, content, url,
          source_path, source_start_line, source_end_line, related_json, content_hash)
          VALUES (?, ?, ?, ?, ?, ?, ?, json_extract(?, '$'), json_extract(?, '$'), json_extract(?, '$'), ?, ?, ?, ?,
            json_extract(?, '$'), json_extract(?, '$'), json_extract(?, '$'), ?, ?)
          ON CONFLICT(record_id) DO UPDATE SET uid=excluded.uid, name=excluded.name, display_name=excluded.display_name,
          qualified_name=excluded.qualified_name, full_name=excluded.full_name, kind=excluded.kind, namespace=excluded.namespace,
          module=excluded.module, parent_uid=excluded.parent_uid, summary=excluded.summary, signature=excluded.signature, content=excluded.content,
          url=excluded.url, source_path=excluded.source_path, source_start_line=excluded.source_start_line, source_end_line=excluded.source_end_line,
          related_json=excluded.related_json, content_hash=excluded.content_hash`,
    params: [record.id, record.uid, record.name, record.displayName, record.qualifiedName, record.fullName, record.kind, toD1JsonScalar(record.namespace),
      toD1JsonScalar(record.module), toD1JsonScalar(record.parentUid), record.summary, record.signature, record.content, record.url, toD1JsonScalar(record.source?.path ?? null),
      toD1JsonScalar(record.source?.startLine ?? null), toD1JsonScalar(record.source?.endLine ?? null), JSON.stringify(record.relatedUids), record.contentHash],
  };
}

function documentStatement(record: ArtifactDocument): Statement {
  return {
    sql: `INSERT INTO staged_documents (record_id, document_key, family, kind, title, description, content, url, module, source_path, content_hash)
          VALUES (?, ?, ?, ?, ?, ?, ?, ?, json_extract(?, '$'), ?, ?)
          ON CONFLICT(record_id) DO UPDATE SET document_key=excluded.document_key, family=excluded.family, kind=excluded.kind,
          title=excluded.title, description=excluded.description, content=excluded.content, url=excluded.url, module=excluded.module,
          source_path=excluded.source_path, content_hash=excluded.content_hash`,
    params: [record.id, record.documentKey, record.family, record.kind, record.title, record.description, record.content, record.url, toD1JsonScalar(record.module), record.sourcePath, record.contentHash],
  };
}

function sourceChunkStatement(record: ArtifactChunk): Statement {
  return {
    sql: `INSERT INTO staged_source_chunks (record_id, path, language, start_line, end_line, content, content_hash)
          VALUES (?, ?, ?, ?, ?, ?, ?)
          ON CONFLICT(record_id) DO UPDATE SET path=excluded.path, language=excluded.language, start_line=excluded.start_line,
          end_line=excluded.end_line, content=excluded.content, content_hash=excluded.content_hash`,
    params: [record.id, record.path, record.language, toD1Scalar(record.startLine), toD1Scalar(record.endLine), record.content, record.contentHash],
  };
}

async function executeBatches(statements: Statement[]): Promise<void> {
  let batch: Statement[] = [];
  let bytes = 0;
  for (const statement of statements) {
    const statementBytes = Buffer.byteLength(JSON.stringify(statement));
    if (batch.length > 0 && (batch.length >= 20 || bytes + statementBytes > 4 * 1024 * 1024)) {
      await executeBatch(batch);
      batch = [];
      bytes = 0;
    }
    if (statementBytes > 4 * 1024 * 1024) {
      throw new Error("A single index record exceeds the 4 MiB synchronization request limit.");
    }
    batch.push(statement);
    bytes += statementBytes;
  }
  if (batch.length > 0) {
    await executeBatch(batch);
  }
}

async function executeBatch(batch: Statement[]): Promise<void> {
  if (batch.length === 0) return;
  await query(undefined, batch);
}

async function query(sql?: string, batch?: Statement[]): Promise<QueryResult[]> {
  const response = await fetchWithRetry(`https://api.cloudflare.com/client/v4/accounts/${accountId}/d1/database/${databaseId}/query`, {
    method: "POST",
    headers: { Authorization: `Bearer ${apiToken}`, "Content-Type": "application/json" },
    body: JSON.stringify(batch === undefined ? { sql } : { batch }),
  });
  const envelope = await response.json() as ApiEnvelope<QueryResult[]>;
  if (!response.ok || !envelope.success) {
    throw new Error(`D1 query failed: ${envelope.errors.map((error) => error.message).join("; ") || response.statusText}`);
  }
  for (const result of envelope.result) {
    if (result.success === false) {
      throw new Error("D1 batch statement failed and the transaction was rolled back.");
    }
    rowsRead += result.meta?.rows_read ?? 0;
    rowsWritten += result.meta?.rows_written ?? 0;
  }
  return envelope.result;
}

async function findDatabaseId(name: string): Promise<string> {
  const response = await fetchWithRetry(`https://api.cloudflare.com/client/v4/accounts/${accountId}/d1/database?name=${encodeURIComponent(name)}&per_page=100`, {
    headers: { Authorization: `Bearer ${apiToken}` },
  });
  const envelope = await response.json() as ApiEnvelope<Array<{ name: string; uuid: string }>>;
  if (!response.ok || !envelope.success) {
    throw new Error(`Unable to list D1 databases: ${envelope.errors.map((error) => error.message).join("; ") || response.statusText}`);
  }
  const matches = envelope.result.filter((database) => database.name === name);
  if (matches.length !== 1) {
    throw new Error(`Expected exactly one D1 database named '${name}', found ${matches.length}. Deploy once to provision it before synchronization.`);
  }
  return matches[0]!.uuid;
}

async function fetchWithRetry(url: string, init: RequestInit): Promise<Response> {
  let lastError: unknown;
  for (let attempt = 0; attempt < 5; attempt++) {
    try {
      const response = await fetch(url, init);
      if (response.status !== 429 && response.status < 500) return response;
      lastError = new Error(`Cloudflare API returned HTTP ${response.status}.`);
      await response.body?.cancel();
    } catch (error) {
      lastError = error;
    }
    if (attempt < 4) {
      await delay(250 * 2 ** attempt + Math.floor(Math.random() * 200));
    }
  }
  throw lastError instanceof Error ? lastError : new Error("Cloudflare API request failed after five attempts.");
}

function requireEnvironment(name: string): string {
  const value = process.env[name];
  if (value === undefined || value.length === 0) throw new Error(`Missing required environment variable ${name}.`);
  return value;
}
