---
uid: modules_audio_voice_events
title: Voice Events
author: DisCatSharp Team
---

# Voice Events

`VoiceConnection` exposes async events for user activity, audio frames, packet diagnostics, and DAVE state.

## Event List

| Event | Args | Purpose |
| --- | --- | --- |
| `UserSpeaking` | `UserSpeakingEventArgs` | Discord speaking flag updates (OP5) |
| `UserJoined` | `VoiceUserJoinEventArgs` | User SSRC/user binding joined |
| `UserLeft` | `VoiceUserLeaveEventArgs` | User SSRC/user binding left |
| `VoiceReceived` | `VoiceReceiveEventArgs` | Decoded PCM (and source Opus when available) |
| `VoicePacketDropped` | `VoicePacketDroppedEventArgs` | Inbound packet drop classification |
| `DaveStateChanged` | `DaveStateChangedEventArgs` | Public DAVE FSM state transitions |
| `DaveOpcodeObserved` | `DaveOpcodeEventArgs` | DAVE opcode send/receive diagnostics |
| `VoiceSocketErrored` | `SocketErrorEventArgs` | Voice WebSocket exception path |

## Subscribe and Unsubscribe

```csharp
using DisCatSharp.EventArgs;
using DisCatSharp.Voice;
using DisCatSharp.Voice.EventArgs;

public static void WireVoiceEvents(VoiceConnection connection)
{
    connection.UserSpeaking += OnUserSpeaking;
    connection.UserJoined += OnUserJoined;
    connection.UserLeft += OnUserLeft;
    connection.VoiceReceived += OnVoiceReceived;
    connection.VoicePacketDropped += OnVoicePacketDropped;
    connection.DaveStateChanged += OnDaveStateChanged;
    connection.DaveOpcodeObserved += OnDaveOpcodeObserved;
    connection.VoiceSocketErrored += OnVoiceSocketErrored;
}

public static void UnwireVoiceEvents(VoiceConnection connection)
{
    connection.UserSpeaking -= OnUserSpeaking;
    connection.UserJoined -= OnUserJoined;
    connection.UserLeft -= OnUserLeft;
    connection.VoiceReceived -= OnVoiceReceived;
    connection.VoicePacketDropped -= OnVoicePacketDropped;
    connection.DaveStateChanged -= OnDaveStateChanged;
    connection.DaveOpcodeObserved -= OnDaveOpcodeObserved;
    connection.VoiceSocketErrored -= OnVoiceSocketErrored;
}

private static Task OnUserSpeaking(VoiceConnection _, UserSpeakingEventArgs e)
{
    Console.WriteLine($"[SPEAK] user={e.User?.Id} ssrc={e.Ssrc} flags={e.Speaking}");
    return Task.CompletedTask;
}

private static Task OnUserJoined(VoiceConnection _, VoiceUserJoinEventArgs e)
{
    Console.WriteLine($"[JOIN] user={e.User.Id} ssrc={e.Ssrc}");
    return Task.CompletedTask;
}

private static Task OnUserLeft(VoiceConnection _, VoiceUserLeaveEventArgs e)
{
    Console.WriteLine($"[LEAVE] user={e.User.Id} ssrc={e.Ssrc}");
    return Task.CompletedTask;
}

private static Task OnVoiceReceived(VoiceConnection _, VoiceReceiveEventArgs e)
{
    Console.WriteLine($"[RX] user={e.User?.Id} seq={e.Sequence} pcm={e.PcmData.Length} missing={e.MissingFrames} conceal={e.IsConcealmentFrame}");
    return Task.CompletedTask;
}

private static Task OnVoicePacketDropped(VoiceConnection _, VoicePacketDroppedEventArgs e)
{
    Console.WriteLine($"[DROP] reason={e.Reason} user={e.User?.Id} ssrc={e.Ssrc} seq={e.Sequence} detail={e.Detail}");
    return Task.CompletedTask;
}

private static Task OnDaveStateChanged(VoiceConnection _, DaveStateChangedEventArgs e)
{
    Console.WriteLine($"[DAVE FSM] {e.OldState} -> {e.NewState} via {e.Handler} ({e.Reason})");
    return Task.CompletedTask;
}

private static Task OnDaveOpcodeObserved(VoiceConnection _, DaveOpcodeEventArgs e)
{
    Console.WriteLine($"[DAVE FLOW] {e.Direction} OP{e.Opcode} len={e.PayloadLength} seq={e.Sequence} binary={e.IsBinary}");
    return Task.CompletedTask;
}

private static Task OnVoiceSocketErrored(VoiceConnection _, SocketErrorEventArgs e)
{
    Console.WriteLine($"[VOICE WS ERROR] {e.Exception.Message}");
    return Task.CompletedTask;
}
```

## DAVE Readiness

For DAVE-gated use cases, combine event monitoring with an explicit wait:

```csharp
bool daveReady = await connection.WaitForDaveActiveAsync(TimeSpan.FromSeconds(5));
if (!daveReady)
{
    // Decide whether to skip playback, pass through, or fail based on your policy.
}
```

Related configuration:

- `VoiceConfiguration.MaxDaveProtocolVersion`
- `VoiceConfiguration.DavePendingAudioBehavior`
- `VoiceConfiguration.EnableDebugLogging`
