import { unstable_startWorker } from "wrangler";

const worker = await unstable_startWorker({ config: "wrangler.jsonc" });
try {
  await worker.ready;
  const search = await worker.fetch("http://localhost/_search?q=DiscordGuild&limit=3");
  const searchBody = await search.json() as { build?: string; results?: Array<{ id: string; family: string; type: string }>; error?: unknown };
  if (!search.ok || (searchBody.results?.length ?? 0) === 0) {
    throw new Error(`Local search smoke failed (${search.status}): ${JSON.stringify(searchBody)}`);
  }
  if (searchBody.results?.[0]?.family !== "symbol") throw new Error("An exact symbol search did not rank a symbol first.");
  if (!/^\d{10,}-[0-9a-f]{12}$/u.test(searchBody.build ?? "")) throw new Error(`Local search returned an invalid build marker: ${searchBody.build ?? "missing"}.`);

  const conceptual = await worker.fetch("http://localhost/_search?q=voice&type=conceptual&limit=3");
  if (!conceptual.ok) throw new Error(`Local conceptual search smoke failed with HTTP ${conceptual.status}.`);
  const conceptualBody = await conceptual.json() as { results?: Array<{ id: string; family: string; type: string }> };
  if (!conceptualBody.results?.length || conceptualBody.results.some((result) => result.family !== "conceptual" || !result.id.startsWith("document:"))) {
    throw new Error(`Local conceptual alias returned an unexpected result: ${JSON.stringify(conceptualBody)}`);
  }
  const articles = await worker.fetch("http://localhost/_search?q=voice&type=article&limit=10");
  const articleBody = await articles.json() as { results?: Array<{ type: string }> };
  if (!articles.ok || !articleBody.results?.length || articleBody.results.some((result) => result.type !== "article")) {
    throw new Error(`Local literal article filter returned an unexpected result: ${JSON.stringify(articleBody)}`);
  }
  const changelogs = await worker.fetch("http://localhost/_search?q=version&type=changelog&limit=10");
  const changelogBody = await changelogs.json() as { results?: Array<{ type: string }> };
  if (!changelogs.ok || !changelogBody.results?.length || changelogBody.results.some((result) => result.type !== "changelog")) {
    throw new Error(`Local changelog filter returned an unexpected result: ${JSON.stringify(changelogBody)}`);
  }
  const naturalLanguage = await worker.fetch("http://localhost/_search?q=voice%20configuration&limit=5");
  const naturalBody = await naturalLanguage.json() as { results?: unknown[] };
  if (!naturalLanguage.ok || !naturalBody.results?.length) {
    throw new Error(`Local natural-language search returned no results: ${JSON.stringify(naturalBody)}`);
  }
  const qualified = await worker.fetch("http://localhost/_search/symbol?q=DisCatSharp.Entities.DiscordGuild.GetMemberAsync()&limit=5");
  const qualifiedBody = await qualified.json() as { results?: Array<{ id: string }> };
  if (!qualified.ok || !qualifiedBody.results?.[0]?.id.startsWith("symbol:DisCatSharp.Entities.DiscordGuild.GetMemberAsync")) {
    throw new Error(`Local qualified method search ranked an unexpected result: ${JSON.stringify(qualifiedBody)}`);
  }
  const overloads = await worker.fetch("http://localhost/_search/symbol?q=DiscordThreadChannel.GetMemberAsync&limit=10");
  const overloadBody = await overloads.json() as { results?: Array<{ id: string; url: string }> };
  const overloadResults = overloadBody.results?.filter((result) => result.id.startsWith("symbol:DisCatSharp.Entities.DiscordThreadChannel.GetMemberAsync")) ?? [];
  if (!overloads.ok || overloadResults.length < 2 || new Set(overloadResults.map((result) => result.url)).size !== overloadResults.length) {
    throw new Error(`Local overload search did not preserve overload-safe results: ${JSON.stringify(overloadBody)}`);
  }
  const invalid = await worker.fetch("http://localhost/_search?q=x&unknown=true");
  if (invalid.status !== 400) throw new Error(`Local HTTP validation smoke expected 400, received ${invalid.status}.`);

  const mcpPreflight = await worker.fetch("http://localhost/mcp", {
    method: "OPTIONS",
    headers: {
      Origin: "http://localhost:6274",
      "Access-Control-Request-Method": "POST",
      "Access-Control-Request-Headers": "content-type",
    },
  });
  if (!mcpPreflight.ok || !mcpPreflight.headers.get("Access-Control-Allow-Methods")?.includes("POST")) {
    throw new Error(`Local MCP browser preflight failed with HTTP ${mcpPreflight.status}.`);
  }

  const initialized = await mcpRequest(1, "initialize", {
    protocolVersion: "2025-06-18",
    capabilities: {},
    clientInfo: { name: "local-smoke", version: "1.0.0" },
  });
  if ((initialized.result as { protocolVersion?: string } | undefined)?.protocolVersion !== "2025-06-18") {
    throw new Error(`Local MCP initialization returned an unexpected response: ${JSON.stringify(initialized)}`);
  }
  const instructions = (initialized.result as { instructions?: string } | undefined)?.instructions;
  if (!instructions?.includes("https://docs.discord.com/mcp")) {
    throw new Error(`Local MCP initialization did not advertise the official Discord documentation MCP: ${JSON.stringify(initialized)}`);
  }

  const rejectedOrigin = await worker.fetch("http://localhost/mcp", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json, text/event-stream",
      Origin: "https://untrusted.example",
    },
    body: JSON.stringify({
      jsonrpc: "2.0",
      id: 99,
      method: "initialize",
      params: { protocolVersion: "2025-06-18", capabilities: {}, clientInfo: { name: "origin-smoke", version: "1.0.0" } },
    }),
  });
  if (rejectedOrigin.ok) throw new Error("Local MCP accepted an untrusted browser Origin.");

  const tools = await mcpRequest(2, "tools/list", {});
  const toolNames = ((tools.result as { tools?: Array<{ name: string }> } | undefined)?.tools ?? []).map((tool) => tool.name).sort();
  if (JSON.stringify(toolNames) !== JSON.stringify(["fetch", "find_symbol", "get_source", "search"])) {
    throw new Error(`Local MCP tool list is incomplete: ${JSON.stringify(toolNames)}`);
  }

  const emptySearch = await mcpRequest(10, "tools/call", { name: "search", arguments: { query: "zzzzzzzzzzzzzzzzzzzz", limit: 1 } });
  const relatedServers = (emptySearch.result as {
    structuredContent?: { relatedServers?: Array<{ url?: string }> };
  } | undefined)?.structuredContent?.relatedServers;
  if (relatedServers?.[0]?.url !== "https://docs.discord.com/mcp") {
    throw new Error(`Local MCP empty search did not suggest the official Discord documentation MCP: ${JSON.stringify(emptySearch)}`);
  }

  const toolSearch = await mcpRequest(3, "tools/call", { name: "search", arguments: { query: "DiscordGuild", limit: 1 } });
  const toolSearchContent = (toolSearch.result as { structuredContent?: { build?: string; results?: Array<{ id: string }> } } | undefined)?.structuredContent;
  const searchResult = toolSearchContent?.results?.[0];
  if (searchResult === undefined) throw new Error(`Local MCP search returned no result: ${JSON.stringify(toolSearch)}`);
  if (toolSearchContent?.build !== searchBody.build) {
    throw new Error(`HTTP/MCP build parity failed: HTTP returned '${searchBody.build}', MCP returned '${toolSearchContent?.build}'.`);
  }
  if (searchResult.id !== searchBody.results?.[0]?.id) {
    throw new Error(`HTTP/MCP search parity failed: HTTP returned '${searchBody.results?.[0]?.id}', MCP returned '${searchResult.id}'.`);
  }

  const toolFetch = await mcpRequest(4, "tools/call", { name: "fetch", arguments: { id: searchResult.id } });
  const fetched = (toolFetch.result as { structuredContent?: { result?: Record<string, unknown> } } | undefined)?.structuredContent?.result;
  if (fetched?.build !== searchBody.build) {
    throw new Error(`MCP fetch returned build '${String(fetched?.build)}' instead of '${searchBody.build}'.`);
  }
  const sourcePath = fetched?.sourcePath;
  const sourceStartLine = fetched?.sourceStartLine;
  const sourceEndLine = fetched?.sourceEndLine;
  if (typeof sourcePath !== "string" || typeof sourceStartLine !== "number" || typeof sourceEndLine !== "number") {
    throw new Error(`Local MCP fetch did not return indexed source metadata: ${JSON.stringify(toolFetch)}`);
  }

  const toolSymbol = await mcpRequest(5, "tools/call", { name: "find_symbol", arguments: { query: "GetMemberAsync", limit: 2 } });
  if (((toolSymbol.result as { structuredContent?: { results?: unknown[] } } | undefined)?.structuredContent?.results?.length ?? 0) === 0) {
    throw new Error(`Local MCP find_symbol returned no result: ${JSON.stringify(toolSymbol)}`);
  }

  const toolSource = await mcpRequest(6, "tools/call", {
    name: "get_source",
    arguments: { path: sourcePath, startLine: sourceStartLine, endLine: Math.min(sourceStartLine + 2, sourceEndLine) },
  });
  const sourceContent = (toolSource.result as { structuredContent?: { build?: unknown; content?: unknown } } | undefined)?.structuredContent;
  if (typeof sourceContent?.content !== "string" || sourceContent.build !== searchBody.build) {
    throw new Error(`Local MCP get_source did not return content: ${JSON.stringify(toolSource)}`);
  }

  const unsafeSource = await mcpRequest(7, "tools/call", { name: "get_source", arguments: { path: "../secret", startLine: 1, endLine: 1 } });
  const unsafeError = (unsafeSource.result as { structuredContent?: { error?: { code?: string } }; isError?: boolean } | undefined);
  if (unsafeError?.structuredContent?.error?.code !== "unsafe_source_path") {
    throw new Error(`Local MCP source protection returned an unexpected result: ${JSON.stringify(unsafeSource)}`);
  }

  const conceptualId = conceptualBody.results[0]!.id;
  const conceptualFetch = await mcpRequest(8, "tools/call", { name: "fetch", arguments: { id: conceptualId } });
  const conceptualResult = (conceptualFetch.result as { structuredContent?: { result?: { id?: string; family?: string; type?: string } } } | undefined)?.structuredContent?.result;
  if (conceptualResult?.id !== conceptualId || conceptualResult.family !== "conceptual") {
    throw new Error(`Local MCP fetch did not preserve the stable conceptual document ID: ${JSON.stringify(conceptualFetch)}`);
  }

  const legacyFetch = await mcpRequest(9, "tools/call", { name: "fetch", arguments: { id: "article:legacy" } });
  const legacyError = (legacyFetch.result as { structuredContent?: { error?: { code?: string } } } | undefined)?.structuredContent?.error?.code;
  if (legacyError !== "invalid_id") {
    throw new Error(`Local MCP fetch accepted a classification-prefixed conceptual ID: ${JSON.stringify(legacyFetch)}`);
  }
  console.log(`Local search, conceptual filtering, and all four MCP tools passed at ${await worker.url}`);
} finally {
  await worker.dispose();
}

async function mcpRequest(id: number, method: string, params: Record<string, unknown>): Promise<Record<string, unknown>> {
  const response = await worker.fetch("http://localhost/mcp", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json, text/event-stream",
      "MCP-Protocol-Version": "2025-06-18",
      Origin: "http://localhost:6274",
    },
    body: JSON.stringify({ jsonrpc: "2.0", id, method, params }),
  });
  const body = await response.text();
  if (!response.ok) throw new Error(`Local MCP request '${method}' failed (${response.status}): ${body}`);
  const data = body.split(/\r?\n/u).find((line) => line.startsWith("data: "))?.slice(6) ?? body;
  return JSON.parse(data) as Record<string, unknown>;
}
