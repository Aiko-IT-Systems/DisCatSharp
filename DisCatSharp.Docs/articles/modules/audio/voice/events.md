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
| `DaveStateChanged` | `DaveStateChangedEventArgs` | DAVE control-plane transitions with executing sender status |
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
    Console.WriteLine(
        $"[DAVE FSM] {e.OldState} -> {e.NewState} via {e.Handler}; " +
        $"protocol={e.ProtocolVersion}; senderEncrypted={e.IsActive}; reason={e.Reason}");
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

`DaveStateChangedEventArgs.NewState` describes the control plane. `DaveStateChangedEventArgs.IsActive` describes whether the currently executing sender transform is encrypting. They intentionally differ during transitions:

- An established sender can report `ReadyForTransition` or `Downgrading` with `IsActive == true` while it keeps using the old epoch.
- After OP22 executes a protocol-0 downgrade, the event reports `Inactive` with `IsActive == false`, but protocol-0 media remains usable.
- `ProtocolVersion` is the currently executing sender version. A merely prepared next version does not replace it.

Use the connection readiness properties when deciding whether media can flow:

```csharp
if (!connection.IsE2eeUsableForSend)
    Console.WriteLine("Outbound media is currently gated by the configured pending-audio policy.");

if (!connection.IsE2eeUsableForReceive)
    Console.WriteLine("Inbound media cannot currently be processed.");
```

Use an explicit wait when the application requires an encrypting DAVE sender:

```csharp
bool daveReady = await connection.WaitForDaveActiveAsync(TimeSpan.FromSeconds(5));
if (!daveReady)
{
    // This can still be usable protocol-0 passthrough. Inspect the readiness
    // properties and apply the application's encryption policy.
}
```

`DaveOpcodeObserved` reports both server-to-client and client-to-server traffic. In the corrected transition sequence, OP23 is observed after successful receiver preparation and before OP22; OP22 has no acknowledgement. Invalid OP29 or OP30 processing produces client-to-server OP31 with the same transition ID, followed by a fresh OP26.

Related configuration:

- `VoiceConfiguration.MaxDaveProtocolVersion`
- `VoiceConfiguration.DavePendingAudioBehavior`
- `VoiceConfiguration.EnableDebugLogging`
- `VoiceConnection.EnableDebugLogging`

## Runtime Logging Control

If you only want deep diagnostics for one problematic call, toggle logging on that connection instead of globally:

```csharp
connection.EnableDebugLogging = true;
// reproduce issue, inspect logs
connection.EnableDebugLogging = false;
```
