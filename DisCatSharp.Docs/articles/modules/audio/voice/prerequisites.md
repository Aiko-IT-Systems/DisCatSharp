---
uid: modules_audio_voice_prerequisites
title: Voice Prerequisites
---

# Voice Prerequisites

> [!WARNING]
> [DisCatSharp.Voice](xref:modules_audio_voice) is highly experimental and may not be suitable for production use. It is recommended to thoroughly test your implementation in a staging environment before deploying to production.

## Required Libraries

Voice depends on the [libsodium](https://github.com/jedisct1/libsodium) and [Opus](https://opus-codec.org/) libraries to decrypt and process audio packets.<br/>
Both _must_ be available on your development and host machines otherwise Voice will _not_ work.

### Windows

When installing Voice though NuGet, an additional package containing the native Windows binaries will automatically be included with **no additional steps required**.

However, if you are using DisCatSharp from source or without a NuGet package manager, you must manually download the binaries and place them at the root of your working directory where your application is located.

If the package doesn't work, you might have to instruct msbuild to include it:

```xml
<PackageReference Include="DisCatSharp.Voice.Natives" Version="10.7.0-nightly-069">
	<IncludeAssets>runtime; native; contentfiles</IncludeAssets>
</PackageReference>
```


### MacOS

Native libraries for Apple's macOS can be installed using the [Homebrew](https://brew.sh) package manager:

```console
$ brew install opus libsodium
```

### Linux

#### Debian and Derivatives

Opus package naming is consistent across Debian, Ubuntu, and Linux Mint.

```bash
sudo apt-get install libopus0 libopus-dev
```

Package naming for _libsodium_ will vary depending on your distro and version:

|           Distributions            |                 Terminal Command                 |
| :--------------------------------: | :----------------------------------------------: |
|     Ubuntu 18.04+, Debian 10+      | `sudo apt-get install libsodium23 libsodium-dev` |
| Linux Mint, Ubuntu 16.04, Debian 9 | `sudo apt-get install libsodium18 libsodium-dev` |
|              Debian 8              | `sudo apt-get install libsodium13 libsodium-dev` |

---

## DAVE E2EE Encryption

DisCatSharp.Voice supports **DAVE** — Discord's end-to-end encryption protocol for voice channels.

### What is DAVE?

DAVE (Discord Audio/Video Encryption) is Discord's E2EE protocol that encrypts voice audio between clients. Unlike traditional transport encryption (which only protects data between your bot and the Discord server), DAVE ensures the Discord server itself cannot decrypt audio. It is built on the [MLS (Messaging Layer Security)](https://www.rfc-editor.org/rfc/rfc9420) standard for group key agreement, combined with **AES-128-GCM** per-frame encryption of each RTP audio packet.

### Native Library — libdave

DAVE support requires `libdave` — Discord's official C++ library that implements:

- MLS group key agreement (key packages, proposals, commits, welcomes)
- Per-sender ratchet key derivation
- AES-128-GCM frame encryption and decryption
- Nonce management and replay protection

`libdave` is distributed as a prebuilt native binary inside the `DisCatSharp.Voice.Natives` package. Prebuilt binaries are included for:

| Platform | Architectures |
| :------: | :-----------: |
| Windows  | x64           |
| Linux    | x64, arm64    |
| macOS    | x64, arm64    |

> [!NOTE]
> `DisCatSharp.Voice.Natives` is automatically included when you install `DisCatSharp.Voice` via NuGet. It ships `libdave`, `opus`, and (on Windows) `libsodium` — all required native libraries bundled in one package. No manual download is needed on NuGet installations.

If `libdave` cannot be loaded at runtime (e.g. when building from source without the Natives package), DAVE encryption is automatically disabled for that session and voice will continue to operate using standard transport encryption only.

---

## See Also

- [Transmitting Audio](xref:modules_audio_voice_transmit)
- [Receiving Audio](xref:modules_audio_voice_receive)
- [Voice Architecture](xref:modules_audio_voice_architecture)
- [Migration Guide](xref:modules_audio_voice_migration)
