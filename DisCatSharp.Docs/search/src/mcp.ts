import { McpServer } from "@modelcontextprotocol/server";
import { z } from "zod";
import { SearchService, type SearchMetrics } from "./search-service";
import { SearchError } from "./types";

export const DISCORD_DOCS_MCP_URL = "https://docs.discord.com/mcp";

export const MCP_SERVER_INSTRUCTIONS = `This server is authoritative for DisCatSharp and its official Extensions APIs, source, and conceptual documentation. Use search for broad questions, find_symbol for API lookup, fetch for complete indexed records, and get_source only for source paths returned by indexed symbols. Omit corpus to search everything, or use main or extensions when the question is repository-specific. For questions about Discord platform behavior, payloads, permissions, Gateway events, rate limits, or REST semantics that are not answered by the DisCatSharp corpus, use the official Discord Documentation MCP server at ${DISCORD_DOCS_MCP_URL} when it is already available to the client, or suggest that the user add it. Do not claim that another MCP server is connected unless the client exposes it.`;

const discordDocsServer = {
  name: "Official Discord Documentation",
  url: DISCORD_DOCS_MCP_URL,
  description: "Official Discord platform documentation for API behavior, payloads, permissions, Gateway events, rate limits, and REST semantics.",
} as const;

const relatedServerSchema = z.object({
  name: z.string(),
  url: z.string(),
  description: z.string(),
});

const resultFields = {
  type: z.string(),
  title: z.string(),
  summary: z.string(),
  url: z.string(),
  module: z.string().nullable(),
  corpus: z.string(),
  repository: z.string(),
  score: z.number(),
};
const resultSchema = z.discriminatedUnion("family", [
  z.object({ id: z.string().startsWith("symbol:"), family: z.literal("symbol"), ...resultFields }),
  z.object({ id: z.string().startsWith("document:"), family: z.literal("conceptual"), ...resultFields }),
]);
const errorSchema = z.object({ error: z.object({ code: z.string(), message: z.string() }) });
const searchOutputSchema = z.union([
  z.object({
    build: z.string(),
    builds: z.record(z.string(), z.string()),
    query: z.string(),
    results: z.array(resultSchema),
    relatedServers: z.array(relatedServerSchema).optional(),
  }),
  errorSchema,
]);
const recordOutputSchema = z.union([
  z.object({ result: z.record(z.string(), z.unknown()) }),
  errorSchema,
]);
const sourceOutputSchema = z.union([
  z.object({
    build: z.string(),
    id: z.string(),
    path: z.string(),
    language: z.string(),
    startLine: z.number().int(),
    endLine: z.number().int(),
    content: z.string(),
    corpus: z.string(),
    repository: z.string(),
  }),
  errorSchema,
]);

const searchInputSchema = z.object({
  query: z.string().min(2).max(200),
  types: z.array(z.string()).max(8).optional(),
  module: z.string().min(1).max(100).optional(),
  corpus: z.string().min(1).max(50).optional(),
  limit: z.number().int().min(1).max(50).optional(),
});

export function createDisCatSharpMcpServer(db: D1Database, onMetrics?: (metrics: SearchMetrics, errorCode?: string, queryLength?: number) => void): McpServer {
  const service = new SearchService(db);
  const server = new McpServer(
    { name: "DisCatSharp Documentation", version: "1.0.0", websiteUrl: "https://docs.dcs.aitsys.dev", description: "Search DisCatSharp and its official Extensions APIs, source, and conceptual documentation.", icons: [{ src: "https://docs.dcs.aitsys.dev/logo.svg", mimeType: "image/svg+xml" }] },
    { instructions: MCP_SERVER_INSTRUCTIONS },
  );

  server.registerTool(
    "search",
    {
      title: "Search DisCatSharp documentation",
      description: "Search API symbols and every conceptual page emitted by the DisCatSharp and official Extensions DocFX builds.",
      inputSchema: searchInputSchema,
      outputSchema: searchOutputSchema,
      annotations: { readOnlyHint: true, openWorldHint: false },
    },
    async (input) => executeTool(
      () => searchWithRelatedServer(() => service.search(input)),
      (errorCode) => onMetrics?.(service.metrics, errorCode, input.query.length),
    ),
  );

  server.registerTool(
    "find_symbol",
    {
      title: "Find a DisCatSharp API symbol",
      description: "Find overload-safe classes, methods, properties, fields, events, and other generated API symbols across DisCatSharp and official Extensions.",
      inputSchema: searchInputSchema,
      outputSchema: searchOutputSchema,
      annotations: { readOnlyHint: true, openWorldHint: false },
    },
    async (input) => executeTool(
      () => searchWithRelatedServer(() => service.search({ ...input, symbolsOnly: true })),
      (errorCode) => onMetrics?.(service.metrics, errorCode, input.query.length),
    ),
  );

  server.registerTool(
    "fetch",
    {
      title: "Fetch an indexed documentation result",
      description: "Fetch normalized documentation and metadata for a stable symbol: or document: ID returned by search or find_symbol.",
      inputSchema: z.object({ id: z.string().min(3).max(1_000) }),
      outputSchema: recordOutputSchema,
      annotations: { readOnlyHint: true, openWorldHint: false },
    },
    async ({ id }) => executeTool(async () => ({ result: await service.fetch(id) }), (errorCode) => onMetrics?.(service.metrics, errorCode, 0)),
  );

  server.registerTool(
    "get_source",
    {
      title: "Read indexed DisCatSharp source",
      description: "Read a bounded line range from a repository-relative source file referenced by the generated API index.",
      inputSchema: z.object({
        path: z.string().min(1).max(500),
        startLine: z.number().int().positive(),
        endLine: z.number().int().positive(),
      }),
      outputSchema: sourceOutputSchema,
      annotations: { readOnlyHint: true, openWorldHint: false },
    },
    async (input) => executeTool(() => service.getSource(input), (errorCode) => onMetrics?.(service.metrics, errorCode, 0)),
  );

  return server;
}

async function searchWithRelatedServer(operation: () => Promise<{ build: string; query: string; results: unknown[] }>) {
  const value = await operation();
  return value.results.length === 0
    ? { ...value, relatedServers: [discordDocsServer] }
    : value;
}

async function executeTool<T extends object>(operation: () => Promise<T>, onComplete?: (errorCode?: string) => void) {
  let errorCode: string | undefined;
  try {
    const value = await operation();
    return {
      content: [{ type: "text" as const, text: JSON.stringify(value) }],
      structuredContent: value,
    };
  } catch (error) {
    const details = error instanceof SearchError
      ? { code: error.code, message: error.message }
      : { code: "internal_error", message: "The documentation service could not complete the request." };
    errorCode = details.code;
    const value = { error: details };
    return {
      isError: true,
      content: [{ type: "text" as const, text: JSON.stringify(value) }],
      structuredContent: value,
    };
  } finally {
    onComplete?.(errorCode);
  }
}
