import { describe, expect, it } from "vitest";
import { diffHashes, toD1JsonScalar, toD1Scalar } from "../src/sync-plan";
import { bindProductionDatabase } from "../src/wrangler-config";

const desired = [
  { id: "a", contentHash: "hash-a", value: 1 },
  { id: "b", contentHash: "hash-b", value: 2 },
  { id: "c", contentHash: "hash-c", value: 3 },
];

describe("incremental synchronization planning", () => {
  it("detects new, changed, stale, and unchanged records deterministically", () => {
    const delta = diffHashes([
      { record_id: "a", content_hash: "hash-a" },
      { record_id: "b", content_hash: "old-b" },
      { record_id: "stale", content_hash: "old" },
    ], desired);

    expect(delta.changedRecords.map((record) => record.id)).toEqual(["b", "c"]);
    expect(delta.staleIds).toEqual(["stale"]);
    expect(delta.unchanged).toBe(1);
  });

  it("is idempotent after completion", () => {
    const delta = diffHashes(desired.map((record) => ({ record_id: record.id, content_hash: record.contentHash })), desired);

    expect(delta).toEqual({ changedRecords: [], staleIds: [], unchanged: 3 });
  });

  it("resumes an interrupted initial seed by only scheduling missing rows", () => {
    const delta = diffHashes([{ record_id: "a", content_hash: "hash-a" }], desired);

    expect(delta.changedRecords.map((record) => record.id)).toEqual(["b", "c"]);
    expect(delta.staleIds).toEqual([]);
  });

  it("migrates a classification-prefixed document ID as an upsert followed by a harmless stale delete", () => {
    const delta = diffHashes(
      [{ record_id: "article:guide", content_hash: "hash-guide" }],
      [{ id: "document:guide", contentHash: "hash-guide" }],
    );

    expect(delta.changedRecords.map((record) => record.id)).toEqual(["document:guide"]);
    expect(delta.staleIds).toEqual(["article:guide"]);
  });

  it("rejects duplicate desired IDs before making a synchronization plan", () => {
    expect(() => diffHashes([], [desired[0]!, { ...desired[0]! }])).toThrow("duplicate record ID 'a'");
  });

  it("serializes REST API parameters without losing numbers or SQL nulls", () => {
    expect(toD1Scalar(12)).toBe("12");
    expect(toD1JsonScalar(12)).toBe("12");
    expect(toD1JsonScalar(null)).toBe("null");
    expect(toD1JsonScalar("DisCatSharp.Voice")).toBe('"DisCatSharp.Voice"');
    expect(() => toD1Scalar(Number.POSITIVE_INFINITY)).toThrow("non-finite");
    expect(() => toD1JsonScalar(Number.NaN)).toThrow("non-finite");
  });
});

describe("production Wrangler configuration", () => {
  it("injects the discovered D1 UUID without changing routes or the committed template", () => {
    const template = {
      name: "discatsharp-docs-search",
      routes: [{ pattern: "docs.dcs.aitsys.dev/_search*" }],
      d1_databases: [{ binding: "DB", database_name: "discatsharp-docs-search", migrations_dir: "migrations" }],
    };
    const configured = bindProductionDatabase(template, "discatsharp-docs-search", "11111111-2222-4333-8444-555555555555");

    expect(configured.d1_databases?.[0]?.database_id).toBe("11111111-2222-4333-8444-555555555555");
    expect(configured.routes).toEqual(template.routes);
    expect(template.d1_databases[0]).not.toHaveProperty("database_id");
  });

  it("rejects invalid UUIDs and ambiguous bindings", () => {
    expect(() => bindProductionDatabase({ d1_databases: [] }, "discatsharp-docs-search", "not-a-uuid")).toThrow("invalid D1 database UUID");
    expect(() => bindProductionDatabase({ d1_databases: [] }, "discatsharp-docs-search", "11111111-2222-4333-8444-555555555555")).toThrow("found 0");
  });
});
