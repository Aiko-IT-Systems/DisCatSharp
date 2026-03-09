---
uid: modules_audio_voice_migration
title: Migration from VoiceNext
---

# Migrating from VoiceNext to DisCatSharp.Voice

DisCatSharp.Voice replaces `DisCatSharp.VoiceNext` starting with **DisCatSharp 10.7.0**. VoiceNext has been fully removed.

This guide covers what changed and how to update your code.

---

## Package Changes

| Old | New |
|---|---|
| `DisCatSharp.VoiceNext` | `DisCatSharp.Voice` |
| `DisCatSharp.VoiceNext.Natives` | `DisCatSharp.Voice.Natives` |

Update your `.csproj`:

```xml
<!-- Remove -->
<PackageReference Include="DisCatSharp.VoiceNext" Version="..." />
<PackageReference Include="DisCatSharp.VoiceNext.Natives" Version="..." />

<!-- Add -->
<PackageReference Include="DisCatSharp.Voice" Version="10.7.0" />
<PackageReference Include="DisCatSharp.Voice.Natives" Version="10.7.0" />
```

---

## Namespace Changes

```csharp
// Old
using DisCatSharp.VoiceNext;

// New
using DisCatSharp.Voice;
```

### DisCatSharp.Voice namespace layout updates

Starting with the namespace cleanup in the `DisCatSharp.Voice` package, several public types moved out of the root namespace.

| Old namespace | New namespace | Public types |
|---|---|---|
| `DisCatSharp.Voice` | `DisCatSharp.Voice.Entities` | `AudioFormat`, `VoiceTransmitSink` |
| `DisCatSharp.Voice` | `DisCatSharp.Voice.Interfaces` | `IVoiceFilter` |
| `DisCatSharp.Voice` | `DisCatSharp.Voice.Enums` | `VoiceApplication`, `VoicePacketDropReason`, `DavePendingAudioBehavior`, `DaveConnectionState`, `DaveOpcodeDirection` |
| `DisCatSharp.Voice` | `DisCatSharp.Voice.Logging` | `VoiceEvents` |

Typical updated usings:

```csharp
using DisCatSharp.Voice;
using DisCatSharp.Voice.Entities;
using DisCatSharp.Voice.Interfaces;
using DisCatSharp.Voice.Enums;
using DisCatSharp.Voice.Logging;
```

---

## API Changes

All public types are renamed. The behavior is identical.

| Old | New |
|---|---|
| `VoiceNextExtension` | `VoiceExtension` |
| `VoiceNextConnection` | `VoiceConnection` |
| `VoiceNextConfiguration` | `VoiceConfiguration` |
| `VoiceNextEvents` | `VoiceEvents` (`DisCatSharp.Voice.Logging`) |

### Extension method registration

```csharp
// Old
discord.UseVoiceNext();
discord.UseVoiceNext(new VoiceNextConfiguration { EnableIncoming = true });

// New
discord.UseVoice();
discord.UseVoice(new VoiceConfiguration { EnableIncoming = true });
```

### Getting the extension

```csharp
// Old
var vnext = discord.GetVoiceNext();

// New
var voice = discord.GetVoice();
```

### Connecting to a channel

No change — `ConnectAsync()` is still on `DiscordChannel`:

```csharp
VoiceConnection connection = await channel.ConnectAsync();
```

### Transmitting audio

No change — `GetTransmitSink()` still returns a `VoiceTransmitSink`:

```csharp
var transmit = connection.GetTransmitSink();
await pcm.CopyToAsync(transmit);
```

### Receiving audio

No change — `VoiceReceived` event is still on the connection:

```csharp
connection.VoiceReceived += OnVoiceReceived;
```

---

## What's New in DisCatSharp.Voice

### DAVE End-to-End Encryption

The most significant addition is full support for Discord's **DAVE** E2EE voice encryption protocol. This is handled automatically — no API changes are needed in your bot code.

When a voice channel uses DAVE:
- Outgoing audio is encrypted by `libdave` before leaving the bot
- Incoming audio is decrypted by `libdave` before reaching your `VoiceReceived` handler
- Key rotation happens automatically on user join/leave

### libdave Native Dependency

`DisCatSharp.Voice.Natives` now ships `libdave` in addition to opus and libsodium. If libdave is missing, voice continues to work but DAVE encryption is disabled for that session.

### Improved Safety

- Ratchet transitions are protected against partial state races
- Decryptor map updates are atomic from the perspective of packet processing
- All native buffers are properly freed
- `ArrayPool<byte>` is used throughout to minimize per-frame allocations

---

## See Also

- [Voice Prerequisites](xref:modules_audio_voice_prerequisites)
- [Transmitting Audio](xref:modules_audio_voice_transmit)
- [Receiving Audio](xref:modules_audio_voice_receive)
- [Voice Architecture](xref:modules_audio_voice_architecture)
