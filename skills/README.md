# DisCatSharp Agent Skills

The repository publishes two portable [Agent Skills](https://agentskills.io/specification):

- `use-discatsharp` for building and troubleshooting applications that consume DisCatSharp.
- `maintain-discatsharp` for contributing to this repository.

The skills are versioned by the repository's existing release tags. An unpinned `gh skill install` resolves the latest stable GitHub Release; select a matching nightly tag or `main` explicitly when working against prerelease code. Until the first stable release containing these skills is published, install from `main`.

## Install

Install one skill for a supported agent at project scope:

```shell
gh skill install Aiko-IT-Systems/DisCatSharp use-discatsharp --agent codex
gh skill install Aiko-IT-Systems/DisCatSharp maintain-discatsharp --agent github-copilot
gh skill install Aiko-IT-Systems/DisCatSharp use-discatsharp --agent codex --pin main
```

Use `--scope user` to install it for every project. Pin a release with `use-discatsharp@v10.7.1` or a commit with `--pin <sha>`. `gh skill` also supports Claude Code, Gemini CLI, Cursor, OpenCode, Windsurf, and other Agent Skills clients.

An alternative installer is:

```shell
npx skills add https://github.com/Aiko-IT-Systems/DisCatSharp --skill use-discatsharp
```

For manual installation, copy the complete selected skill directory, including `references/` and `agents/`, into the client's skill directory.

## Connect the documentation MCP

The public server is stateless and does not require authentication:

`https://docs.dcs.aitsys.dev/mcp`

### Codex

Add this to the user or trusted project `config.toml`:

```toml
[mcp_servers.discatsharp]
url = "https://docs.dcs.aitsys.dev/mcp"
```

The Codex app, CLI, and IDE integration share the same MCP configuration.

### GitHub Copilot CLI

```shell
copilot mcp add --transport http discatsharp https://docs.dcs.aitsys.dev/mcp
```

### Claude Code

```shell
claude mcp add --transport http --scope user discatsharp https://docs.dcs.aitsys.dev/mcp
```

### Gemini CLI

```shell
gemini mcp add --transport http --scope user discatsharp https://docs.dcs.aitsys.dev/mcp
```

Other clients should add the same URL as a Streamable HTTP MCP server named `discatsharp`.

The server exposes `search`, `find_symbol`, `fetch`, and `get_source`. It is authoritative for the current DisCatSharp documentation corpus. For the Discord platform contract itself, also consider Discord's official MCP at `https://docs.discord.com/mcp`.

## Support status

- Behavior-tested: Codex and GitHub Copilot.
- Format and installation verified: Claude Code, Gemini CLI, Cursor, OpenCode, Windsurf, and compatible Agent Skills hosts.

Open a **Skill Issue** when guidance is inaccurate, a skill does not trigger, installation fails, or MCP-backed behavior differs between clients. Do not include bot tokens, API keys, MCP authorization headers, or private source.
