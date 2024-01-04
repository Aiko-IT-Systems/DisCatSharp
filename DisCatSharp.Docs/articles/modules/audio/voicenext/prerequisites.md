---
uid: modules_audio_voicenext_prerequisites
title: VoiceNext Prerequisites
---

# VoiceNext Prerequisites

> [!NOTE]
> We highly suggest using the [DisCatSharp.Lavalink](xref:modules_audio_lavalink_v4_intro) package for audio playback. It is much easier to use and has a lot of features that VoiceNext does not have.

## Required Libraries

VoiceNext depends on the [libsodium](https://github.com/jedisct1/libsodium) and [Opus](https://opus-codec.org/) libraries to decrypt and process audio packets.<br/>
Both _must_ be available on your development and host machines otherwise VoiceNext will _not_ work.

### Windows

When installing VoiceNext though NuGet, an additional package containing the native Windows binaries will automatically be included with **no additional steps required**.

However, if you are using DisCatSharp from source or without a NuGet package manager, you must manually [download](xref:natives) the binaries and place them at the root of your working directory where your application is located.

If the package doesn't work, you might have to instruct msbuild to include it:

```xml
<PackageReference Include="DisCatSharp.VoiceNext.Natives" Version="10.6.0-nightly-001">
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
