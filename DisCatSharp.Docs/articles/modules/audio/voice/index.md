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
<PackageReference Include="DisCatSharp.Voice" Version="10.7.1" />
```

`DisCatSharp.Voice.Natives` is pulled automatically as a dependency for normal NuGet installs.

## Quick Start

```csharp
using DisCatSharp;
using DisCatSharp.Voice;
using DisCatSharp.Voice.Entities;

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

// Optional: enable extra diagnostics only for this specific connection.
connection.EnableDebugLogging = true;
```

## Complete Example

This example configures voice, records receive diagnostics, waits only when outbound media is genuinely unavailable, streams a file through `ffmpeg`, and cleans up the connection.

```csharp
using System.Diagnostics;
using DisCatSharp;
using DisCatSharp.Enums;
using DisCatSharp.Voice;
using DisCatSharp.Voice.Enums;

const string token = "YOUR_BOT_TOKEN";
const ulong voiceChannelId = 123456789012345678;
const string audioPath = "audio/example.mp3";

using var shutdown = new CancellationTokenSource();

await using var client = new DiscordClient(new DiscordConfiguration
{
    Token = token,
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.Guilds | DiscordIntents.GuildVoiceStates
});

client.UseVoice(new VoiceConfiguration
{
    EnableIncoming = true,
    MaxDaveProtocolVersion = 1,
    DavePendingAudioBehavior = DavePendingAudioBehavior.Throw
});

await client.ConnectAsync(cancellationToken: shutdown.Token);

var channel = await client.GetChannelAsync(voiceChannelId, cancellationToken: shutdown.Token);
var connection = await channel.ConnectAsync();

connection.DaveStateChanged += (_, e) =>
{
    Console.WriteLine(
        $"[DAVE] {e.OldState} -> {e.NewState}; " +
        $"protocol={e.ProtocolVersion}; senderEncrypted={e.IsActive}; reason={e.Reason}");
    return Task.CompletedTask;
};

connection.VoicePacketDropped += (_, e) =>
{
    Console.WriteLine($"[DROP] reason={e.Reason}; user={e.User?.Id}; detail={e.Detail}");
    return Task.CompletedTask;
};

connection.VoiceReceived += (_, e) =>
{
    Console.WriteLine($"[RX] user={e.User?.Id}; pcmBytes={e.PcmData.Length}");
    return Task.CompletedTask;
};

try
{
    // Protocol 0 is usable passthrough. A positive DAVE negotiation with no
    // executing sender ratchet is not usable under the Throw policy above.
    if (!connection.IsE2eeUsableForSend)
    {
        _ = await connection.WaitForDaveActiveAsync(
            TimeSpan.FromSeconds(10),
            shutdown.Token);

        if (!connection.IsE2eeUsableForSend)
            throw new TimeoutException("Voice media did not become ready before playback.");
    }

    await PlayFileAsync(connection, audioPath, shutdown.Token);
}
finally
{
    connection.Disconnect();
    await client.DisconnectAsync();
}

static async Task PlayFileAsync(
    VoiceConnection connection,
    string path,
    CancellationToken cancellationToken)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "ffmpeg",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    startInfo.ArgumentList.Add("-hide_banner");
    startInfo.ArgumentList.Add("-loglevel");
    startInfo.ArgumentList.Add("warning");
    startInfo.ArgumentList.Add("-i");
    startInfo.ArgumentList.Add(path);
    startInfo.ArgumentList.Add("-ac");
    startInfo.ArgumentList.Add("2");
    startInfo.ArgumentList.Add("-ar");
    startInfo.ArgumentList.Add("48000");
    startInfo.ArgumentList.Add("-f");
    startInfo.ArgumentList.Add("s16le");
    startInfo.ArgumentList.Add("pipe:1");

    using var ffmpeg = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start ffmpeg.");
    await using var pcm = ffmpeg.StandardOutput.BaseStream;

    var sink = connection.GetTransmitSink();
    var buffer = new byte[81920];
    int bytesRead;
    while ((bytesRead = await pcm.ReadAsync(buffer, cancellationToken)) > 0)
        await sink.WriteAsync(buffer, 0, bytesRead, cancellationToken);

    await sink.FlushAsync(cancellationToken);
    await connection.WaitForPlaybackFinishAsync();
    await ffmpeg.WaitForExitAsync(cancellationToken);

    if (ffmpeg.ExitCode != 0)
        throw new InvalidOperationException($"ffmpeg exited with code {ffmpeg.ExitCode}.");
}
```

`IsE2eeUsableForSend` answers whether audio can flow under the configured policy. If your application requires DAVE encryption specifically, require `WaitForDaveActiveAsync(...)` to return `true` instead of accepting protocol-0 passthrough.

## DAVE and Runtime Behavior

When Discord enables DAVE for a channel, DisCatSharp negotiates it automatically.

Important runtime flags on `VoiceConnection`:

- `IsDaveNegotiated`: a DAVE coordinator exists, including protocol-0 passthrough
- `IsDaveActive`: the currently executing sender transform is encrypting
- `IsE2eeUsableForSend`: outbound media can flow under the selected pending-audio policy
- `IsE2eeUsableForReceive`: inbound media can be processed in the current receive mode
- `DaveProtocolVersion`: currently executing sender protocol version
- `DaveState`: control-plane negotiation state

You can also wait for DAVE activation explicitly:

```csharp
bool active = await connection.WaitForDaveActiveAsync(TimeSpan.FromSeconds(5));
```

`WaitForDaveActiveAsync` waits for DAVE encryption. It can return `false` while protocol-0 passthrough is nevertheless media-ready.

## Debug Logging Scope

`VoiceConfiguration.EnableDebugLogging` sets the default debug/trace behavior for voice connections.

You can override this at runtime per connection using `VoiceConnection.EnableDebugLogging`.

```csharp
connection.EnableDebugLogging = true;  // enable only this connection
connection.EnableDebugLogging = false; // disable again
```

## Next Steps

- [Voice Prerequisites](xref:modules_audio_voice_prerequisites)
- [Transmitting Audio](xref:modules_audio_voice_transmit)
- [Receiving Audio](xref:modules_audio_voice_receive)
- [Audio Output](xref:modules_audio_voice_output)
- [Voice Events](xref:modules_audio_voice_events)
- [Voice Architecture](xref:modules_audio_voice_architecture)
- [Migration Guide](xref:modules_audio_voice_migration)
