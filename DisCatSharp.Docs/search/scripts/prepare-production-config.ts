import { readFile, writeFile } from "node:fs/promises";
import { resolve } from "node:path";
import { setTimeout as delay } from "node:timers/promises";
import { bindProductionDatabase, type WranglerConfiguration } from "../src/wrangler-config";

interface ApiEnvelope<T> {
  success: boolean;
  result: T;
  errors: Array<{ message: string }>;
}

const accountId = requireEnvironment("CLOUDFLARE_ACCOUNT_ID");
const apiToken = requireEnvironment("CLOUDFLARE_API_TOKEN");
const databaseName = process.env.CLOUDFLARE_D1_DATABASE_NAME ?? "discatsharp-docs-search";
const databaseId = await discoverDatabaseId();

await Promise.all([
  writeConfiguredFile("wrangler.jsonc", "wrangler.production.json", databaseId),
  writeConfiguredFile("wrangler.bootstrap.jsonc", "wrangler.bootstrap.production.json", databaseId),
]);
console.log(JSON.stringify({ database: databaseName, productionConfigsReady: true }));

async function writeConfiguredFile(source: string, destination: string, databaseId: string): Promise<void> {
  const template = JSON.parse(await readFile(resolve(source), "utf8")) as WranglerConfiguration;
  const configured = bindProductionDatabase(template, databaseName, databaseId);
  await writeFile(resolve(destination), JSON.stringify(configured, null, 2) + "\n", "utf8");
}

function requireEnvironment(name: string): string {
  const value = process.env[name];
  if (value === undefined || value.length === 0) throw new Error(`Missing required environment variable ${name}.`);
  return value;
}

async function discoverDatabaseId(): Promise<string> {
  for (let attempt = 0; attempt < 5; attempt++) {
    const response = await fetchWithRetry(`https://api.cloudflare.com/client/v4/accounts/${accountId}/d1/database?name=${encodeURIComponent(databaseName)}&per_page=100`, {
      headers: { Authorization: `Bearer ${apiToken}` },
    });
    const envelope = await response.json() as ApiEnvelope<Array<{ name: string; uuid: string }>>;
    if (!response.ok || !envelope.success) {
      throw new Error(`Unable to discover the D1 database: ${envelope.errors.map((error) => error.message).join("; ") || response.statusText}`);
    }
    const matches = envelope.result.filter((database) => database.name === databaseName);
    if (matches.length === 1) return matches[0]!.uuid;
    if (matches.length > 1) throw new Error(`Expected exactly one D1 database named '${databaseName}', found ${matches.length}.`);
    if (attempt < 4) await delay(250 * 2 ** attempt);
  }
  throw new Error(`No D1 database named '${databaseName}' became visible after five attempts.`);
}

async function fetchWithRetry(url: string, init: RequestInit): Promise<Response> {
  let lastError: unknown;
  for (let attempt = 0; attempt < 5; attempt++) {
    try {
      const response = await fetch(url, init);
      if (response.status !== 429 && response.status < 500) return response;
      lastError = new Error(`Cloudflare API returned HTTP ${response.status}.`);
      await response.body?.cancel();
    } catch (error) {
      lastError = error;
    }
    if (attempt < 4) await delay(250 * 2 ** attempt);
  }
  throw lastError instanceof Error ? lastError : new Error("Cloudflare API request failed after five attempts.");
}
