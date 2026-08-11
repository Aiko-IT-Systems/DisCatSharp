---
name: use-discatsharp
description: Build, explain, migrate, or troubleshoot Discord applications that use DisCatSharp. Use for DisCatSharp setup, configuration, clients, events, intents, slash commands, CommandsNext, interactivity, hosting, Lavalink, voice, entities, REST operations, exact API lookup, overload selection, and version-aware C# examples.
license: MIT
---

# Use DisCatSharp

Answer from the consuming project's DisCatSharp version and verified DisCatSharp evidence. The public documentation MCP covers the main library and official DisCatSharp.Extensions; prefer it when available. Do not infer APIs from DSharpPlus, Discord.Net, Pycord, or another Discord library.

## Establish the project context

1. Inspect the local project when available. Determine:
   - The installed DisCatSharp package versions from project files, central package files, lock files, or restored assets.
   - The target framework and hosting model.
   - Which DisCatSharp modules are already referenced.
2. If no project is available, ask for the DisCatSharp version only when the answer materially depends on it. Otherwise state which current API version the answer targets.
3. Treat live project source and installed package metadata as authoritative for that project. The public MCP normally describes the latest documentation build and can be newer than the user's packages.

Read [references/modules.md](references/modules.md) when choosing packages or extensions.

## Retrieve evidence

When the DisCatSharp documentation MCP is connected, use the smallest useful sequence:

1. Call `search` for a broad feature, workflow, or conceptual question. Search both corpora unless the project or question clearly concerns only `main` or `extensions`.
2. Call `find_symbol` for exact types, members, qualified names, and overloads. Use the corpus filter to disambiguate types that exist in both repositories.
3. Call `fetch` with the returned `symbol:` or `document:` ID before relying on a result's full documentation.
4. Call `get_source` only for a repository-relative path and bounded range returned by indexed symbol metadata, and only when documented behavior is insufficient.

Preserve overload-specific IDs and URLs. A search result is a candidate, not complete documentation. Do not fabricate a UID, document ID, source path, signature, or overload.

For Discord platform behavior—payloads, permissions, intents, Gateway events, rate limits, REST semantics, or application-command rules—consult the official Discord Documentation MCP at `https://docs.discord.com/mcp` when it is available. Clearly separate Discord's contract from DisCatSharp's representation of it.

Read [references/evidence-and-fallbacks.md](references/evidence-and-fallbacks.md) when MCP is unavailable, results conflict with the installed version, or a lookup returns no match.

## Produce an answer

- Use exact DisCatSharp names, signatures, packages, and namespaces supported by the evidence.
- Give a minimal compile-ready example when code is requested. Include relevant intents, configuration, extension registration, dependency injection, and disposal/lifecycle details.
- Explain which event or command model is being used. Do not casually mix ApplicationCommands and CommandsNext.
- Preserve asynchronous behavior and cancellation. Do not introduce blocking `.Result` or `.Wait()` calls.
- Mention version requirements, experimental status, required Discord configuration, and migration impact when relevant.
- Link the most useful DisCatSharp page or exact overload. Avoid dumping search results without resolving them.
- If evidence remains incomplete, say what was verified, what is inferred, and what the user should check. Never hide uncertainty behind plausible-looking code.

## Common traps

- Similar APIs in other Discord libraries are not interchangeable.
- A type existing in the current documentation does not prove it exists in an older installed package.
- Cache state can be partial and events may require intents.
- Discord REST bucket FIFO and Gateway user-handler ordering are separate concerns.
- Gateway cache mutation remains ordered, while user handlers are concurrent by default unless `GatewayAdvancedConfiguration.DispatchMode` uses `SequentialHandlers`.
- Slash-command interactions have response deadlines and response/follow-up rules defined by Discord.
