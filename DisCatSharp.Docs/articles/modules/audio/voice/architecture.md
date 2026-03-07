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
- OP22 `execute_transition` (optional per transition)
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
- `CanSendAudio`
- `CanReceiveAudio`
- `WaitForDaveActiveAsync(...)`
- `DaveStateChanged` event
- `DaveOpcodeObserved` event

## See Also

- [Voice Overview](xref:modules_audio_voice)
- [Voice Events](xref:modules_audio_voice_events)
- [Voice Prerequisites](xref:modules_audio_voice_prerequisites)
- [Transmitting Audio](xref:modules_audio_voice_transmit)
- [Receiving Audio](xref:modules_audio_voice_receive)
