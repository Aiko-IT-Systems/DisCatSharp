# Evidence and fallbacks

## Preferred evidence order

1. The user's checked-out source, project files, restored assets, and installed package version.
2. DisCatSharp MCP results from `https://docs.dcs.aitsys.dev/mcp`.
3. Public HTTP search at `https://docs.dcs.aitsys.dev/_search?q=<query>` and the linked documentation pages. Add `corpus=main` or `corpus=extensions` to select one repository, or omit `corpus` to search both. The optional `module` parameter accepts exact package names from [modules.md](modules.md).
4. Repository source at `https://github.com/Aiko-IT-Systems/DisCatSharp`.

`discatsharp-ai.xml` is a generated Repomix snapshot for offline context and evaluation. It may be stale. Never prefer it over live files, an installed package, or the current MCP corpus.

## No MCP connection

- Continue with local evidence when the project is available.
- Otherwise use public search and documentation if browsing is available.
- If neither is available, explain how to connect the MCP and avoid producing unverified signatures.
- Label any answer based only on general library knowledge as potentially version-sensitive.

## Version conflict

If current documentation and the installed package disagree:

1. Answer for the installed version when local evidence is sufficient.
2. Identify the current documented behavior separately.
3. Recommend an upgrade only when it is actually needed for the requested feature.
4. Do not silently rewrite the user's code to a newer major or prerelease API.

## No result

- Try a short type/member name, a fully qualified name, and a natural-language description.
- Remove trailing `()` only as a query variation; preserve the selected member signature in the answer.
- Search the owning type, module, or conceptual topic.
- Use official Discord documentation for the underlying protocol question.
- Conclude that the API is unverified if no DisCatSharp evidence exists; never borrow another library's equivalent.
