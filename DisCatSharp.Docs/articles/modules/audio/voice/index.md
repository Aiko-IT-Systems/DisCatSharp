---
uid: modules_audio_voice
title: Voice Overview
author: DisCatSharp Team
---

# DisCatSharp.Voice Overview

`DisCatSharp.Voice` is the built-in Discord voice client for DisCatSharp.
It supports:

- Voice gateway signaling (join, move, reconnect)
- RTP audio send and receive
- Opus encode/decode
- Discord transport encryption
- DAVE end-to-end encryption (when the channel enables it)

## Install

Install `DisCatSharp.Voice` from NuGet.

```xml
<PackageReference Include="DisCatSharp.Voice" Version="10.7.0" />
```

`DisCatSharp.Voice.Natives` is pulled automatically as a dependency for normal NuGet installs.

## Quick Start

```csharp
using DisCatSharp;
using DisCatSharp.Voice;

var client = new DiscordClient(new DiscordConfiguration
{
    Token = "...",
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildVoiceStates
});

client.UseVoice(new VoiceConfiguration
{
    EnableIncoming = true,
    EnableDebugLogging = false
});

// Later, once you have a DiscordChannel (voice/stage):
VoiceConnection connection = await channel.ConnectAsync();
VoiceTransmitSink sink = connection.GetTransmitSink();
```

## DAVE and Runtime Behavior

When Discord enables DAVE for a channel, DisCatSharp negotiates it automatically.

Important runtime flags on `VoiceConnection`:

- `IsDaveNegotiated`
- `IsDaveActive`
- `IsE2eeUsableForSend`
- `IsE2eeUsableForReceive`
- `DaveState`

You can also wait for DAVE activation explicitly:

```csharp
bool active = await connection.WaitForDaveActiveAsync(TimeSpan.FromSeconds(5));
```

## Next Steps

- [Voice Prerequisites](xref:modules_audio_voice_prerequisites)
- [Transmitting Audio](xref:modules_audio_voice_transmit)
- [Receiving Audio](xref:modules_audio_voice_receive)
- [Voice Events](xref:modules_audio_voice_events)
- [Voice Architecture](xref:modules_audio_voice_architecture)
- [Migration Guide](xref:modules_audio_voice_migration)
