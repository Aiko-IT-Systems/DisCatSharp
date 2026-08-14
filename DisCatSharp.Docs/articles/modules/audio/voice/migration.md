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
<PackageReference Include="DisCatSharp.Voice" Version="10.7.1" />
<PackageReference Include="DisCatSharp.Voice.Natives" Version="10.7.1" />
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

The core connection, transmit, and receive workflow remains familiar, but behavior is not identical: `DisCatSharp.Voice` adds DAVE negotiation, current Discord transport modes, packet diagnostics, external Opus input, and explicit media-readiness surfaces.

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

The DAVE control and media planes are intentionally separate:

- OP21, OP29, and OP30 prepare receive transforms first.
- Nonzero transition IDs produce OP23 readiness; matching OP22 later switches only the local sender.
- Transition ID `0` executes immediately without OP23.
- `ReadyForTransition` and `Downgrading` can retain an active old sender epoch.
- Protocol `0` uses media-ready passthrough even though `IsDaveActive` is `false`.

Consumer code normally needs no special handshake implementation. If old code gated audio with an enum comparison, migrate it to the media properties:

```csharp
// Media policy: can audio flow right now?
if (!connection.IsE2eeUsableForSend)
    return;

// Encryption policy: is the executing sender actually DAVE-encrypted?
if (!connection.IsDaveActive)
    Console.WriteLine("Current media mode is not DAVE-encrypted.");
```

`WaitForDaveActiveAsync` waits for DAVE encryption, not generic media readiness. It can return `false` for usable protocol-0 passthrough.

### libdave Native Dependency

`DisCatSharp.Voice.Natives` now ships `libdave` in addition to Opus and libsodium. Keep the managed and native package versions aligned. If libdave is missing, the transport may initialize but the connection cannot participate correctly in a DAVE-required media session.

### Improved Safety

- Ratchet transitions are protected against partial state races
- Decryptor map updates are atomic from the perspective of packet processing
- Existing per-user decryptors survive epoch changes so old encrypted ratchets and plaintext overlap remain available for ten seconds
- Invalid commits and Welcomes preserve their transition ID for OP31 recovery while retaining currently executing media transforms
- All native buffers are properly freed
- `ArrayPool<byte>` is used throughout to minimize per-frame allocations

---

## See Also

- [Voice Prerequisites](xref:modules_audio_voice_prerequisites)
- [Transmitting Audio](xref:modules_audio_voice_transmit)
- [Receiving Audio](xref:modules_audio_voice_receive)
- [Voice Architecture](xref:modules_audio_voice_architecture)
