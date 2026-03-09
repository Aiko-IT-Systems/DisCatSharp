---
uid: modules_audio_voice_transmit
title: Transmitting
author: DisCatSharp Team
---

# Transmitting with Voice

## 1. Enable Voice

```csharp
using DisCatSharp.Voice;
using DisCatSharp.Voice.Entities;
using DisCatSharp.Voice.Enums;

client.UseVoice(new VoiceConfiguration
{
    EnableDebugLogging = false,
    MaxDaveProtocolVersion = 1,
    DavePendingAudioBehavior = DavePendingAudioBehavior.PassThrough
});
```

## 2. Connect to a Voice Channel

```csharp
using DisCatSharp.Voice;
using DisCatSharp.Voice.Entities;

VoiceConnection connection = await channel.ConnectAsync();
VoiceTransmitSink transmit = connection.GetTransmitSink();
```

## 3. Send PCM Audio

Discord voice expects PCM S16LE, 48kHz, stereo before Opus encoding.

Example using `ffmpeg` to convert an input file and stream PCM to the transmit sink:

```csharp
using System.Diagnostics;
using DisCatSharp.Voice;

public static async Task PlayFileAsync(VoiceConnection connection, string filePath, CancellationToken ct = default)
{
    var ffmpeg = Process.Start(new ProcessStartInfo
    {
        FileName = "ffmpeg",
        Arguments = $"-hide_banner -loglevel warning -i \"{filePath}\" -ac 2 -ar 48000 -f s16le pipe:1",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    }) ?? throw new InvalidOperationException("Failed to start ffmpeg.");

    await using var pcm = ffmpeg.StandardOutput.BaseStream;
    var transmit = connection.GetTransmitSink();

    await pcm.CopyToAsync(transmit, ct);
    await transmit.FlushAsync(ct);
    await connection.WaitForPlaybackFinishAsync();
}
```

## 4. DAVE Behavior During Handshake

When DAVE is negotiated but not active yet, outbound handling is controlled by `VoiceConfiguration.DavePendingAudioBehavior`:

- `PassThrough` (default): send without DAVE layer until active
- `Drop`: drop outbound frames until active
- `Throw`: throw `InvalidOperationException`

If you want to gate playback on DAVE becoming active:

```csharp
bool daveReady = await connection.WaitForDaveActiveAsync(TimeSpan.FromSeconds(5));
if (!daveReady)
{
    // Decide your fallback policy here.
}
```

## 5. Send Filters

```csharp
using DisCatSharp.Voice;
using DisCatSharp.Voice.Entities;
using DisCatSharp.Voice.Interfaces;

public sealed class SoftClipFilter : IVoiceFilter
{
    public void Transform(Span<short> pcmData, AudioFormat pcmFormat, int duration)
    {
        for (var i = 0; i < pcmData.Length; i++)
        {
            var sample = pcmData[i];
            // Light soft-clip to reduce peaks before encode.
            if (sample > 28000) sample = (short)(28000 + (sample - 28000) / 4);
            if (sample < -28000) sample = (short)(-28000 + (sample + 28000) / 4);
            pcmData[i] = sample;
        }
    }
}

var transmit = connection.GetTransmitSink();
var filter = new SoftClipFilter();

transmit.InstallFilter(filter);  // appended at end of filter chain
transmit.VolumeModifier = 0.9;   // optional global gain

// ... write PCM as usual ...

transmit.UninstallFilter(filter);
```

Use `InstallFilter(filter, order)` if you need deterministic ordering.

## 6. Playback Controls

```csharp
transmit.Pause();
await transmit.ResumeAsync();
await transmit.FlushAsync();
await connection.WaitForPlaybackFinishAsync();
```

## 7. Disconnect

```csharp
connection.Disconnect();
```

## CommandsNext Example

```csharp
[Command("join")]
public static async Task JoinAsync(CommandContext ctx)
{
    var channel = ctx.Member?.VoiceState?.Channel
        ?? throw new InvalidOperationException("Join a voice channel first.");

    await channel.ConnectAsync();
}

[Command("play")]
public static async Task PlayAsync(CommandContext ctx, string path)
{
    var voice = ctx.Client.GetVoice()
        ?? throw new InvalidOperationException("Voice is not enabled.");

    var connection = voice.GetConnection(ctx.Guild)
        ?? throw new InvalidOperationException("Bot is not connected.");

    await PlayFileAsync(connection, path, CancellationToken.None);
}

[Command("leave")]
public static Task LeaveAsync(CommandContext ctx)
{
    var connection = ctx.Client.GetVoice()?.GetConnection(ctx.Guild);
    connection?.Disconnect();
    return Task.CompletedTask;
}
```

## See Also

- [Voice Overview](xref:modules_audio_voice)
- [Voice Prerequisites](xref:modules_audio_voice_prerequisites)
- [Voice Events](xref:modules_audio_voice_events)
- [Receiving Audio](xref:modules_audio_voice_receive)
