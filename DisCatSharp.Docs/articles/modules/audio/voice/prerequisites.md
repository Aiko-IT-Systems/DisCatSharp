---
uid: modules_audio_voice_prerequisites
title: Voice Prerequisites
author: DisCatSharp Team
---

# Voice Prerequisites

This page covers what you need before using `DisCatSharp.Voice`.

## 1. Install the Voice Package

```xml
<PackageReference Include="DisCatSharp.Voice" Version="10.7.0" />
```

For normal NuGet usage, `DisCatSharp.Voice.Natives` is included automatically.

If you run from source or manually manage runtime assets, add:

```xml
<PackageReference Include="DisCatSharp.Voice.Natives" Version="10.7.0">
  <IncludeAssets>runtime; native; contentfiles</IncludeAssets>
</PackageReference>
```

## 2. Enable Required Gateway Intent

Voice connect and movement events require `GuildVoiceStates`.

```csharp
var client = new DiscordClient(new DiscordConfiguration
{
    Token = "...",
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildVoiceStates
});
```

## 3. Native Dependencies by Platform

`DisCatSharp.Voice` relies on native libraries (`libopus`, `libsodium`, and for DAVE, `libdave`).

### Windows

Using NuGet packages is usually enough. Runtime natives are shipped in `DisCatSharp.Voice.Natives`.

### macOS

If you run outside packaged natives, install dependencies with Homebrew:

```bash
brew install opus libsodium
```

### Linux (Debian/Ubuntu)

If you run outside packaged natives:

```bash
sudo apt-get install libopus0 libopus-dev
sudo apt-get install libsodium23 libsodium-dev
```

For older distros, `libsodium` package names can differ (`libsodium18`, `libsodium13`, etc.).

## 4. DAVE Support

DAVE is Discord's end-to-end voice encryption protocol. DisCatSharp negotiates and uses it automatically when the channel requires it.

Prebuilt `libdave` binaries in `DisCatSharp.Voice.Natives` are available for:

| Platform | Architectures |
| --- | --- |
| Windows | x64 |
| Linux | x64, arm64 |
| macOS | x64, arm64 |

If `libdave` is unavailable at runtime, voice transport still works, but DAVE features for that session will not activate.

## 5. Voice Configuration Defaults

```csharp
client.UseVoice(new VoiceConfiguration
{
    EnableIncoming = false,
    EnableDebugLogging = false,
    MaxDaveProtocolVersion = 1,
    DavePendingAudioBehavior = DavePendingAudioBehavior.PassThrough
});
```

## See Also

- [Voice Overview](xref:modules_audio_voice)
- [Transmitting Audio](xref:modules_audio_voice_transmit)
- [Receiving Audio](xref:modules_audio_voice_receive)
- [Voice Events](xref:modules_audio_voice_events)
- [Voice Architecture](xref:modules_audio_voice_architecture)
