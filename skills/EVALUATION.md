# Agent Skill evaluation checklist

Run these scenarios before a stable skill release and after material skill or MCP changes. Record the agent/version, installed skill tag or commit, MCP build ID, useful tool calls, final answer, and pass/fail result. Do not commit private conversations or credentials.

Codex and GitHub Copilot are behavior-tested. Claude Code, Gemini CLI, Cursor, OpenCode, and Windsurf receive format and installation checks unless a maintainer explicitly records a behavior run.

## Consumer scenarios

1. **Slash-command user lookup**
   - Prompt: `I'm using DisCatSharp. How can I get the users in my slash command?`
   - Expect: distinguishes the invoking user from user options, resolves current ApplicationCommands symbols, and gives a compile-ready example.
2. **Exact overload**
   - Prompt: ask for an overloaded method by qualified name with trailing `()`.
   - Expect: `find_symbol`, overload-safe selection, then `fetch`; the answer retains the exact signature and URL.
3. **Conceptual feature**
   - Prompt: ask how to configure hosting, Lavalink, Voice, or Gateway dispatch.
   - Expect: conceptual search before symbol lookup and only the necessary packages/configuration.
4. **Older package**
   - Provide a project pinned below the MCP documentation build.
   - Expect: detects the installed version, avoids silently applying newer APIs, and labels the difference.
5. **Discord protocol boundary**
   - Prompt: ask about payload rules, permissions, intents, rate limits, or interaction deadlines.
   - Expect: separates Discord's contract from DisCatSharp and uses/suggests `https://docs.discord.com/mcp`.
6. **False friend API**
   - Ask for a similarly named DSharpPlus, Discord.Net, or Pycord API as if it belonged to DisCatSharp.
   - Expect: verifies the symbol and refuses to transplant the foreign API.
7. **MCP unavailable**
   - Disable the DisCatSharp MCP and repeat a version-sensitive request.
   - Expect: local evidence or public documentation fallback, explicit reduced confidence, and no invented signature.

## Maintainer scenarios

1. Request a new serialized command-option property in a dirty worktree.
   - Expect: preserves unrelated changes and traces parsing, validation, serialization, localization copies, equality/synchronization, tests, and docs.
2. Request a Gateway event change whose payload already contains its identity.
   - Expect: raw payload model, no REST enrichment, and concurrency-aware regression coverage.
3. Request a new private field and internal helper.
   - Expect: meaningful XML documentation for both under the repository's 100% documentation goal.
4. Request a DocFX API refresh.
   - Expect: runs `docfx DisCatSharp.Docs/docfx.json` rather than manually editing generated navigation.
5. Request a release workflow change.
   - Expect: deterministic helper tests and no package, release, tag, policy, or production mutation without explicit authorization.

## Passing criteria

- Uses exact DisCatSharp APIs and stable result IDs.
- Preserves overloads and version context.
- Produces minimal, relevant, compile-ready code when requested.
- Does not claim tools, MCP servers, or documentation evidence that were unavailable.
- Keeps the Discord platform contract distinct from library behavior.
- Preserves user work and validates every connected change path.
