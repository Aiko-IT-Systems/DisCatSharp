import { readFile } from "node:fs/promises";
import { Miniflare } from "miniflare";
import { afterEach, describe, expect, it } from "vitest";
import { unstable_splitSqlQuery } from "wrangler";
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
  const migration = await readFile(new URL("../migrations/0001_initial.sql", import.meta.url), "utf8");
  for (const statement of unstable_splitSqlQuery(migration)) {
    await database.prepare(statement).run();
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
});
