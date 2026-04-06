---
uid: modules_audio_lavalink_v4_bridge
title: Lavalink V4 Voice Bridge
author: DisCatSharp Team
---

# Lavalink Voice Bridge Mode

## What is Bridge Mode?

By default, Lavalink handles **both** audio decoding **and** Discord voice transport (via its built-in [Koe](https://github.com/KyokoSpl/koe) library). This works well for most use cases, but means:

- **No bot-side voice receive** while Lavalink is playing — Lavalink owns the voice connection exclusively
- **DisCatSharp.Voice's transport stack goes unused** — its RTP, DAVE E2EE, AEAD encryption, and UDP handling are bypassed entirely
- **Duplicate voice stacks** — if your bot needs voice features beyond playback (e.g. receive, mixing), you'd need two separate connections

**Bridge mode** changes this by splitting responsibilities:

| Component | Standard Mode | Bridge Mode |
|-----------|--------------|-------------|
| **Lavalink** | Decodes audio + sends to Discord | Decodes audio only, exports Opus frames |
| **DisCatSharp.Voice** | Not used | Handles Discord voice transport (RTP → DAVE → AEAD → UDP) |
| **DisCatSharp.Lavalink** | Manages Lavalink session | Coordinates both Lavalink + Voice |

The result: Lavalink handles what it does best (audio decoding, track resolution, filters), and DisCatSharp.Voice handles what **it** does best (Discord voice protocol, encryption, DAVE E2EE) — with both stacks unified under a single voice connection.

## Requirements

- **DisCatSharp** packages with bridge support (development branch)
- **Custom Lavalink fork** from [Aiko-IT-Systems/Lavalink](https://github.com/Aiko-IT-Systems/Lavalink) on the `feat/transport-bridge` branch
- Docker (recommended) or Java 21+ for running the custom Lavalink

## Lavalink Server Setup

### Option 1: Docker (Recommended)

Pull the pre-built bridge image from GitHub Container Registry:

```bash
docker pull ghcr.io/aiko-it-systems/lavalink:bridge
```

Or use it in a `docker-compose.yml`:

```yaml
services:
  lavalink:
    image: ghcr.io/aiko-it-systems/lavalink:bridge
    container_name: lavalink-bridge
    restart: unless-stopped
    environment:
      - _JAVA_OPTIONS=-Xmx2G
    ports:
      - "127.0.0.1:2333:2333"
    volumes:
      - ./application.yml:/opt/Lavalink/application.yml:ro
```

### Option 2: Build from Source

```bash
git clone https://github.com/Aiko-IT-Systems/Lavalink.git
cd Lavalink
git checkout feat/transport-bridge
./gradlew :Lavalink-Server:bootJar
java -jar LavalinkServer/build/libs/Lavalink.jar
```

### Lavalink application.yml

Add the `transport-mode` and `bridge` settings to your `application.yml` under `lavalink.server`:

```yaml
lavalink:
  server:
    password: "your-lavalink-password"
    # Enable bridge mode — Lavalink produces Opus frames,
    # DisCatSharp.Voice handles Discord transport
    transport-mode: external_bridge
    bridge:
      auth-token: "your-bridge-auth-token"
    # ... rest of your standard lavalink config (sources, filters, etc.)
```

> [!IMPORTANT]
> The `transport-mode` must be set to `external_bridge`. The default value `koe` preserves standard Lavalink behavior.

> [!NOTE]
> The `auth-token` is used to authenticate the bridge WebSocket connection. Choose a strong, random token and keep it consistent between your Lavalink server and bot configuration.

The bridge WebSocket endpoint is served on the **same port** as the main Lavalink WebSocket (default 2333) at the path `/bridge/v1`.

## Bot Configuration

### 1. Install NuGet Packages

You need both `DisCatSharp.Lavalink` and `DisCatSharp.Voice` (with natives):

```xml
<PackageReference Include="DisCatSharp.Lavalink" Version="..." />
<PackageReference Include="DisCatSharp.Voice" Version="..." />
<PackageReference Include="DisCatSharp.Voice.Natives" Version="..." />
```

### 2. Register Voice Extension

Bridge mode requires the Voice extension to be registered **before** Lavalink connects. Enable `EnableExternalOpus` so it can accept pre-encoded Opus frames from the bridge:

```cs
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Bridge;
using DisCatSharp.Voice;

// Register Voice extension with external Opus support
var voice = discord.UseVoice(new VoiceConfiguration
{
    EnableExternalOpus = true
});
```

For sharded clients:

```cs
await shardedClient.UseVoiceAsync(new VoiceConfiguration
{
    EnableExternalOpus = true
});
```

### 3. Configure Lavalink with Bridge

Add a `Bridge` configuration to your `LavalinkConfiguration`:

```cs
var endpoint = new ConnectionEndpoint
{
    Hostname = "127.0.0.1",
    Port = 2333
};

var lavalinkConfig = new LavalinkConfiguration
{
    Password = "your-lavalink-password",
    RestEndpoint = endpoint,
    SocketEndpoint = endpoint,
    Bridge = new LavalinkBridgeConfiguration
    {
        EnableExternalVoiceBridge = true,
        BridgeEndpoint = new Uri("ws://127.0.0.1:2333/bridge/v1"),
        BridgeAuthToken = "your-bridge-auth-token"
    }
};

var lavalink = discord.UseLavalink();
await lavalink.ConnectAsync(lavalinkConfig);
```

### 4. Use Lavalink as Normal

That's it! Once bridge mode is configured, all existing Lavalink commands work exactly the same. The bridge is transparent to your command handlers:

```cs
// Join a voice channel — bridge mode handles the rest
var session = lavalink.GetGuildSession(guild);
var player = await session.ConnectAsync(voiceChannel);

// Play a track — same API as always
var result = await session.LoadTracksAsync(LavalinkSearchType.Youtube, "never gonna give you up");
await player.PlayAsync(result.Tracks.First());

// Filters, volume, seek — all work the same
await player.SetVolumeAsync(50);
await player.SeekAsync(TimeSpan.FromSeconds(30));
```

Behind the scenes, when bridge mode is active:

1. **DisCatSharp.Lavalink** creates a player on Lavalink (for audio decoding/control)
2. **DisCatSharp.Voice** connects to the Discord voice channel (handles voice gateway + UDP)
3. The **bridge client** receives Opus frames from Lavalink over WebSocket
4. Opus frames are fed into VoiceConnection, which handles **RTP → DAVE E2EE → AEAD → UDP**

## Bridge Configuration Reference

### `LavalinkBridgeConfiguration`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `EnableExternalVoiceBridge` | `bool` | `false` | Enables bridge mode |
| `BridgeEndpoint` | `Uri?` | `null` | WebSocket URI for the bridge (e.g., `ws://localhost:2333/bridge/v1`) |
| `BridgeAuthToken` | `string?` | `null` | Authentication token (must match Lavalink's `bridge.auth-token`) |
| `ReconnectDelay` | `TimeSpan` | 5 seconds | Delay between reconnection attempts |
| `MaxReconnectAttempts` | `int` | 10 | Max reconnect attempts (-1 for unlimited) |

### Lavalink Server Config

| YAML Path | Values | Default | Description |
|-----------|--------|---------|-------------|
| `lavalink.server.transport-mode` | `koe`, `external_bridge` | `koe` | Transport mode selection |
| `lavalink.server.bridge.auth-token` | string | — | Bridge authentication token |

## Docker Compose Example

A complete example with both standard and bridge Lavalink using Docker Compose profiles:

```yaml
services:
  # Standard Lavalink (default profile)
  lavalink:
    image: ghcr.io/lavalink-devs/lavalink:4-alpine
    container_name: lavalink
    profiles: ["default", "standard"]
    restart: unless-stopped
    environment:
      - _JAVA_OPTIONS=-Xmx2G
    ports:
      - "127.0.0.1:2333:2333"
    volumes:
      - ./lavalink/application.yml:/opt/Lavalink/application.yml:ro

  # Bridge Lavalink (bridge profile)
  lavalink-bridge:
    image: ghcr.io/aiko-it-systems/lavalink:bridge
    container_name: lavalink-bridge
    profiles: ["bridge"]
    restart: unless-stopped
    environment:
      - _JAVA_OPTIONS=-Xmx2G
    ports:
      - "127.0.0.1:2333:2333"
    volumes:
      - ./lavalink-bridge/application.yml:/opt/Lavalink/application.yml:ro
```

Switch between modes:

```bash
# Standard Lavalink
docker compose --profile standard up -d

# Bridge Lavalink
docker compose --profile bridge up -d
```

## Troubleshooting

### "Bridge mode requires DisCatSharp.Voice to be registered"

You must call `UseVoice()` (or `UseVoiceAsync()` for sharded clients) **before** connecting to Lavalink. Make sure `EnableExternalOpus` is set to `true`.

### "BridgeEndpoint must be set when EnableExternalVoiceBridge is true"

The `BridgeEndpoint` URI is required when bridge mode is enabled. Make sure it points to your Lavalink server's bridge endpoint (same host/port as Lavalink, path `/bridge/v1`).

### "External Opus input is not enabled"

Set `EnableExternalOpus = true` in your `VoiceConfiguration` when registering the Voice extension.

### Bridge client keeps reconnecting

Check that:
1. Your Lavalink server is running with `transport-mode: external_bridge`
2. The `auth-token` matches between bot and server config
3. The bridge endpoint URI is correct and reachable

### Audio plays but no sound in Discord

Verify that the bridge Lavalink container is using the custom fork image (`ghcr.io/aiko-it-systems/lavalink:bridge`), not the stock Lavalink image. The stock image doesn't have the bridge transport.
