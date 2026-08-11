import { readFile } from "node:fs/promises";
import { Miniflare } from "miniflare";
import { afterEach, describe, expect, it } from "vitest";
import { unstable_splitSqlQuery } from "wrangler";
import { createActivationStatements } from "../src/release-sync";
import { toD1JsonScalar } from "../src/sync-plan";

const runtimes: Miniflare[] = [];

afterEach(async () => {
  await Promise.all(runtimes.splice(0).map((runtime) => runtime.dispose()));
});

async function migratedDatabase(): Promise<D1Database> {
  const runtime = new Miniflare({
    modules: true,
    script: "export default { fetch() { return new Response('ok'); } }",
    d1Databases: ["DB"],
  });
  runtimes.push(runtime);
  const database = await runtime.getD1Database("DB");
  for (const migrationName of ["0001_initial.sql", "0002_staged_releases.sql", "0003_documentation_corpora.sql"]) {
    const migration = await readFile(new URL(`../migrations/${migrationName}`, import.meta.url), "utf8");
    for (const statement of unstable_splitSqlQuery(migration)) {
      await database.prepare(statement).run();
    }
  }
  return database;
}

async function countMatches(database: D1Database, table: "symbols_fts" | "documents_fts", query: string): Promise<number> {
  const row = await database.prepare(`SELECT count(*) AS count FROM ${table} WHERE ${table} MATCH ?`).bind(query).first<{ count: number }>();
  return Number(row?.count ?? 0);
}

describe("D1 migration", () => {
  it("creates the expected exact-match indexes and FTS maintenance triggers", async () => {
    const database = await migratedDatabase();
    const response = await database.prepare("SELECT name FROM sqlite_master WHERE type IN ('index', 'trigger')").all<{ name: string }>();
    const names = response.results.map((row) => row.name);

    expect(names).toEqual(expect.arrayContaining([
      "symbols_uid_nocase",
      "symbols_name_nocase",
      "symbols_qualified_name_nocase",
      "symbols_full_name_nocase",
      "symbols_corpus_canonical_uid_nocase",
      "documents_title_nocase",
      "documents_key_nocase",
      "source_chunks_path_range",
      "symbols_fts_insert",
      "symbols_fts_update",
      "symbols_fts_delete",
      "documents_fts_insert",
      "documents_fts_update",
      "documents_fts_delete",
    ]));
  });

  it("keeps symbol and document FTS rows correct across insert, update, and delete", async () => {
    const database = await migratedDatabase();
    await database.prepare(`INSERT INTO symbols
      (record_id, uid, name, display_name, qualified_name, full_name, kind, summary, signature, content, url, related_json, content_hash)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`).bind(
      "symbol:Alpha", "Library.Alpha", "Alpha", "Alpha", "Library.Alpha", "Library.Alpha", "class",
      "old symbol token", "public class Alpha", "old symbol body", "/api/Alpha.html", "[]", "hash-1",
    ).run();
    expect(await countMatches(database, "symbols_fts", "old")).toBe(1);
    await database.prepare("UPDATE symbols SET summary = ?, content = ?, content_hash = ? WHERE record_id = ?")
      .bind("new symbol token", "new symbol body", "hash-2", "symbol:Alpha").run();
    expect(await countMatches(database, "symbols_fts", "old")).toBe(0);
    expect(await countMatches(database, "symbols_fts", "new")).toBe(1);

    await database.prepare(`INSERT INTO documents
      (record_id, document_key, family, kind, title, description, content, url, source_path, content_hash)
      VALUES (?, ?, 'conceptual', ?, ?, ?, ?, ?, ?, ?)`).bind(
      "document:guide", "guide", "article", "Guide", "legacy document token", "legacy document body", "/guide.html", "guide.md", "hash-1",
    ).run();
    expect(await countMatches(database, "documents_fts", "legacy")).toBe(1);
    await database.prepare("UPDATE documents SET description = ?, content = ?, content_hash = ? WHERE record_id = ?")
      .bind("modern document token", "modern document body", "hash-2", "document:guide").run();
    expect(await countMatches(database, "documents_fts", "legacy")).toBe(0);
    expect(await countMatches(database, "documents_fts", "modern")).toBe(1);

    await database.prepare("DELETE FROM symbols WHERE record_id = ?").bind("symbol:Alpha").run();
    await database.prepare("DELETE FROM documents WHERE record_id = ?").bind("document:guide").run();
    expect(await countMatches(database, "symbols_fts", "new")).toBe(0);
    expect(await countMatches(database, "documents_fts", "modern")).toBe(0);
  });

  it("preserves REST parameter types and migrates a legacy classification-prefixed document ID without deleting the replacement", async () => {
    const database = await migratedDatabase();
    await database.prepare(`INSERT INTO symbols
      (record_id, uid, name, display_name, qualified_name, full_name, kind, namespace, module, parent_uid, summary, signature, content, url,
       source_path, source_start_line, source_end_line, related_json, content_hash)
      VALUES (?, ?, ?, ?, ?, ?, ?, json_extract(?, '$'), json_extract(?, '$'), json_extract(?, '$'), ?, ?, ?, ?,
        json_extract(?, '$'), json_extract(?, '$'), json_extract(?, '$'), ?, ?)`).bind(
      "symbol:Library.Alpha", "Library.Alpha", "Alpha", "Alpha", "Library.Alpha", "Library.Alpha", "class",
      toD1JsonScalar("Library"), toD1JsonScalar(null), toD1JsonScalar(null), "summary", "public class Alpha", "body", "/api/Alpha.html",
      toD1JsonScalar("Library/Alpha.cs"), toD1JsonScalar(12), toD1JsonScalar(20), "[]", "hash-1",
    ).run();
    const symbol = await database.prepare("SELECT namespace, module, parent_uid, source_start_line, source_end_line FROM symbols").first<{
      namespace: string; module: string | null; parent_uid: string | null; source_start_line: number; source_end_line: number;
    }>();
    expect(symbol).toMatchObject({ namespace: "Library", module: null, parent_uid: null, source_start_line: 12, source_end_line: 20 });

    await database.prepare(`INSERT INTO documents
      (record_id, document_key, family, kind, title, description, content, url, module, source_path, content_hash)
      VALUES (?, ?, 'conceptual', ?, ?, ?, ?, ?, json_extract(?, '$'), ?, ?)`).bind(
      "article:guide", "guide", "article", "Guide", "description", "body", "/guide.html", toD1JsonScalar(null), "guide.md", "hash-1",
    ).run();
    await database.prepare(`INSERT INTO documents
      (record_id, document_key, family, kind, title, description, content, url, module, source_path, content_hash)
      VALUES (?, ?, 'conceptual', ?, ?, ?, ?, ?, json_extract(?, '$'), ?, ?)
      ON CONFLICT(document_key) DO UPDATE SET record_id=excluded.record_id, kind=excluded.kind, content_hash=excluded.content_hash`).bind(
      "document:guide", "guide", "changelog", "Guide", "description", "body", "/guide.html", toD1JsonScalar(null), "guide.md", "hash-2",
    ).run();
    await database.prepare("DELETE FROM documents WHERE record_id = ?").bind("article:guide").run();
    const documents = await database.prepare("SELECT record_id, kind, module, content_hash FROM documents").all();
    expect(documents.results).toEqual([{ record_id: "document:guide", kind: "changelog", module: null, content_hash: "hash-2" }]);
  });

  it("keeps the active corpus unchanged until a staged release is atomically activated", async () => {
    const database = await migratedDatabase();
    await database.prepare(`INSERT INTO sync_state
      (id, schema_version, source_commit, generated_at, completed_at, ready, symbol_count, document_count, source_chunk_count, modules_json, types_json)
      VALUES (1, 1, 'oldcommit', '2026-08-10T00:00:00Z', '2026-08-10T00:01:00Z', 1, 1, 1, 0, '[]', '[]')`).run();
    await database.prepare(`INSERT INTO symbols
      (record_id, uid, name, display_name, qualified_name, full_name, kind, summary, signature, content, url, related_json, content_hash)
      VALUES ('symbol:Alpha', 'Library.Alpha', 'Alpha', 'Alpha', 'Library.Alpha', 'Library.Alpha', 'class', 'old token', '', 'old body', '/old.html', '[]', 'old-hash')`).run();
    await database.prepare(`INSERT INTO documents
      (record_id, document_key, family, kind, title, description, content, url, source_path, content_hash)
      VALUES ('document:old', 'old', 'conceptual', 'article', 'Old', 'old document', 'old body', '/old-document.html', 'old.md', 'old-hash')`).run();
    await database.prepare(`INSERT INTO staged_corpus_sync_state
      (corpus, repository, schema_version, source_commit, generated_at, completed_at, complete, symbol_count, document_count, source_chunk_count, modules_json, types_json)
      VALUES ('main', 'Aiko-IT-Systems/DisCatSharp', 1, 'newcommit', '2026-08-11T00:00:00Z', '2026-08-11T00:01:00Z', 1, 1, 1, 0, '["Library"]', '["article","class"]')`).run();
    await database.prepare(`INSERT INTO staged_symbols
      (record_id, uid, name, display_name, qualified_name, full_name, kind, summary, signature, content, url, related_json, content_hash)
      VALUES ('symbol:Alpha', 'Library.Alpha', 'Alpha', 'Alpha', 'Library.Alpha', 'Library.Alpha', 'class', 'new token', '', 'new body', '/new.html', '[]', 'new-hash')`).run();
    await database.prepare(`INSERT INTO staged_documents
      (record_id, document_key, family, kind, title, description, content, url, source_path, content_hash)
      VALUES ('document:new', 'new', 'conceptual', 'article', 'New', 'new document', 'new body', '/new-document.html', 'new.md', 'new-hash')`).run();
    await database.prepare("INSERT INTO staged_deletions (table_name, record_id) VALUES ('documents', 'document:old')").run();

    expect(await database.prepare("SELECT summary FROM symbols WHERE record_id = 'symbol:Alpha'").first()).toEqual({ summary: "old token" });
    expect(await database.prepare("SELECT source_commit FROM sync_state WHERE id = 1").first()).toEqual({ source_commit: "oldcommit" });

    await database.batch(createActivationStatements("main").map((statement) => database.prepare(statement.sql).bind(...(statement.params ?? []))));

    expect(await database.prepare("SELECT summary, url FROM symbols WHERE record_id = 'symbol:Alpha'").first()).toEqual({ summary: "new token", url: "/new.html" });
    expect(await database.prepare("SELECT record_id FROM documents ORDER BY record_id").all()).toMatchObject({ results: [{ record_id: "document:new" }] });
    expect(await database.prepare("SELECT ready, source_commit FROM sync_state WHERE id = 1").first()).toEqual({ ready: 1, source_commit: "newcommit" });
    expect(await countMatches(database, "symbols_fts", "old")).toBe(0);
    expect(await countMatches(database, "symbols_fts", "new")).toBe(1);
    expect(await database.prepare("SELECT count(*) AS count FROM staged_corpus_sync_state").first()).toEqual({ count: 0 });
  });

  it("rolls back activation and preserves the previous release when a staged row is invalid", async () => {
    const database = await migratedDatabase();
    await database.prepare(`INSERT INTO sync_state
      (id, schema_version, source_commit, generated_at, completed_at, ready, modules_json, types_json)
      VALUES (1, 1, 'oldcommit', '2026-08-10T00:00:00Z', '2026-08-10T00:01:00Z', 1, '[]', '[]')`).run();
    await database.prepare(`INSERT INTO symbols
      (record_id, uid, name, display_name, qualified_name, full_name, kind, summary, signature, content, url, related_json, content_hash)
      VALUES ('symbol:Alpha', 'Library.Alpha', 'Alpha', 'Alpha', 'Library.Alpha', 'Library.Alpha', 'class', 'old token', '', 'old body', '/old.html', '[]', 'old-hash')`).run();
    await database.prepare(`INSERT INTO staged_corpus_sync_state
      (corpus, repository, schema_version, source_commit, generated_at, completed_at, complete, symbol_count, document_count, source_chunk_count, modules_json, types_json)
      VALUES ('main', 'Aiko-IT-Systems/DisCatSharp', 1, 'newcommit', '2026-08-11T00:00:00Z', '2026-08-11T00:01:00Z', 1, 1, 1, 0, '[]', '[]')`).run();
    await database.prepare(`INSERT INTO staged_symbols
      (record_id, uid, name, display_name, qualified_name, full_name, kind, summary, signature, content, url, related_json, content_hash)
      VALUES ('symbol:Alpha', 'Library.Alpha', 'Alpha', 'Alpha', 'Library.Alpha', 'Library.Alpha', 'class', 'new token', '', 'new body', '/new.html', '[]', 'new-hash')`).run();
    await database.prepare(`INSERT INTO staged_documents
      (record_id, document_key, family, kind, title, description, content, url, source_path, content_hash)
      VALUES ('document:invalid', 'invalid', 'invalid', 'article', 'Invalid', '', '', '/invalid.html', 'invalid.md', 'bad-hash')`).run();

    await expect(database.batch(createActivationStatements("main").map((statement) => database.prepare(statement.sql).bind(...(statement.params ?? []))))).rejects.toThrow();

    expect(await database.prepare("SELECT summary FROM symbols WHERE record_id = 'symbol:Alpha'").first()).toEqual({ summary: "old token" });
    expect(await database.prepare("SELECT ready, source_commit FROM sync_state WHERE id = 1").first()).toEqual({ ready: 1, source_commit: "oldcommit" });
    expect(await database.prepare("SELECT count(*) AS count FROM staged_corpus_sync_state").first()).toEqual({ count: 1 });
  });

  it("activates Extensions without replacing or deleting the main documentation corpus", async () => {
    const database = await migratedDatabase();
    await database.prepare(`INSERT INTO sync_state
      (id, schema_version, source_commit, generated_at, ready, modules_json, types_json)
      VALUES (1, 1, 'maincommit', '2026-08-10T00:00:00Z', 1, '[]', '[]')`).run();
    await database.prepare(`INSERT INTO corpus_sync_state
      (corpus, repository, schema_version, source_commit, generated_at, ready, modules_json, types_json)
      VALUES ('main', 'Aiko-IT-Systems/DisCatSharp', 1, 'maincommit', '2026-08-10T00:00:00Z', 1, '[]', '[]')`).run();
    await database.prepare(`INSERT INTO documents
      (record_id, document_key, family, kind, title, description, content, url, source_path, content_hash, corpus, repository)
      VALUES ('document:home', 'home', 'conceptual', 'article', 'Main', '', 'main body', '/index.html', 'index.md', 'main-hash', 'main', 'Aiko-IT-Systems/DisCatSharp')`).run();
    await database.prepare(`INSERT INTO symbols
      (record_id, uid, canonical_uid, name, display_name, qualified_name, full_name, kind, summary, signature, content, url,
       related_json, content_hash, corpus, repository)
      VALUES ('symbol:Shared.Namespace', 'Shared.Namespace', 'Shared.Namespace', 'Namespace', 'Shared.Namespace', 'Shared.Namespace',
       'Shared.Namespace', 'namespace', '', '', '', '/api/shared.html', '[]', 'main-symbol-hash', 'main', 'Aiko-IT-Systems/DisCatSharp')`).run();
    await database.prepare(`INSERT INTO staged_corpus_sync_state
      (corpus, repository, site_base_url, schema_version, source_commit, generated_at, completed_at, complete,
       symbol_count, document_count, source_chunk_count, modules_json, types_json)
      VALUES ('extensions', 'Aiko-IT-Systems/DisCatSharp.Extensions', 'https://ext-docs.dcs.aitsys.dev', 1, 'extcommit',
       '2026-08-11T00:00:00Z', '2026-08-11T00:01:00Z', 1, 1, 1, 0, '[]', '["article","conceptual","namespace"]')`).run();
    await database.prepare(`INSERT INTO staged_symbols
      (record_id, uid, canonical_uid, name, display_name, qualified_name, full_name, kind, summary, signature, content, url,
       related_json, content_hash, corpus, repository)
      VALUES ('symbol:extensions:Shared.Namespace', 'extensions:Shared.Namespace', 'Shared.Namespace', 'Namespace', 'Shared.Namespace',
       'Shared.Namespace', 'Shared.Namespace', 'namespace', '', '', '', 'https://ext-docs.dcs.aitsys.dev/api/shared.html', '[]',
       'ext-symbol-hash', 'extensions', 'Aiko-IT-Systems/DisCatSharp.Extensions')`).run();
    await database.prepare(`INSERT INTO staged_documents
      (record_id, document_key, family, kind, title, description, content, url, source_path, content_hash, corpus, repository)
      VALUES ('document:extensions:home', 'extensions:home', 'conceptual', 'article', 'Extensions', '', 'extensions body',
       'https://ext-docs.dcs.aitsys.dev/index.html', 'DisCatSharp.Extensions/index.md', 'ext-hash', 'extensions', 'Aiko-IT-Systems/DisCatSharp.Extensions')`).run();

    await database.batch(createActivationStatements("extensions").map((statement) => database.prepare(statement.sql).bind(...(statement.params ?? []))));

    expect(await database.prepare("SELECT record_id, corpus FROM documents ORDER BY record_id").all()).toMatchObject({ results: [
      { record_id: "document:extensions:home", corpus: "extensions" },
      { record_id: "document:home", corpus: "main" },
    ] });
    expect(await database.prepare("SELECT record_id, uid, canonical_uid, corpus FROM symbols ORDER BY record_id").all()).toMatchObject({ results: [
      { record_id: "symbol:Shared.Namespace", uid: "Shared.Namespace", canonical_uid: "Shared.Namespace", corpus: "main" },
      { record_id: "symbol:extensions:Shared.Namespace", uid: "extensions:Shared.Namespace", canonical_uid: "Shared.Namespace", corpus: "extensions" },
    ] });
    expect(await database.prepare("SELECT source_commit FROM sync_state WHERE id = 1").first()).toEqual({ source_commit: "maincommit" });
    expect(await database.prepare("SELECT source_commit FROM corpus_sync_state WHERE corpus = 'extensions'").first()).toEqual({ source_commit: "extcommit" });
  });
});
