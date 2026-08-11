import { createServer, type IncomingMessage } from "node:http";
import { readFile } from "node:fs/promises";
import { extname, resolve, sep } from "node:path";
import { unstable_startWorker } from "wrangler";

const siteRoot = resolve("../_site");
const worker = await unstable_startWorker({ config: "wrangler.jsonc" });
await worker.ready;

const server = createServer(async (request, response) => {
  try {
    const url = new URL(request.url ?? "/", "http://127.0.0.1:8080");
    if (url.pathname === "/_search" || url.pathname === "/_search/symbol" || url.pathname === "/mcp") {
      const body = request.method === "GET" || request.method === "HEAD"
        ? undefined
        : await readRequestBody(request);
      const proxied = await worker.fetch(`http://127.0.0.1${url.pathname}${url.search}`, {
        method: request.method,
        headers: request.headers as Record<string, string>,
        body,
      });
      response.writeHead(proxied.status, Object.fromEntries(proxied.headers));
      response.end(Buffer.from(await proxied.arrayBuffer()));
      return;
    }

    const file = await resolveSiteFile(url.pathname);
    const content = await readFile(file);
    response.writeHead(200, { "Content-Type": contentType(file), "Cache-Control": "no-store" });
    response.end(content);
  } catch (error) {
    const status = (error as NodeJS.ErrnoException).code === "ENOENT" ? 404 : 500;
    response.writeHead(status, { "Content-Type": "text/plain; charset=utf-8" });
    response.end(status === 404 ? "Not found" : "Local documentation preview failed.");
  }
});

await new Promise<void>((resolveReady) => server.listen(8080, "127.0.0.1", resolveReady));
console.log("DisCatSharp docs: http://127.0.0.1:8080/");
console.log(`DisCatSharp MCP: ${(await worker.url).toString().replace(/\/$/u, "")}/mcp`);
console.log("Press Ctrl+C to stop the local preview.");

const stop = async () => {
  server.closeAllConnections();
  await new Promise<void>((resolveClose) => server.close(() => resolveClose()));
  await worker.dispose();
  process.exit(0);
};
process.once("SIGINT", stop);
process.once("SIGTERM", stop);

async function resolveSiteFile(pathname: string): Promise<string> {
  const decoded = decodeURIComponent(pathname);
  if (decoded.includes("\0") || decoded.includes("\\")) throw Object.assign(new Error("Unsafe path"), { code: "ENOENT" });
  const relative = decoded.replace(/^\/+/, "");
  const candidates = relative.length === 0 || relative.endsWith("/")
    ? [`${relative}index.html`]
    : [relative, `${relative}.html`, `${relative}/index.html`];
  const rootPrefix = siteRoot.endsWith(sep) ? siteRoot : siteRoot + sep;
  for (const candidate of candidates) {
    const fullPath = resolve(siteRoot, candidate);
    if (!fullPath.startsWith(rootPrefix)) continue;
    try {
      await readFile(fullPath);
      return fullPath;
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code !== "ENOENT") throw error;
    }
  }
  throw Object.assign(new Error("Not found"), { code: "ENOENT" });
}

function contentType(path: string): string {
  return ({
    ".css": "text/css; charset=utf-8",
    ".html": "text/html; charset=utf-8",
    ".js": "text/javascript; charset=utf-8",
    ".json": "application/json; charset=utf-8",
    ".png": "image/png",
    ".svg": "image/svg+xml",
    ".webp": "image/webp",
    ".woff": "font/woff",
    ".woff2": "font/woff2",
  } as Record<string, string>)[extname(path).toLowerCase()] ?? "application/octet-stream";
}

async function readRequestBody(request: IncomingMessage): Promise<Uint8Array> {
  const chunks: Buffer[] = [];
  for await (const chunk of request) chunks.push(Buffer.isBuffer(chunk) ? chunk : Buffer.from(chunk));
  return new Uint8Array(Buffer.concat(chunks));
}
