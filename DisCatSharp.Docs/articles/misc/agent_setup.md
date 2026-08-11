---
uid: misc_agent_setup
title: AI Coding Assistants
description: Install the DisCatSharp Agent Skill and connect the live documentation MCP for the core library and official Extensions.
---

# AI coding assistants

DisCatSharp provides a version-aware Agent Skill for building and troubleshooting bots with the core library and its official Extensions. The skill gives compatible coding agents focused guidance while the public documentation MCP supplies live API, article, and approved source evidence.

## Install the skill

Install `use-discatsharp` globally for Codex:

```shell
gh skill install Aiko-IT-Systems/DisCatSharp use-discatsharp --agent codex --scope user --pin main
```

Omit `--pin main` to follow the latest stable DisCatSharp release after the skills ship in a stable version. You can also pin the same tag as the DisCatSharp version used by your application.

For Copilot, Claude Code, Gemini CLI, generic Agent Skills clients, and alternative installation methods, see the [complete skill installation reference](https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/skills/README.md#install).

## Connect the documentation MCP

The public MCP searches the current generated documentation for DisCatSharp and official DisCatSharp.Extensions:

```text
https://docs.dcs.aitsys.dev/mcp
```

It is public, stateless, and does not require authentication. Agents can search both corpora together, resolve exact overloads, fetch complete documentation records, and request only indexed source ranges. Clients may restrict a lookup to the `main` or `extensions` corpus when the same API name exists in both repositories.

See the [MCP client examples](https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/skills/README.md#connect-the-documentation-mcp) for Codex, Copilot, Claude Code, Gemini CLI, and generic clients.

> [!TIP]
> For Discord payloads, permissions, Gateway behavior, REST semantics, and rate limits, also connect the official Discord Documentation MCP at `https://docs.discord.com/mcp`.
