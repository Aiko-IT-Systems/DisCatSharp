import { describe, expect, it } from "vitest";
import { SearchService } from "../src/search-service";
import type { SearchRow, SyncStateRow } from "../src/types";

const META: D1Meta & Record<string, unknown> = {
  duration: 1,
  size_after: 0,
  rows_read: 1,
  rows_written: 0,
  last_row_id: 0,
  changed_db: false,
  changes: 0,
};

class FakeDatabase {
  public readonly prepared: FakeStatement[] = [];
  private batchOffset = 0;

  public constructor(
    private readonly batchRows: SearchRow[][] = [],
    private readonly sourceRows: Array<{ start_line: number; end_line: number; content: string; corpus: string; repository: string }> = [],
    private readonly state: SyncStateRow | SyncStateRow[] = {
      ready: 1,
      corpus: "main",
      repository: "Aiko-IT-Systems/DisCatSharp",
      site_base_url: "https://docs.dcs.aitsys.dev",
      source_commit: "0123456789abcdef0123456789abcdef01234567",
      generated_at: "2026-08-11T03:04:06.4128512+02:00",
      modules_json: '["DisCatSharp"]',
      types_json: '["article","class","conceptual","future-kind","method"]',
    },
    private readonly documentRows: Array<Record<string, unknown>> = [],
  ) {}

  public prepare(sql: string): D1PreparedStatement {
    const statement = new FakeStatement(this, sql);
    this.prepared.push(statement);
    return statement as unknown as D1PreparedStatement;
  }

  public withSession(): FakeDatabase {
    return this;
  }

  public async batch<T>(statements: D1PreparedStatement[]): Promise<D1Result<T>[]> {
    const offset = this.batchOffset;
    this.batchOffset += statements.length;
    return statements.map((_, index) => ({ success: true, meta: META, results: (this.batchRows[offset + index] ?? []) as T[] }));
  }

  public rowsFor(sql: string): unknown[] {
    if (sql.includes("FROM corpus_sync_state")) return Array.isArray(this.state) ? this.state : [this.state];
    if (sql.includes("FROM source_chunks")) return this.sourceRows;
    if (sql.includes("FROM documents WHERE record_id")) return this.documentRows;
    return [];
  }
}

class FakeStatement {
  public parameters: unknown[] = [];

  public constructor(private readonly database: FakeDatabase, public readonly sql: string) {}

  public bind(...values: unknown[]): D1PreparedStatement {
    this.parameters = values;
    return this as unknown as D1PreparedStatement;
  }

  public async all<T>(): Promise<D1Result<T>> {
    return { success: true, meta: META, results: this.database.rowsFor(this.sql) as T[] };
  }
}

function row(id: string, family: "symbol" | "conceptual", type: string, title: string, score: number): SearchRow {
  return { id, family, type, title, summary: "summary", url: `/${id}.html`, module: "DisCatSharp", corpus: "main", repository: "Aiko-IT-Systems/DisCatSharp", score };
}

describe("SearchService", () => {
  it("deduplicates candidates and keeps exact symbol tiers above FTS and documents", async () => {
    const database = new FakeDatabase([
      [
        row("symbol:Guild", "symbol", "class", "DiscordGuild", 100),
        row("symbol:Guild", "symbol", "class", "DiscordGuild", 800),
        row("symbol:Method", "symbol", "method", "DiscordGuild.GetMemberAsync", 700),
      ],
      [row("document:guilds", "conceptual", "article", "Guilds", 480)],
    ]);

    const service = new SearchService(database as unknown as D1Database);
    const response = await service.search({ query: "DiscordGuild", limit: 3 });

    expect(response.results.map((result) => result.id)).toEqual(["symbol:Guild", "symbol:Method", "document:guilds"]);
    expect(response.build).toBe("1786410246-0123456789ab");
    expect(response.builds).toEqual({ main: "1786410246-0123456789ab" });
    expect(response.results[0]?.score).toBe(800);
    expect(service.metrics.statementCount).toBe(3);
    expect(service.metrics.resultCount).toBe(3);
  });

  it("uses the conceptual family alias without narrowing to literal article records", async () => {
    const database = new FakeDatabase([[row("document:voice", "conceptual", "future-kind", "Voice", 420)]]);
    const service = new SearchService(database as unknown as D1Database);

    const response = await service.search({ query: "voice", types: ["conceptual"] });

    expect(response.results[0]?.type).toBe("future-kind");
    expect(database.prepared.some((statement) => statement.sql.includes("FROM symbols_fts"))).toBe(false);
    const documentStatement = database.prepared.find((statement) => statement.sql.includes("FROM documents_fts"));
    expect(documentStatement?.parameters).toContain(1);
  });

  it("accepts future literal document kinds emitted by index metadata", async () => {
    const database = new FakeDatabase([[row("document:voice", "conceptual", "future-kind", "Voice", 420)]]);
    const service = new SearchService(database as unknown as D1Database);

    const response = await service.search({ query: "voice", types: ["future-kind"] });

    expect(response.results.map((result) => result.id)).toEqual(["document:voice"]);
    const documentStatement = database.prepared.find((statement) => statement.sql.includes("FROM documents_fts"));
    expect(documentStatement?.parameters).toContain('["future-kind"]');
    expect(documentStatement?.parameters).toContain(0);
  });

  it("uses IDs as the final deterministic tie breaker", async () => {
    const database = new FakeDatabase([[
      row("symbol:z", "symbol", "class", "Same", 700),
      row("symbol:a", "symbol", "class", "Same", 700),
    ]]);
    const service = new SearchService(database as unknown as D1Database);

    const response = await service.search({ query: "Same", types: ["class"] });

    expect(response.results.map((result) => result.id)).toEqual(["symbol:a", "symbol:z"]);
  });

  it("does not issue broad symbol FTS work when a strong exact result is available", async () => {
    const database = new FakeDatabase([[row("symbol:Guild", "symbol", "class", "DiscordGuild", 800)]]);
    const service = new SearchService(database as unknown as D1Database);

    await service.search({ query: "DiscordGuild", types: ["class"] });

    expect(database.prepared.some((statement) => statement.sql.includes("FROM symbols_fts"))).toBe(false);
  });

  it("restricts searches and build identity to the requested documentation corpus", async () => {
    const states: SyncStateRow[] = [
      {
        ready: 1,
        corpus: "main",
        repository: "Aiko-IT-Systems/DisCatSharp",
        site_base_url: "https://docs.dcs.aitsys.dev",
        source_commit: "1111111111111111111111111111111111111111",
        generated_at: "2026-08-10T00:00:00Z",
        modules_json: '["DisCatSharp"]',
        types_json: '["class"]',
      },
      {
        ready: 1,
        corpus: "extensions",
        repository: "Aiko-IT-Systems/DisCatSharp.Extensions",
        site_base_url: "https://ext-docs.dcs.aitsys.dev",
        source_commit: "2222222222222222222222222222222222222222",
        generated_at: "2026-08-11T00:00:00Z",
        modules_json: '["DisCatSharp.Extensions"]',
        types_json: '["class"]',
      },
    ];
    const extensionResult = { ...row("symbol:extensions:Widget", "symbol", "class", "Widget", 800), corpus: "extensions", repository: "Aiko-IT-Systems/DisCatSharp.Extensions" };
    const database = new FakeDatabase([[extensionResult]], [], states);
    const service = new SearchService(database as unknown as D1Database);

    const response = await service.search({ query: "Widget", corpus: "extensions", types: ["class"] });

    expect(response.build).toBe("1786406400-222222222222");
    expect(response.builds).toEqual({ extensions: "1786406400-222222222222" });
    expect(response.results).toEqual([extensionResult]);
    expect(database.prepared.find((statement) => statement.sql.includes("FROM symbols\n"))?.parameters).toContain("extensions");
    await expect(service.search({ query: "Widget", corpus: "unknown" })).rejects.toMatchObject({ code: "invalid_corpus" });
  });

  it("rejects conceptual filters for symbol-only search", async () => {
    const service = new SearchService(new FakeDatabase() as unknown as D1Database);
    await expect(service.search({ query: "voice", types: ["conceptual"], symbolsOnly: true })).rejects.toMatchObject({ code: "invalid_type" });
  });

  it("rejects partially indexed source ranges instead of returning truncated content", async () => {
    const database = new FakeDatabase([], [{ start_line: 1, end_line: 10, content: Array.from({ length: 10 }, (_, index) => `line ${index + 1}`).join("\n"), corpus: "main", repository: "Aiko-IT-Systems/DisCatSharp" }]);
    const service = new SearchService(database as unknown as D1Database);

    await expect(service.getSource({ path: "DisCatSharp/Guild.cs", startLine: 5, endLine: 20 })).rejects.toMatchObject({ code: "source_not_found" });
  });

  it("fetches stable document IDs and rejects classification-prefixed legacy IDs", async () => {
    const document = { id: "document:guide", documentKey: "guide", family: "conceptual", type: "changelog", title: "Guide", corpus: "main", repository: "Aiko-IT-Systems/DisCatSharp" };
    const service = new SearchService(new FakeDatabase([], [], undefined, [document]) as unknown as D1Database);

    await expect(service.fetch("document:guide")).resolves.toEqual({ build: "1786410246-0123456789ab", ...document });
    await expect(service.fetch("article:guide")).rejects.toMatchObject({ code: "invalid_id", status: 400 });
    await expect(service.fetch("changelog:guide")).rejects.toMatchObject({ code: "invalid_id", status: 400 });
  });
});
