import { setTimeout as delay } from "node:timers/promises";
import { validateMcpResponse, validateSearchResponse } from "../src/smoke-validation";

const baseUrl = (process.env.DCS_SEARCH_BASE_URL ?? "https://docs.dcs.aitsys.dev").replace(/\/$/u, "");
const expectedCommit = process.env.EXPECTED_BUILD_SHA?.trim().toLowerCase().slice(0, 12);
const searchQuery = process.env.DCS_SEARCH_QUERY ?? "DiscordGuild";
const searchCorpus = process.env.DCS_SEARCH_CORPUS;
const deadline = Date.now() + 120_000;
let attempt = 0;
let searchReady = false;
let mcpReady = false;
let lastSearchResponse = "";
let lastMcpResponse = "";

while (Date.now() < deadline) {
  attempt++;

  if (!searchReady) {
    const parameters = new URLSearchParams({ q: searchQuery, limit: "1" });
    if (searchCorpus !== undefined) parameters.set("corpus", searchCorpus);
    lastSearchResponse = await request(`${baseUrl}/_search?${parameters}`);
    searchReady = validateSearchResponse(lastSearchResponse, expectedCommit);
  }

  if (!mcpReady) {
    lastMcpResponse = await request(`${baseUrl}/mcp`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json, text/event-stream",
      },
      body: JSON.stringify({
        jsonrpc: "2.0",
        id: 1,
        method: "initialize",
        params: {
          protocolVersion: "2025-06-18",
          capabilities: {},
          clientInfo: { name: "documentation-workflow", version: "1.0.0" },
        },
      }),
    });
    mcpReady = validateMcpResponse(lastMcpResponse);
  }

  if (searchReady && mcpReady) {
    console.log(`Production search and MCP became ready after ${attempt} attempt(s).`);
    process.exit(0);
  }

  console.log(`Waiting for Cloudflare route propagation (attempt ${attempt}, search=${searchReady}, mcp=${mcpReady})...`);
  if (Date.now() < deadline) await delay(5_000);
}

console.error("Production search and MCP did not become ready within 120 seconds.");
console.error(`Last search response (first 1 KiB):\n${lastSearchResponse.slice(0, 1_024)}`);
console.error(`Last MCP response (first 1 KiB):\n${lastMcpResponse.slice(0, 1_024)}`);
process.exit(1);

async function request(url: string, init?: RequestInit): Promise<string> {
  try {
    const response = await fetch(url, { ...init, signal: AbortSignal.timeout(10_000) });
    return await response.text();
  } catch (error) {
    return error instanceof Error ? `${error.name}: ${error.message}` : "Unknown request failure.";
  }
}
