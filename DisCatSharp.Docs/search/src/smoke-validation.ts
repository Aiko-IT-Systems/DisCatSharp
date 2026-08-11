interface McpEnvelope {
  result?: {
    protocolVersion?: unknown;
    instructions?: unknown;
  };
}

interface SearchEnvelope {
  build?: unknown;
  results?: unknown[];
}

export function validateSearchResponse(body: string, expectedSha?: string): boolean {
  const value = parseJson<SearchEnvelope>(body);
  if (value === undefined || !Array.isArray(value.results) || value.results.length === 0 || typeof value.build !== "string") return false;
  if (!/^\d{10,}-[0-9a-f]{12}$/u.test(value.build)) return false;
  return expectedSha === undefined || value.build.endsWith(`-${expectedSha}`);
}

export function validateMcpResponse(body: string): boolean {
  for (const value of parseMcpEnvelopes(body)) {
    if (value.result?.protocolVersion === "2025-06-18"
      && typeof value.result.instructions === "string"
      && value.result.instructions.includes("https://docs.discord.com/mcp")) return true;
  }

  return false;
}

function parseMcpEnvelopes(body: string): McpEnvelope[] {
  const direct = parseJson<McpEnvelope>(body);
  if (direct !== undefined) return [direct];

  const envelopes: McpEnvelope[] = [];
  for (const event of body.split(/\r?\n\r?\n/gu)) {
    const data = event
      .split(/\r?\n/gu)
      .filter((line) => line.startsWith("data:"))
      .map((line) => line.slice(5).trimStart())
      .join("\n");
    if (data.length === 0) continue;

    const envelope = parseJson<McpEnvelope>(data);
    if (envelope !== undefined) envelopes.push(envelope);
  }

  return envelopes;
}

function parseJson<T>(value: string): T | undefined {
  try {
    return JSON.parse(value) as T;
  } catch {
    return undefined;
  }
}
