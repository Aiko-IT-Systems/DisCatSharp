# DisCatSharp module map

Use the consuming project and current documentation to confirm package names and versions. This map is for routing, not a substitute for API lookup.

| Area | Primary package | Use it for |
| --- | --- | --- |
| Core | `DisCatSharp` | Clients, entities, events, Gateway state, REST operations, webhooks, and OAuth |
| Slash and context commands | `DisCatSharp.ApplicationCommands` | Application-command registration, handlers, checks, autocomplete, and interaction responses |
| Prefix commands | `DisCatSharp.CommandsNext` | Text/prefix command parsing, converters, checks, and execution |
| Interaction helpers | `DisCatSharp.Interactivity` | Waiters, pagination, polls, collectors, and component flows |
| Configuration models | `DisCatSharp.Configuration` | Configuration abstractions shared with hosting integrations |
| Generic Host | `DisCatSharp.Hosting` | Hosted services and host-builder integration |
| Dependency injection | `DisCatSharp.Hosting.DependencyInjection` | Registering clients and extensions with Microsoft DI |
| ASP.NET Core | `DisCatSharp.Hosting.AspNetCore` | ASP.NET Core-specific hosting integration |
| Lavalink | `DisCatSharp.Lavalink` | External Lavalink node connections and audio players |
| Native voice | `DisCatSharp.Voice` | Discord voice connections and audio transport |
| Voice natives | `DisCatSharp.Voice.Natives` | Native voice dependencies distributed with Voice |
| Experimental | `DisCatSharp.Experimental` | Explicitly experimental Discord or library surfaces |
| Analyzers | `DisCatSharp.Analyzer` | Compile-time migration and usage diagnostics |

## Official Extensions

These packages live in `Aiko-IT-Systems/DisCatSharp.Extensions` and use the `extensions` documentation corpus.

| Area | Primary package | Use it for |
| --- | --- | --- |
| Two-factor commands | `DisCatSharp.Extensions.TwoFactorCommands` | TOTP enrollment, verification, and command-flow helpers |
| Simple music commands | `DisCatSharp.Extensions.SimpleMusicCommands` | Prebuilt Lavalink-backed music commands and controls |
| Legacy OAuth web server | `DisCatSharp.Extensions.OAuth2Web` | Maintaining existing embedded OAuth2 web-server integrations; deprecated in favor of `DisCatSharp.Hosting.AspNetCore` |

Do not add every package by default. Choose the smallest module set required by the feature and preserve the application's existing hosting model.
