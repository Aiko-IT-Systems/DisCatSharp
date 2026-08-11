import { normalizeCorpus, normalizeLimit, normalizeSearchQuery, normalizeTypes, toFtsQuery, validateSourceRequest } from "./validation";
import { SearchError, type SearchInput, type SearchResponse, type SearchResult, type SearchRow, type SourceRequest, type SyncStateRow } from "./types";

const SYMBOL_KINDS = new Set(["namespace", "class", "struct", "interface", "enum", "delegate", "constructor", "method", "property", "field", "event", "operator"]);

export interface SearchMetrics {
  statementCount: number;
  resultCount: number;
  d1: Array<{ durationMs: number; rowsRead: number; rowsWritten: number; region?: string; colo?: string }>;
}

const SYMBOL_EXACT_SQL = `SELECT record_id AS id, 'symbol' AS family, kind AS type, qualified_name AS title,
       summary, url, module, corpus, repository,
       CASE
         WHEN canonical_uid = ? COLLATE NOCASE THEN 1000.0
         WHEN full_name = ? COLLATE NOCASE THEN 900.0
         WHEN qualified_name = ? COLLATE NOCASE THEN 800.0
         ELSE 700.0
       END AS score
FROM symbols
WHERE (? IS NULL OR corpus = ? COLLATE NOCASE)
  AND (? IS NULL OR module = ? COLLATE NOCASE)
  AND (json_array_length(?) = 0 OR kind IN (SELECT value FROM json_each(?)))
  AND (canonical_uid = ? COLLATE NOCASE OR full_name = ? COLLATE NOCASE OR qualified_name = ? COLLATE NOCASE OR name = ? COLLATE NOCASE)
ORDER BY score DESC, qualified_name COLLATE NOCASE, record_id
LIMIT ?`;

const SYMBOL_FTS_SQL = `SELECT symbols.record_id AS id, 'symbol' AS family, symbols.kind AS type, symbols.qualified_name AS title,
       symbols.summary, symbols.url, symbols.module, symbols.corpus, symbols.repository,
       100.0 - bm25(symbols_fts, 12.0, 10.0, 8.0, 9.0, 7.0, 4.0, 5.0, 1.0) AS score
FROM symbols_fts JOIN symbols ON symbols.id = symbols_fts.rowid
WHERE symbols_fts MATCH ?
  AND (? IS NULL OR symbols.corpus = ? COLLATE NOCASE)
  AND (? IS NULL OR symbols.module = ? COLLATE NOCASE)
  AND (json_array_length(?) = 0 OR symbols.kind IN (SELECT value FROM json_each(?)))
ORDER BY bm25(symbols_fts, 12.0, 10.0, 8.0, 9.0, 7.0, 4.0, 5.0, 1.0), symbols.qualified_name COLLATE NOCASE, symbols.record_id
LIMIT ?`;

const DOCUMENT_EXACT_SQL = `SELECT record_id AS id, family, kind AS type, title, description AS summary, url, module, corpus, repository,
       CASE WHEN document_key = ? COLLATE NOCASE THEN 500.0 WHEN title = ? COLLATE NOCASE THEN 480.0 ELSE 400.0 END AS score
FROM documents
WHERE (? IS NULL OR corpus = ? COLLATE NOCASE)
  AND (? IS NULL OR module = ? COLLATE NOCASE)
  AND (? = 1 OR json_array_length(?) = 0 OR kind IN (SELECT value FROM json_each(?)))
  AND (document_key = ? COLLATE NOCASE OR title = ? COLLATE NOCASE)
ORDER BY score DESC, title COLLATE NOCASE, record_id
LIMIT ?`;

const DOCUMENT_FTS_SQL = `SELECT documents.record_id AS id, documents.family, documents.kind AS type, documents.title,
       documents.description AS summary, documents.url, documents.module, documents.corpus, documents.repository,
       100.0 - bm25(documents_fts, 8.0, 10.0, 6.0, 1.0) AS score
FROM documents_fts JOIN documents ON documents.id = documents_fts.rowid
WHERE documents_fts MATCH ?
  AND (? IS NULL OR documents.corpus = ? COLLATE NOCASE)
  AND (? IS NULL OR documents.module = ? COLLATE NOCASE)
  AND (? = 1 OR json_array_length(?) = 0 OR documents.kind IN (SELECT value FROM json_each(?)))
ORDER BY bm25(documents_fts, 8.0, 10.0, 6.0, 1.0), documents.title COLLATE NOCASE, documents.record_id
LIMIT ?`;

export class SearchService {
  public readonly metrics: SearchMetrics = {
    statementCount: 0,
    resultCount: 0,
    d1: [],
  };

  public constructor(private readonly db: D1Database) {}

  public async search(input: SearchInput): Promise<SearchResponse> {
    const query = normalizeSearchQuery(input.query);
    const limit = normalizeLimit(input.limit);
    const types = normalizeTypes(input.types);
    const corpus = normalizeCorpus(input.corpus);
    const states = await this.getReadyStates();
    const selectedStates = corpus === undefined ? states : states.filter((state) => state.corpus === corpus);
    if (selectedStates.length === 0) {
      throw new SearchError("invalid_corpus", `Unknown or unavailable documentation corpus '${corpus}'.`);
    }
    const state = selectedStates.reduce((latest, candidate) => Date.parse(candidate.generated_at) > Date.parse(latest.generated_at) ? candidate : latest);
    const availableTypes = new Set<string>(selectedStates.flatMap((item) => JSON.parse(item.types_json) as string[]));
    for (const type of types) {
      if (type !== "conceptual" && !availableTypes.has(type)) {
        throw new SearchError("invalid_type", `Unknown search type '${type}'.`);
      }
    }
    if (input.symbolsOnly === true && types.some((type) => !SYMBOL_KINDS.has(type))) {
      throw new SearchError("invalid_type", "Symbol searches only accept API symbol kinds.");
    }
    if (input.module !== undefined) {
      const modules = selectedStates.flatMap((item) => JSON.parse(item.modules_json) as string[]);
      if (!modules.some((module) => module.localeCompare(input.module!, undefined, { sensitivity: "accent" }) === 0)) {
        throw new SearchError("invalid_module", `Unknown documentation module '${input.module}'.`);
      }
    }

    const conceptual = types.includes("conceptual");
    const symbolKinds = types.filter((type) => SYMBOL_KINDS.has(type));
    // Every non-symbol type emitted by the indexer is a document kind. Keeping
    // this metadata-driven means a future conceptual classification works as
    // soon as it appears in sync_state.types_json.
    const documentKinds = types.filter((type) => type !== "conceptual" && !SYMBOL_KINDS.has(type));
    const includeSymbols = input.symbolsOnly === true || types.length === 0 || symbolKinds.length > 0;
    const includeDocuments = input.symbolsOnly !== true && (types.length === 0 || conceptual || documentKinds.length > 0);
    const fts = toFtsQuery(query);
    const module = input.module ?? null;
    const expandedLimit = Math.min(100, limit * 3);
    const exactStatements: D1PreparedStatement[] = [];
    let symbolExactIndex = -1;
    let documentExactIndex = -1;

	const session = this.db.withSession();

    if (includeSymbols) {
      const kindsJson = JSON.stringify(symbolKinds);
      symbolExactIndex = exactStatements.length;
      exactStatements.push(session.prepare(SYMBOL_EXACT_SQL).bind(
        query, query, query,
        corpus ?? null, corpus ?? null, module, module, kindsJson, kindsJson,
        query, query, query, query, expandedLimit,
      ));
    }
    if (includeDocuments) {
      const kindsJson = JSON.stringify(documentKinds);
      const allConceptual = conceptual ? 1 : 0;
      documentExactIndex = exactStatements.length;
      exactStatements.push(session.prepare(DOCUMENT_EXACT_SQL).bind(
        query, query, corpus ?? null, corpus ?? null, module, module, allConceptual, kindsJson, kindsJson,
        query, query, expandedLimit,
      ));
    }

    const exactBatches = exactStatements.length === 0 ? [] : await session.batch<SearchRow>(exactStatements);
    for (const batch of exactBatches) this.captureMeta(batch.meta);
    const symbolExactRows = symbolExactIndex < 0 ? [] : exactBatches[symbolExactIndex]?.results ?? [];
    const documentExactRows = documentExactIndex < 0 ? [] : exactBatches[documentExactIndex]?.results ?? [];
    const merged = new Map<string, SearchResult>();
    const mergeRows = (rows: SearchRow[]) => {
      for (const row of rows) {
        const current = merged.get(row.id);
        const candidate: SearchResult = { ...row, score: Number(row.score) };
        if (current === undefined || candidate.score > current.score) {
          merged.set(row.id, candidate);
        }
      }
    };
    mergeRows(symbolExactRows);
    mergeRows(documentExactRows);

    const fuzzyStatements: D1PreparedStatement[] = [];
    if (includeSymbols && merged.size < limit && symbolExactRows.length < limit && !symbolExactRows.some((row) => Number(row.score) >= 700)) {
      const kindsJson = JSON.stringify(symbolKinds);
      fuzzyStatements.push(session.prepare(SYMBOL_FTS_SQL).bind(fts, corpus ?? null, corpus ?? null, module, module, kindsJson, kindsJson, expandedLimit));
    }
    if (includeDocuments && merged.size < limit && documentExactRows.length < limit) {
      const kindsJson = JSON.stringify(documentKinds);
      const allConceptual = conceptual ? 1 : 0;
      fuzzyStatements.push(session.prepare(DOCUMENT_FTS_SQL).bind(fts, corpus ?? null, corpus ?? null, module, module, allConceptual, kindsJson, kindsJson, expandedLimit));
    }
    if (fuzzyStatements.length > 0) {
      const fuzzyBatches = await session.batch<SearchRow>(fuzzyStatements);
      for (const batch of fuzzyBatches) {
        this.captureMeta(batch.meta);
        mergeRows(batch.results);
      }
    }

    const results = [...merged.values()]
      .sort((left, right) => right.score - left.score || left.title.localeCompare(right.title) || left.id.localeCompare(right.id))
      .slice(0, limit);
    this.metrics.resultCount = results.length;
    return {
      build: createBuildId(state.generated_at, state.source_commit),
      builds: Object.fromEntries(selectedStates.map((item) => [item.corpus, createBuildId(item.generated_at, item.source_commit)])),
      query,
      results,
    };
  }

  public async fetch(id: string): Promise<Record<string, unknown>> {
    const symbolId = id.startsWith("symbol:") && id.length > "symbol:".length;
    const documentId = id.startsWith("document:") && id.length > "document:".length;
    if (!symbolId && !documentId) {
      throw new SearchError("invalid_id", "Documentation IDs must begin with 'symbol:' or 'document:'.");
    }
	const session = this.db.withSession();
    const states = await this.getReadyStates();
    if (symbolId) {
      const symbolResponse = await session.prepare(`SELECT record_id AS id, canonical_uid AS uid, name, display_name AS displayName, qualified_name AS qualifiedName,
        full_name AS fullName, kind AS type, namespace, module, parent_uid AS parentUid, summary, signature, content, url,
        source_path AS sourcePath, source_start_line AS sourceStartLine, source_end_line AS sourceEndLine, related_json AS relatedJson,
        corpus, repository
        FROM symbols WHERE record_id = ?`).bind(id).all<Record<string, unknown>>();
      this.captureMeta(symbolResponse.meta);
      const symbol = symbolResponse.results[0];
      if (symbol === undefined) {
        throw new SearchError("not_found", `No indexed symbol has ID '${id}'.`, 404);
      }
      const state = stateForCorpus(states, String(symbol.corpus));
      const build = createBuildId(state.generated_at, state.source_commit);
      const relatedUids = JSON.parse(String(symbol.relatedJson ?? "[]")) as string[];
      delete symbol.relatedJson;
      let related: unknown[] = [];
      if (relatedUids.length > 0) {
        const relatedResponse = await session.prepare(`SELECT record_id AS id, canonical_uid AS uid, qualified_name AS title, kind AS type, url
          FROM symbols WHERE corpus = ? AND canonical_uid IN (SELECT value FROM json_each(?)) ORDER BY qualified_name COLLATE NOCASE LIMIT 50`)
          .bind(symbol.corpus, JSON.stringify(relatedUids)).all();
        this.captureMeta(relatedResponse.meta);
        related = relatedResponse.results;
      }
      this.metrics.resultCount = 1;
      return { build, ...symbol, family: "symbol", related };
    }

    const documentResponse = await session.prepare(`SELECT record_id AS id, document_key AS documentKey, family, kind AS type, title,
      description, content, url, module, source_path AS sourcePath, corpus, repository FROM documents WHERE record_id = ?`).bind(id).all<Record<string, unknown>>();
    this.captureMeta(documentResponse.meta);
    const document = documentResponse.results[0];
    if (document === undefined) {
      throw new SearchError("not_found", `No indexed document has ID '${id}'.`, 404);
    }
    const state = stateForCorpus(states, String(document.corpus));
    const build = createBuildId(state.generated_at, state.source_commit);
    this.metrics.resultCount = 1;
    return { build, ...document };
  }

  public async getSource(input: SourceRequest): Promise<Record<string, unknown>> {
    const states = await this.getReadyStates();
    const request = validateSourceRequest(input);
	const session = this.db.withSession();
    const result = await session.prepare(`SELECT start_line, end_line, content, corpus, repository FROM source_chunks
      WHERE path = ? AND start_line <= ? AND end_line >= ? ORDER BY start_line`).bind(request.path, request.endLine, request.startLine).all<{
        start_line: number; end_line: number; content: string; corpus: string; repository: string;
      }>();
    this.captureMeta(result.meta);
    if (result.results.length === 0) {
      throw new SearchError("source_not_found", `The source path or requested range is not indexed: '${request.path}'.`, 404);
    }
    const corpus = String(result.results[0]!.corpus);
    if (result.results.some((chunk) => String(chunk.corpus) !== corpus)) {
      throw new SearchError("invalid_index_state", "The source range spans multiple documentation corpora.", 500);
    }
    const state = stateForCorpus(states, corpus);

    let coveredThrough = request.startLine - 1;
    for (const chunk of result.results) {
      if (chunk.start_line > coveredThrough + 1) break;
      coveredThrough = Math.max(coveredThrough, chunk.end_line);
    }
    if (coveredThrough < request.endLine) {
      throw new SearchError("source_not_found", `The source path or requested range is not indexed: '${request.path}'.`, 404);
    }

    const selected: string[] = [];
    for (const chunk of result.results) {
      const lines = chunk.content.replaceAll("\r\n", "\n").replaceAll("\r", "\n").split("\n");
      const from = Math.max(request.startLine, chunk.start_line);
      const to = Math.min(request.endLine, chunk.end_line);
      selected.push(...lines.slice(from - chunk.start_line, to - chunk.start_line + 1));
    }
    const content = selected.join("\n");
    if (new TextEncoder().encode(content).byteLength > 128 * 1024) {
      throw new SearchError("source_response_too_large", "The source response exceeds 128 KiB; request a narrower range.");
    }
    this.metrics.resultCount = 1;
    return {
      build: createBuildId(state.generated_at, state.source_commit),
      id: `source:${request.path}#L${request.startLine}-L${request.endLine}`,
      path: request.path,
      language: "csharp",
      startLine: request.startLine,
      endLine: request.endLine,
      content,
      corpus,
      repository: result.results[0]!.repository,
    };
  }

  private async getReadyStates(): Promise<SyncStateRow[]> {
	const session = this.db.withSession();
    const response = await session.prepare(`SELECT corpus, repository, site_base_url, ready, source_commit, generated_at, modules_json, types_json
      FROM corpus_sync_state WHERE ready = 1 ORDER BY corpus`).all<SyncStateRow>();
    this.captureMeta(response.meta);
    if (response.results.length === 0) {
      throw new SearchError("index_not_ready", "The documentation search index is not ready yet.", 503);
    }
    return response.results;
  }

  private captureMeta(meta: D1Meta): void {
    this.metrics.statementCount++;
    this.metrics.d1.push({
      durationMs: meta.timings?.sql_duration_ms ?? meta.duration,
      rowsRead: meta.rows_read,
      rowsWritten: meta.rows_written,
      ...(meta.served_by_region === undefined ? {} : { region: meta.served_by_region }),
      ...(meta.served_by_colo === undefined ? {} : { colo: meta.served_by_colo }),
    });
  }
}

function stateForCorpus(states: readonly SyncStateRow[], corpus: string): SyncStateRow {
  const state = states.find((candidate) => candidate.corpus === corpus);
  if (state === undefined) {
    throw new SearchError("index_not_ready", `The '${corpus}' documentation corpus is not ready yet.`, 503);
  }
  return state;
}

export function createBuildId(generatedAt: string, sourceCommit: string): string {
  const generatedAtMilliseconds = Date.parse(generatedAt);
  const commit = sourceCommit.trim().toLowerCase();
  if (!Number.isFinite(generatedAtMilliseconds) || !/^[0-9a-f]{7,64}$/u.test(commit)) {
    throw new SearchError("invalid_index_state", "The documentation search index has invalid build metadata.", 500);
  }
  return `${Math.floor(generatedAtMilliseconds / 1_000)}-${commit.slice(0, 12)}`;
}
