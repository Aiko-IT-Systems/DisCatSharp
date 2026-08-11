# DisCatSharp repository map

Use repository search before assuming a path is current.

| Area | Typical location |
| --- | --- |
| Core clients, entities, REST, Gateway | `DisCatSharp/` |
| Slash/context commands | `DisCatSharp.ApplicationCommands/` |
| Prefix commands | `DisCatSharp.CommandsNext/` |
| Shared primitives | `DisCatSharp.Common/` |
| Configuration package | `DisCatSharp.Configuration/` |
| Experimental APIs | `DisCatSharp.Experimental/` |
| Hosting and DI | `DisCatSharp.Hosting*/` |
| Interaction helpers | `DisCatSharp.Interactivity/` |
| Lavalink | `DisCatSharp.Lavalink/` |
| Voice and native packaging | `DisCatSharp.Voice*/` |
| Shared targets and versions | `DisCatSharp.Targets/` |
| Analyzers and repository tools | `DisCatSharp.Tools/` |
| Tests | `DisCatSharp.Tests/` and tool-specific test projects |
| DocFX source and template | `DisCatSharp.Docs/` |
| Workflows and repository automation | `.github/` |

Important cross-cutting paths include REST payload serialization, Gateway dispatch/event args, localized application-command reconstruction, application-command equality/synchronization, hosted configuration copies, and XML/DocFX documentation.
