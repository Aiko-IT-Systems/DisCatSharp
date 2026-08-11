import { SearchError, type SourceRequest } from "./types";

const MAX_QUERY_LENGTH = 200;
const DEFAULT_LIMIT = 12;
const MAX_LIMIT = 50;
const MAX_SOURCE_LINES = 200;

export function normalizeSearchQuery(value: string): string {
  const normalized = value.trim().replace(/\s+/gu, " ").replace(/\(\)$/u, "");
  if (normalized.length < 2) {
    throw new SearchError("query_too_short", "Search queries must contain at least two characters.");
  }
  if (normalized.length > MAX_QUERY_LENGTH) {
    throw new SearchError("query_too_long", `Search queries may contain at most ${MAX_QUERY_LENGTH} characters.`);
  }
  if (!/[\p{L}\p{N}_]/u.test(normalized)) {
    throw new SearchError("invalid_query", "The query must contain at least one letter, number, or underscore.");
  }
  return normalized;
}

export function normalizeLimit(value: number | undefined): number {
  const limit = value ?? DEFAULT_LIMIT;
  if (!Number.isInteger(limit) || limit < 1 || limit > MAX_LIMIT) {
    throw new SearchError("invalid_limit", `Limit must be an integer between 1 and ${MAX_LIMIT}.`);
  }
  return limit;
}

export function normalizeTypes(values: readonly string[] | undefined): string[] {
  if (values === undefined || values.length === 0) {
    return [];
  }
  if (values.length > 8) {
    throw new SearchError("too_many_types", "At most eight type filters may be supplied.");
  }
  const normalized = values.map((value) => value.trim().toLowerCase()).filter(Boolean);
  return [...new Set(normalized)];
}

export function normalizeCorpus(value: string | undefined): string | undefined {
  if (value === undefined) return undefined;
  const normalized = value.trim().toLowerCase();
  if (!/^[a-z0-9-]{1,50}$/u.test(normalized)) {
    throw new SearchError("invalid_corpus", "Corpus names may only contain letters, digits, and hyphens.");
  }
  return normalized;
}

export function toFtsQuery(query: string): string {
  const tokens = query.match(/[\p{L}\p{N}_]+/gu) ?? [];
  if (tokens.length === 0) {
    throw new SearchError("invalid_query", "The query does not contain searchable terms.");
  }
  return tokens
    .map((token, index) => `"${token.replaceAll('"', '""')}"${index === tokens.length - 1 ? "*" : ""}`)
    .join(" AND ");
}

export function escapeLike(value: string): string {
  return value.replaceAll("\\", "\\\\").replaceAll("%", "\\%").replaceAll("_", "\\_");
}

export function validateSourceRequest(request: SourceRequest): SourceRequest {
  const path = request.path.trim();
  if (
    path.length === 0 ||
    path.includes("\0") ||
    path.includes("\\") ||
    path.startsWith("/") ||
    /^[A-Za-z]:/u.test(path) ||
    path.split("/").some((segment) => segment === "." || segment === ".." || segment.length === 0)
  ) {
    throw new SearchError("unsafe_source_path", "Source paths must be normalized repository-relative paths.");
  }
  if (!Number.isInteger(request.startLine) || !Number.isInteger(request.endLine) || request.startLine < 1 || request.endLine < request.startLine) {
    throw new SearchError("invalid_source_range", "Source ranges use positive 1-based lines and endLine must not precede startLine.");
  }
  if (request.endLine - request.startLine + 1 > MAX_SOURCE_LINES) {
    throw new SearchError("source_range_too_large", `Source requests may contain at most ${MAX_SOURCE_LINES} lines.`);
  }
  return { path, startLine: request.startLine, endLine: request.endLine };
}
