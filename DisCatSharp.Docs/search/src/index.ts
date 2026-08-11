import { createMcpHandler } from "agents/mcp/server";
import { createDisCatSharpMcpServer } from "./mcp";
import { SearchService, type SearchMetrics } from "./search-service";
import { SearchError } from "./types";

const SEARCH_PARAMETERS = new Set(["q", "type", "module", "limit"]);
const MCP_HOSTNAMES = ["docs.dcs.aitsys.dev", "localhost", "127.0.0.1"];
const CORS_HEADERS = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Methods": "GET, OPTIONS",
  "Access-Control-Allow-Headers": "Content-Type",
};

export default {
  async fetch(request, env, ctx): Promise<Response> {
    const started = performance.now();
    const requestId = crypto.randomUUID();
    const url = new URL(request.url);
    let route = "not_found";
    let errorCode: string | undefined;
    let metrics: SearchMetrics | undefined;
    try {
      if (url.pathname === "/mcp") {
        route = "mcp";
        return await createMcpHandler(
          () => createDisCatSharpMcpServer(env.DB, (value, toolErrorCode, queryLength = 0) => {
            metrics = value;
            if (toolErrorCode !== undefined) errorCode = toolErrorCode;
            console.log(JSON.stringify({
              level: "info",
              phase: "completed",
              route: "mcp",
              requestId,
              method: request.method,
              queryLength,
              latencyMs: Math.round(performance.now() - started),
              statementCount: value.statementCount,
              resultCount: value.resultCount,
              d1: value.d1,
              ...(toolErrorCode === undefined ? {} : { errorCode: toolErrorCode }),
            }));
          }),
          {
            route: "/mcp",
            responseMode: "auto",
            legacy: "stateless",
            allowedHostnames: url.hostname.endsWith(".workers.dev") ? [...MCP_HOSTNAMES, url.hostname] : MCP_HOSTNAMES,
            allowedOriginHostnames: MCP_HOSTNAMES,
            onerror: (error) => console.error(JSON.stringify({ level: "error", route: "mcp", requestId, error: error.name })),
          },
        )(request, env, ctx);
      }

      if (url.pathname === "/_search" || url.pathname === "/_search/symbol") {
        route = url.pathname;
        return await handleSearchRequest(request, env.DB, url.pathname === "/_search/symbol", requestId, (value) => { metrics = value; });
      }

      return jsonResponse({ error: { code: "not_found", message: "The requested documentation service route does not exist.", requestId } }, 404);
    } catch (error) {
      const searchError = error instanceof SearchError ? error : new SearchError("internal_error", "The documentation search service could not complete the request.", 500);
      errorCode = searchError.code;
      console.error(JSON.stringify({ level: "error", route, requestId, errorCode: searchError.code }));
      return jsonResponse({ error: { code: searchError.code, message: searchError.message, requestId } }, searchError.status);
    } finally {
      console.log(JSON.stringify({
        level: "info",
        phase: route === "mcp" ? "accepted" : "completed",
        route,
        requestId,
        method: request.method,
        queryLength: url.searchParams.get("q")?.length ?? 0,
        latencyMs: Math.round(performance.now() - started),
        statementCount: metrics?.statementCount ?? 0,
        resultCount: metrics?.resultCount ?? 0,
        d1: metrics?.d1 ?? [],
        ...(errorCode === undefined ? {} : { errorCode }),
      }));
    }
  },
} satisfies ExportedHandler<Env>;

async function handleSearchRequest(request: Request, db: D1Database, symbolsOnly: boolean, requestId: string, onMetrics: (metrics: SearchMetrics) => void): Promise<Response> {
  if (request.method === "OPTIONS") {
    return new Response(null, { status: 204, headers: CORS_HEADERS });
  }
  if (request.method !== "GET") {
    return jsonResponse({ error: { code: "method_not_allowed", message: "Search endpoints only accept GET requests.", requestId } }, 405, { Allow: "GET, OPTIONS" });
  }

  const url = new URL(request.url);
  for (const key of url.searchParams.keys()) {
    if (!SEARCH_PARAMETERS.has(key)) {
      throw new SearchError("unknown_parameter", `Unknown search parameter '${key}'.`);
    }
    if (url.searchParams.getAll(key).length !== 1) {
      throw new SearchError("duplicate_parameter", `Search parameter '${key}' may only appear once.`);
    }
  }
  const query = url.searchParams.get("q") ?? "";
  const typeValue = url.searchParams.get("type");
  const module = url.searchParams.get("module") ?? undefined;
  const rawLimit = url.searchParams.get("limit");
  const limit = rawLimit === null || rawLimit === "" ? undefined : Number(rawLimit);
  const service = new SearchService(db);
  try {
    const response = await service.search({
      query,
      types: typeValue === null ? undefined : typeValue.split(","),
      module,
      limit,
      symbolsOnly,
    });
    return jsonResponse(response, 200);
  } finally {
    onMetrics(service.metrics);
  }
}

function jsonResponse(value: unknown, status: number, headers: Record<string, string> = {}): Response {
  return Response.json(value, {
    status,
    headers: {
      ...CORS_HEADERS,
      "Cache-Control": "no-store",
      "Content-Type": "application/json; charset=utf-8",
      ...headers,
    },
  });
}
