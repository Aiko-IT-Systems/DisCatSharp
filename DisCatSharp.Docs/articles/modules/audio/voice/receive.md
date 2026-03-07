---
uid: modules_audio_voice_receive
title: Receiving
author: DisCatSharp Team
---

# Receiving with Voice

## 1. Enable Incoming Audio

Incoming receive is opt-in.

```csharp
using DisCatSharp.Voice;

client.UseVoice(new VoiceConfiguration
{
    EnableIncoming = true,
    EnableDebugLogging = false
});
```

## 2. Connect and Subscribe

```csharp
VoiceConnection connection = await channel.ConnectAsync();
connection.VoiceReceived += OnVoiceReceived;
connection.VoicePacketDropped += OnVoicePacketDropped;
```

## 3. Handle Decoded Frames

`VoiceReceived` gives you decoded PCM plus metadata.

```csharp
using DisCatSharp.Voice;
using DisCatSharp.Voice.EventArgs;

private static Task OnVoiceReceived(VoiceConnection _, VoiceReceiveEventArgs e)
{
    Console.WriteLine(
        $"RX user={e.User?.Id} ssrc={e.Ssrc} seq={e.Sequence} " +
        $"pcm={e.PcmData.Length} opus={e.OpusData.Length} " +
        $"missing={e.MissingFrames} conceal={e.IsConcealmentFrame}");

    // e.PcmData is raw PCM bytes for this frame.
    return Task.CompletedTask;
}
```

Useful fields:

- `PcmData`: decoded PCM for the frame
- `OpusData`: original Opus payload when available
- `AudioFormat`: format of decoded PCM
- `AudioDuration`: frame duration in ms
- `MissingFrames`: count of gap frames before this frame
- `IsConcealmentFrame`: true when frame is packet-loss concealment

## 4. Monitor Packet Drops

```csharp
private static Task OnVoicePacketDropped(VoiceConnection _, VoicePacketDroppedEventArgs e)
{
    Console.WriteLine($"DROP reason={e.Reason} user={e.User?.Id} ssrc={e.Ssrc} seq={e.Sequence} detail={e.Detail}");
    return Task.CompletedTask;
}
```

Drop reasons include malformed RTP, DAVE pending/missing ratchet, out-of-order, and decode failures.

## 5. Record to MP3 (ffmpeg)

If you want a listen-test artifact, you can stream decoded PCM directly into `ffmpeg`:

```csharp
using System.Diagnostics;
using DisCatSharp.Voice;
using DisCatSharp.Voice.EventArgs;

private static Process? _ffmpeg;
private static Stream? _ffmpegIn;

public static void StartMp3Recording(string outputPath)
{
    _ffmpeg = Process.Start(new ProcessStartInfo
    {
        FileName = "ffmpeg",
        Arguments = $"-y -hide_banner -loglevel warning -f s16le -ar 48000 -ac 2 -i pipe:0 -codec:a libmp3lame -q:a 2 \"{outputPath}\"",
        RedirectStandardInput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    }) ?? throw new InvalidOperationException("Failed to start ffmpeg.");

    _ffmpegIn = _ffmpeg.StandardInput.BaseStream;
}

private static async Task OnVoiceReceivedForRecording(VoiceConnection _, VoiceReceiveEventArgs e)
{
    if (_ffmpegIn is null || e.PcmData.Length == 0)
        return;

    // Preserve timeline continuity by filling packet-loss gaps with silence.
    if (e.MissingFrames > 0)
    {
        var bytesPerMs = e.AudioFormat.SampleRate * e.AudioFormat.ChannelCount * sizeof(short) / 1000;
        var silenceBytes = bytesPerMs * e.AudioDuration * e.MissingFrames;
        if (silenceBytes > 0)
            await _ffmpegIn.WriteAsync(new byte[silenceBytes]);
    }

    await _ffmpegIn.WriteAsync(e.PcmData);
}

public static async Task StopMp3RecordingAsync()
{
    if (_ffmpeg is null || _ffmpegIn is null)
        return;

    await _ffmpegIn.FlushAsync();
    _ffmpegIn.Dispose();
    _ffmpegIn = null;

    _ffmpeg.WaitForExit();
    _ffmpeg.Dispose();
    _ffmpeg = null;
}
```

Wire it like this:

```csharp
connection.VoiceReceived += OnVoiceReceivedForRecording;
StartMp3Recording("logs/recordings/session.mp3");
```

The same pattern works for WAV by changing ffmpeg output arguments.

## 6. DAVE Decryption Notes

When DAVE is active for the channel, decryption happens before your handler runs.

- You always receive plain PCM in `VoiceReceiveEventArgs.PcmData`.
- If DAVE is negotiated but not active yet, frames can be dropped depending on session readiness.
- `DaveStateChanged` and `DaveOpcodeObserved` help diagnose handshake timing.

## 7. Unsubscribe / Disconnect

```csharp
connection.VoiceReceived -= OnVoiceReceived;
connection.VoicePacketDropped -= OnVoicePacketDropped;
connection.Disconnect();
```

## See Also

- [Voice Overview](xref:modules_audio_voice)
- [Voice Events](xref:modules_audio_voice_events)
- [Transmitting Audio](xref:modules_audio_voice_transmit)
- [Voice Architecture](xref:modules_audio_voice_architecture)
