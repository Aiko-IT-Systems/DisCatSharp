import { unstable_startWorker } from "wrangler";

const corpus = process.env.DCS_SEARCH_CORPUS ?? "main";
const query = process.env.DCS_SEARCH_QUERY ?? "DiscordGuild";
const expectedRepository = process.env.DCS_SEARCH_REPOSITORY;
const expectedSite = process.env.DCS_SEARCH_SITE_BASE_URL?.replace(/\/$/u, "");
const worker = await unstable_startWorker({ config: "wrangler.jsonc" });

try {
  await worker.ready;
  const parameters = new URLSearchParams({ q: query, corpus, limit: "5" });
  const search = await worker.fetch(`http://localhost/_search?${parameters}`);
  const body = await search.json() as {
    build?: string;
    results?: Array<{ id: string; corpus: string; repository: string; url: string }>;
    error?: unknown;
  };
  if (!search.ok || !body.results?.length) {
    throw new Error(`Local '${corpus}' corpus search failed (${search.status}): ${JSON.stringify(body)}`);
  }
  if (body.results.some((result) => result.corpus !== corpus)) {
    throw new Error(`Local '${corpus}' search leaked another corpus: ${JSON.stringify(body.results)}`);
  }
  if (expectedRepository !== undefined && body.results.some((result) => result.repository !== expectedRepository)) {
    throw new Error(`Local '${corpus}' search returned unexpected repository metadata: ${JSON.stringify(body.results)}`);
  }
  if (expectedSite !== undefined && body.results.some((result) => !result.url.startsWith(`${expectedSite}/`))) {
    throw new Error(`Local '${corpus}' search returned a non-canonical URL: ${JSON.stringify(body.results)}`);
  }

  const initialized = await mcpRequest(1, "initialize", {
    protocolVersion: "2025-06-18",
    capabilities: {},
    clientInfo: { name: "corpus-smoke", version: "1.0.0" },
  });
  if ((initialized.result as { protocolVersion?: string } | undefined)?.protocolVersion !== "2025-06-18") {
    throw new Error(`Local MCP initialization failed: ${JSON.stringify(initialized)}`);
  }
  const searched = await mcpRequest(2, "tools/call", { name: "search", arguments: { query, corpus, limit: 1 } });
  const result = (searched.result as { structuredContent?: { build?: string; results?: Array<{ id: string; corpus: string }> } } | undefined)?.structuredContent;
  const searchedResult = result?.results?.[0];
  if (result?.build !== body.build || searchedResult?.corpus !== corpus) {
    throw new Error(`Local HTTP/MCP corpus parity failed: ${JSON.stringify(searched)}`);
  }
  const fetched = await mcpRequest(3, "tools/call", { name: "fetch", arguments: { id: searchedResult.id } });
  const fetchedResult = (fetched.result as { structuredContent?: { result?: { corpus?: string; repository?: string } } } | undefined)?.structuredContent?.result;
  if (fetchedResult?.corpus !== corpus || (expectedRepository !== undefined && fetchedResult.repository !== expectedRepository)) {
    throw new Error(`Local MCP fetch lost corpus metadata: ${JSON.stringify(fetched)}`);
  }

  console.log(`Local '${corpus}' corpus search and MCP smoke passed at ${await worker.url}.`);
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
