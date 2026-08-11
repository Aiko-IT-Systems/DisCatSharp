# DisCatSharp documentation search

This package contains the Cloudflare D1 search Worker, public stateless MCP endpoint, database migration, incremental synchronization, tests, and local preview tooling.

## Architecture

The .NET indexer runs after DocFX. It uses `_site/manifest.json` as the authoritative page inventory, generated managed-reference YAML and `_site/xrefmap.yml` for overload-safe API metadata, Markdown sources for conceptual content, and Roslyn for declaration ranges and source-friendly chunk boundaries. The generated `DisCatSharp.Docs/obj/search/search-index.json` artifact is build output and is not committed.

D1 stores:

- `sync_state`: readiness, schema/build metadata, counts, modules, and available filter types.
- `symbols`: one row per overload-safe DocFX UID.
- `documents`: one row per manifest `Conceptual` output, always with `family = conceptual` and a more specific `kind`.
- `source_chunks`: the only source paths and ranges exposed by `get_source`.
- External-content `symbols_fts` and `documents_fts` tables maintained by insert, update, and delete triggers.

HTTP and MCP use the same `SearchService`. Search first batches indexed exact lookups, then only issues FTS queries for result families that still need fuzzy candidates. Exact UID, full-name, qualified-name, and short-name tiers score above document exact matches and BM25 results. Final ties use title and typed ID for deterministic ordering.

Successful search, fetch, and source responses include `build` as `<generated-at Unix timestamp>-<12-character source commit>`, derived from the ready D1 `sync_state`. HTTP and MCP therefore expose the same deterministic corpus identity, and production deployment smoke tests require it to match the workflow commit.

The MCP initialization instructions identify this server as authoritative for DisCatSharp and direct agents to the official Discord Documentation MCP at `https://docs.discord.com/mcp` for underlying Discord platform semantics when that server is available. A zero-result MCP `search` or `find_symbol` response also includes the same endpoint under `relatedServers`; successful DisCatSharp results remain uncluttered.

Conceptual identity is classification-independent: every conceptual result uses `document:<document-key>`, where the key comes only from its normalized canonical output URL. `family = conceptual` and `kind` (`article`, `changelog`, `native`, `vs`, `api`, or a future label) are metadata. Reclassifying or moving a source page without changing its canonical output URL therefore preserves its external ID; changing the canonical URL intentionally creates a new identity.

## Full local test plan

No Cloudflare account, token, database, Worker, or route is touched by this flow. Use Node 24 (the CI version) and run commands from the repository root unless a step says otherwise.

1. Build the current documentation and its manifest-driven artifact:

   ```powershell
   docfx DisCatSharp.Docs/docfx.json
   Remove-Item DisCatSharp.Docs/_site/index.json -ErrorAction SilentlyContinue
   Remove-Item DisCatSharp.Docs/_site/public/search-worker.min.js -ErrorAction SilentlyContinue
   dotnet run --project DisCatSharp.Tools/DisCatSharp.Docs.SearchIndexer/DisCatSharp.Docs.SearchIndexer.csproj -c Release -- --repo . --docs DisCatSharp.Docs --site DisCatSharp.Docs/_site --output DisCatSharp.Docs/obj/search/search-index.json
   ```

2. Run the indexer and Worker unit/integration suites:

   ```powershell
   dotnet test DisCatSharp.Tools/DisCatSharp.Docs.SearchIndexer.Tests/DisCatSharp.Docs.SearchIndexer.Tests.csproj -c Release
   Set-Location DisCatSharp.Docs/search
   npm ci
   npm run types:check
   npm run build
   npm test
   ```

3. Create, migrate, seed, and smoke-test local D1:

   ```powershell
   npm run migrate:local
   npm run sync:local
   npm run smoke:local
   ```

   The smoke test exercises HTTP validation, conceptual filtering, all four MCP tools, source-path protections, and identical leading results from HTTP and MCP.

4. Start the combined DocFX site and local Worker preview:

   ```powershell
   npm run local
   ```

5. Open `http://127.0.0.1:8080/` and exercise:

   - Two-character minimum, 200 ms debounce, loading, empty, retry, and stale-response behavior.
   - All, Classes, Methods, Properties, Articles, and Changelogs filters.
   - Articles returning every conceptual kind; Changelogs returning only changelog records.
   - Arrow Up/Down, Enter, Escape, focus ring, and narrow/mobile layout.
   - Exact UIDs, fully qualified methods, overloads, prefixes, and natural-language queries.
   - Dark-theme alignment and readable title, kind badge, summary, hover, and selected-result organization.

6. Connect MCP Inspector or another MCP client to `http://127.0.0.1:8787/mcp` and verify `search`, `find_symbol`, `fetch`, and `get_source`. Inspector running on `localhost` or `127.0.0.1` is accepted by Origin validation.

The local database lives under the gitignored `.wrangler/` directory. Stop the preview before reseeding it. Delete that directory only when intentionally testing a completely fresh local database.

## Production prerequisites

Configure the repository with:

- Secret `CLOUDFLARE_API_TOKEN`, limited to D1, Worker Scripts, and Worker Routes for the AITSYS account and `aitsys.dev` zone.
- Variable `CLOUDFLARE_ACCOUNT_ID`.
- Existing `NYUW_TOKEN_GH` for the generated documentation repository.

No account ID, database UUID, token, or route credential is committed. The workflow discovers or provisions D1 by the configured name `discatsharp-docs-search`, then writes ignored production Wrangler configs with the discovered UUID. The committed name-only configs remain safe local/build templates.

## Production rollout and recovery

The production workflow performs this order and does not publish new HTML if any stage fails:

1. Build DocFX, remove/assert absence of stock browser search assets, generate the manifest-driven artifact, and run all .NET/Worker validation.
2. Discover or create D1 by name, inject its UUID into ignored runtime configs, and, when the Worker does not exist yet, deploy the route-free bootstrap so a Workers.dev endpoint exists.
3. Apply additive D1 migrations and hash-synchronize changed records. `sync_state.ready` remains false during an initial seed and is set last.
4. On the first deployment, discover the account Workers.dev subdomain through the Cloudflare API and smoke-test search plus MCP before attaching custom-domain routes.
5. Deploy the narrow `docs.dcs.aitsys.dev/_search*` and `docs.dcs.aitsys.dev/mcp*` routes, smoke-test both, then package and publish `_site`.

An interrupted initial seed is resumed by rerunning the workflow: existing IDs/content hashes are read, completed rows are skipped, missing or changed rows are upserted, stale rows are removed only after upserts, and `sync_state` is updated last. The document-key conflict target also safely migrates any pre-V1 classification-prefixed local rows to `document:` before their stale IDs are deleted. Subsequent builds use the same delta process. Review the sync summary and `rowsWritten` log before retrying a very large seed; if the D1 Free daily write allowance is near exhaustion, wait for its daily reset and rerun rather than deleting the database.

For an intentional manual rollout from `DisCatSharp.Docs/search`:

```powershell
npm ci
npx wrangler d1 info discatsharp-docs-search
# If the preceding command reports that the database is absent:
npx wrangler d1 create discatsharp-docs-search
npm run prepare:production
# Only when `wrangler deployments list --config wrangler.production.json --json` reports no Worker:
npx wrangler deploy --config wrangler.bootstrap.production.json --minify
npm run migrate:remote
npm run sync -- ../obj/search/search-index.json
npm run smoke:workers-dev
npm run deploy
```

Only run those commands with the required Cloudflare environment values and explicit approval to mutate production. PR documentation workflows never run them; previews build and validate their own artifact but query the production `/_search` endpoint.
