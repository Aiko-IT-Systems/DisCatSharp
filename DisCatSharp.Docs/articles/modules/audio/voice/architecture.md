---
uid: modules_audio_voice_architecture
title: Voice Architecture
author: DisCatSharp Team
---

# Voice Architecture

`DisCatSharp.Voice` combines gateway signaling, UDP media transport, Opus, Discord transport encryption, and optional DAVE E2EE.

## Connection Lifecycle

```mermaid
sequenceDiagram
    participant Bot
    participant Gateway
    participant VoiceGateway
    participant UDP

    Bot->>Gateway: VOICE_STATE_UPDATE (join)
    Gateway-->>Bot: VOICE_STATE_UPDATE (session_id)
    Gateway-->>Bot: VOICE_SERVER_UPDATE (endpoint, token)
    Bot->>VoiceGateway: OP0 Identify
    VoiceGateway-->>Bot: OP2 Ready (ssrc, ip, port, modes)
    Bot->>UDP: IP Discovery
    UDP-->>Bot: External IP:Port
    Bot->>VoiceGateway: OP1 SelectProtocol
    VoiceGateway-->>Bot: OP4 SessionDescription
    Bot->>UDP: RTP packets
```

## Media Pipeline

### Send path

1. PCM input
2. Opus encode
3. RTP header encode
4. DAVE frame encryption (if active)
5. Discord transport encryption (AEAD/XSalsa mode)
6. UDP send

### Receive path

1. UDP receive
2. RTP header parse
3. Discord transport decryption
4. DAVE frame decryption (if active)
5. RTP extension strip (RFC 5285)
6. Opus decode
7. `VoiceReceived` event dispatch

## DAVE Handshake Overview

Typical gateway flow:

- OP21 `prepare_transition`
- OP22 `execute_transition` (used to execute staged non-zero transitions)
- OP24 `prepare_epoch`
- OP25 binary `external_sender`
- OP26 binary key package (client send)
- OP27 binary proposals
- OP28 binary commit (client send)
- OP29 binary announce_commit
- OP30 binary welcome

`VoiceConnection` emits:

- `[DAVE FLOW] OPxx received/sent`
- `[DAVE FSM] {OldState} -> {NewState} via {Handler}`

## DAVE Join Workflow

When the bot joins an encrypted channel, DAVE activation is a multi-step MLS flow.

```mermaid
sequenceDiagram
    participant VC as VoiceConnection
    participant VG as Voice Gateway
    participant MLS as DaveSession/libdave

    VG-->>VC: OP21 prepare_transition
    VC->>MLS: Transition to ReadyForTransition
    VG-->>VC: OP24 prepare_epoch
    VC->>MLS: Transition to Pending
    VG-->>VC: OP25 external_sender (binary)
    VC->>MLS: Ensure group initialized + SetExternalSender
    VC->>VG: OP26 key_package (binary, when prepared)
    VG-->>VC: OP27 proposals (binary)
    VC->>MLS: ProcessProposals -> CreateCommit
    VC->>VG: OP28 commit (binary)
    VG-->>VC: OP29 announce_commit (binary)
    alt transition_id == 0
        VG-->>VC: OP30 welcome (binary)
        VC->>MLS: InstallRatchets()
        MLS-->>VC: State Active
    else transition_id != 0
        VC->>MLS: Stage commit (pre-active)
        VC->>VG: OP23 ready_for_transition
        VG-->>VC: OP22 execute_transition
        VC->>MLS: Transition to Pending (next epoch)
    end
```

### Join state progression

```mermaid
stateDiagram-v2
    [*] --> Inactive
    Inactive --> Pending: SessionDescription + DAVE negotiated
    Pending --> ReadyForTransition: OP21
    ReadyForTransition --> Pending: OP24
    Pending --> AwaitingResponse: OP25 handled + OP26 sent
    AwaitingResponse --> Active: OP29/OP30 + InstallRatchets (transition_id=0)
    AwaitingResponse --> ReadyForTransition: OP29 staged (transition_id!=0)
    ReadyForTransition --> Pending: OP22 execute_transition
    Pending --> Active: Subsequent OP29/OP30 + InstallRatchets
```

If no proposals arrive (for example, bot alone in channel), DAVE can remain `Pending`/`AwaitingResponse` until another participant triggers group activity.

## DAVE Move / Reconnect Workflow

Moving the bot (or forced region move) triggers voice gateway rebind and DAVE re-negotiation.

```mermaid
sequenceDiagram
    participant DiscordGW as Discord Gateway
    participant VC as VoiceConnection
    participant VG as Voice Gateway
    participant MLS as DaveSession

    DiscordGW-->>VC: VOICE_STATE_UPDATE (new channel/region)
    DiscordGW-->>VC: VOICE_SERVER_UPDATE (new endpoint/token)
    VC->>VC: Mark fresh session (no resume)
    VC->>VG: Reconnect + OP0 Identify
    VG-->>VC: OP2 Ready / OP4 SessionDescription
    VC->>MLS: Reset old session -> Pending
    VG-->>VC: OP21..OP30 (new epoch flow)
    VC->>MLS: InstallRatchets()
    MLS-->>VC: Active
```

Key rule: when endpoint/channel context changes, perform a fresh identify path for the new server context and complete a new DAVE handshake.

## Playback Workflow vs DAVE State

Outbound audio behavior is controlled by state and `DavePendingAudioBehavior`.

```mermaid
flowchart TD
    A[PCM frame ready] --> B{Is DAVE negotiated?}
    B -->|No| C[Send with transport encryption]
    B -->|Yes| D{Is DAVE active?}
    D -->|Yes| E[DAVE encrypt frame]
    E --> F[Transport encrypt + RTP send]
    D -->|No| G{Pending behavior}
    G -->|PassThrough| C
    G -->|Drop| H[Drop frame]
    G -->|Throw| I[Throw InvalidOperationException]
```

## Public DAVE States

`VoiceConnection.DaveState` maps to:

- `NotNegotiated`
- `Inactive`
- `Pending`
- `AwaitingResponse`
- `ReadyForTransition`
- `Active`
- `Downgrading`

## Runtime Signals

Use these for application-level gating and diagnostics:

- `IsDaveNegotiated`
- `IsDaveActive`
- `IsE2eeUsableForSend`
- `IsE2eeUsableForReceive`
- `WaitForDaveActiveAsync(...)`
- `EnableDebugLogging` (per connection toggle)
- `DaveStateChanged` event
- `DaveOpcodeObserved` event

## See Also

- [Voice Overview](xref:modules_audio_voice)
- [Voice Events](xref:modules_audio_voice_events)
- [Voice Prerequisites](xref:modules_audio_voice_prerequisites)
- [Transmitting Audio](xref:modules_audio_voice_transmit)
- [Receiving Audio](xref:modules_audio_voice_receive)
