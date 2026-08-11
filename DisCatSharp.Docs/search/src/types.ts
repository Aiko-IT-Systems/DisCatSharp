export interface SearchInput {
  query: string;
  types?: readonly string[];
  module?: string;
  corpus?: string;
  limit?: number;
  symbolsOnly?: boolean;
}

export interface SearchResult {
  id: string;
  family: "symbol" | "conceptual";
  type: string;
  title: string;
  summary: string;
  url: string;
  module: string | null;
  corpus: string;
  repository: string;
  score: number;
}

export interface SearchResponse {
  build: string;
  builds: Record<string, string>;
  query: string;
  results: SearchResult[];
}

export interface SourceRequest {
  path: string;
  startLine: number;
  endLine: number;
}

export class SearchError extends Error {
  public constructor(
    public readonly code: string,
    message: string,
    public readonly status = 400,
  ) {
    super(message);
    this.name = "SearchError";
  }
}

export interface SyncStateRow {
  corpus: string;
  repository: string;
  site_base_url: string | null;
  ready: number;
  source_commit: string;
  generated_at: string;
  modules_json: string;
  types_json: string;
}

export interface SearchRow {
  id: string;
  family: "symbol" | "conceptual";
  type: string;
  title: string;
  summary: string;
  url: string;
  module: string | null;
  corpus: string;
  repository: string;
  score: number;
}
