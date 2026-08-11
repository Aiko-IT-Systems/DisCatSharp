import { setTimeout as delay } from "node:timers/promises";

interface ApiEnvelope<T> { success: boolean; result: T | null; errors: Array<{ message: string }> }

const accountId = requireEnvironment("CLOUDFLARE_ACCOUNT_ID");
const apiToken = requireEnvironment("CLOUDFLARE_API_TOKEN");
const workerName = process.env.CLOUDFLARE_WORKER_NAME ?? "discatsharp-docs-search";
const response = await fetch(`https://api.cloudflare.com/client/v4/accounts/${accountId}/workers/subdomain`, {
  headers: { Authorization: `Bearer ${apiToken}` },
});
const envelope = await response.json() as ApiEnvelope<{ subdomain: string }>;
if (!response.ok || !envelope.success || !envelope.result?.subdomain) {
  throw new Error(`Unable to discover the Workers.dev subdomain: ${envelope.errors.map((error) => error.message).join("; ") || response.statusText}`);
}

const baseUrl = `https://${workerName}.${envelope.result.subdomain}.workers.dev`;
const search = await retry(() => fetch(`${baseUrl}/_search?q=DiscordGuild&limit=1`, { headers: { Accept: "application/json" } }), [200, 503]);
const searchBody = await search.text();
const searchValue = JSON.parse(searchBody) as { results?: unknown[]; error?: { code?: string } };
if (search.ok ? !searchValue.results?.length : searchValue.error?.code !== "index_not_ready") {
  throw new Error(`Workers.dev search smoke failed (${search.status}): ${searchBody}`);
}

const mcp = await retry(() => fetch(`${baseUrl}/mcp`, {
  method: "POST",
  headers: { "Content-Type": "application/json", Accept: "application/json, text/event-stream" },
  body: JSON.stringify({
    jsonrpc: "2.0",
    id: 1,
    method: "initialize",
    params: { protocolVersion: "2025-06-18", capabilities: {}, clientInfo: { name: "documentation-workflow", version: "1.0.0" } },
  }),
}));
const mcpBody = await mcp.text();
if (!mcp.ok || !mcpBody.includes("2025-06-18")) {
  throw new Error(`Workers.dev MCP smoke failed (${mcp.status}): ${mcpBody}`);
}

console.log(`Workers.dev search and MCP smoke passed at ${baseUrl}.`);

async function retry(operation: () => Promise<Response>, acceptedStatuses: number[] = [200]): Promise<Response> {
  let lastError: unknown;
  for (let attempt = 0; attempt < 6; attempt++) {
    try {
      const response = await operation();
      if (acceptedStatuses.includes(response.status)) return response;
      lastError = new Error(`Workers.dev returned HTTP ${response.status}.`);
      await response.body?.cancel();
    } catch (error) {
      lastError = error;
    }
    if (attempt < 5) await delay(5_000);
  }
  throw lastError instanceof Error ? lastError : new Error("Workers.dev did not become ready after six attempts.");
}

function requireEnvironment(name: string): string {
  const value = process.env[name];
  if (value === undefined || value.length === 0) throw new Error(`Missing required environment variable ${name}.`);
  return value;
}
