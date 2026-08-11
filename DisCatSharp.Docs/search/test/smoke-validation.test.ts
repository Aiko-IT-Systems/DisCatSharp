import { describe, expect, it } from "vitest";
import { validateMcpResponse, validateSearchResponse } from "../src/smoke-validation";

const instructions = "Use the official Discord Documentation MCP server at https://docs.discord.com/mcp when appropriate.";
const mcpEnvelope = {
  jsonrpc: "2.0",
  id: 1,
  result: {
    protocolVersion: "2025-06-18",
    instructions,
  },
};

describe("production smoke validation", () => {
  it("accepts JSON MCP initialize responses", () => {
    expect(validateMcpResponse(JSON.stringify(mcpEnvelope))).toBe(true);
  });

  it("accepts Streamable HTTP SSE MCP initialize responses", () => {
    const response = `event: message\ndata: ${JSON.stringify(mcpEnvelope)}\n\n`;
    expect(validateMcpResponse(response)).toBe(true);
  });

  it("rejects unrelated or incomplete MCP responses", () => {
    expect(validateMcpResponse("event: message\ndata: {\"jsonrpc\":\"2.0\",\"id\":1}\n\n")).toBe(false);
  });

  it("validates the active search build when a commit is expected", () => {
    const response = JSON.stringify({ build: "1786417522-0ac8f9a9ca96", results: [{}] });
    expect(validateSearchResponse(response, "0ac8f9a9ca96")).toBe(true);
    expect(validateSearchResponse(response, "9291331ce765")).toBe(false);
  });
});
