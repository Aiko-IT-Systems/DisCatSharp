---
uid: modules_audio_lavalink_v4_setup
title: Lavalink V4 Setup
author: DisCatSharp Team
---

# Lavalink Setup

## Configuring Java

In order to run Lavalink, you must have Java 17 or greater installed. For more details check the [requirements](https://github.com/lavalink-devs/Lavalink/tree/master?tab=readme-ov-file#requirements) before downloading.
The latest v17 releases can be found [here](https://www.oracle.com/java/technologies/downloads/#java17).

Make sure the location of the newest JRE's bin folder is added to your system variable's path. This will make the `java` command run from the latest runtime. You can verify that you have the right version by entering `java -version` in your command prompt or terminal.

## Downloading Lavalink

Download the Lavalink V4 server from the [GitHub](https://github.com/lavalink-devs/Lavalink/releases).

To use the Lavalink server, you need to configure it first.

Create a new YAML file called `application.yml`, and use the [example file](https://github.com/lavalink-devs/Lavalink/blob/dev/LavalinkServer/application.yml.example), or copy this snippet:

>[!NOTE]
> For YouTube Support, see the sections `plugins -> youtube` and `lavalink -> plugins` in the config.

```yaml
server:
  port: 2333
  address: 127.0.0.1 # Set it to 0.0.0.0 if you run it in docker or want to share it
  http2:
    enabled: false # Personally we don't see any reason currently to enable it
plugins:
  youtube: # To read more about it's configuration visit https://github.com/lavalink-devs/youtube-source#plugin
    enabled: true
    allowSearch: true
    allowDirectVideoIds: true
    allowDirectPlaylistIds: true
    clients: # We suggest using it like this, since it switches through the clients to get streams and search running
      - M_WEB
      - WEB
      - MUSIC
      - WEBEMBEDDED
      - ANDROID_VR
      - TV
      - TVHTML5EMBEDDED
    oauth: # Read https://github.com/lavalink-devs/youtube-source?tab=readme-ov-file#using-oauth-tokens for more information
      enabled: true
      skipInitialization: false # Set to true if you got your refresh token
      # refreshToken: "" # Fill out after u got the refresh token
    pot: # Read https://github.com/lavalink-devs/youtube-source?tab=readme-ov-file#using-a-potoken for more information or use this one
      token: "CgtLYnJKeDl1N0pJMCjW2cO8BjIKCgJERRIEEgAgKg=="
      visitorData: "MnRuJyEtJOv8LW4fImJbwY4qXcflEPdSWXwKWnQappnJt4Ee_3bFCJEUmiePXV3jvyjxMuT8pE3j-ZKoLtF-bIjo7-erKATkj38QRYgrGRsEHDC97Qk9a-tcYdXpmMQt2h6A1S325QgSsRfbfjBfBTeDq_oZBA=="
  lavasrc: # To read more about it's configuration visit https://github.com/topi314/LavaSrc?tab=readme-ov-file
    providers:
      - "ytsearch:\"%ISRC%\""
      - "ytsearch:%QUERY%"
      - "scsearch:%QUERY%"
      - "spsearch:%QUERY%"
      - "sprec:%QUERY%"
      - "ymsearch:%QUERY%"
      #- "amsearch:%QUERY%"
      #- "dzisrc:%ISRC%"
      #- "dzsearch:%QUERY%"
      - "ymsearch:%QUERY%"
    sources:
      spotify: true
      applemusic: false
      deezer: false
      yandexmusic: true
      flowerytts: true
      vkmusic: false
      youtube: true
    lyrics-sources:
      spotify: true
      deezer: false
      youtube: true
      yandexmusic: true
      vkmusic: false
    spotify:
        clientId: "" # Aquire it by visiting https://developer.spotify.com/dashboard/create
        clientSecret: "" # Aquire it by visiting https://developer.spotify.com/dashboard/create
        spDc: "" # Needed for lyrics, if used. Read https://github.com/topi314/LavaSrc?tab=readme-ov-file#spotify for more details.
        playlistLoadLimit: 6
        albumLoadLimit: 6
        resolveArtistsInSearch: false
        localFiles: true
    yandexmusic:
      accessToken: "" # Aquire it by visiting https://oauth.yandex.ru/authorize?response_type=token&client_id=23cabbbdc6cd418abb4b39c32c41195d
      playlistLoadLimit: 1
      albumLoadLimit: 1
      artistLoadLimit: 1
    flowerytts:
      voice: "default voice"
      translate: false
      silence: 0
      speed: 1.0
      audioFormat: "mp3"
  lavalyrics: # To read more about it's configuration visit https://github.com/topi314/LavaLyrics?tab=readme-ov-file#lavalink-usage
    sources:
      - spotify
      - youtube
      #- deezer
      - yandexMusic
lavalink:
  plugins:
    - dependency: "dev.lavalink.youtube:youtube-plugin:1.11.3" # Source: https://github.com/lavalink-devs/youtube-source
      snapshot: false
    - dependency: "com.github.topi314.lavasearch:lavasearch-plugin:1.0.0" # Source: https://github.com/topi314/LavaSearch
      repository: "https://maven.lavalink.dev/releases"
      snapshot: false
    - dependency: "com.github.topi314.lavasrc:lavasrc-plugin:4.3.0" # Source: https://github.com/topi314/LavaSrc
      repository: "https://maven.lavalink.dev/releases"
      snapshot: false
    - dependency: "com.github.topi314.sponsorblock:sponsorblock-plugin:3.0.0" # Source: https://github.com/topi314/SponsorBlock-Plugin
      repository: "https://maven.lavalink.dev/releases"
      snapshot: false
    - dependency: "com.github.topi314.lavalyrics:lavalyrics-plugin" # Source: https://github.com/topi314/LavaLyrics
      repository: "https://maven.lavalink.dev/releases"
      snapshot: false
  server:
    password: "youshallnotpassMEOW" # Set your lavalink password
    sources:
      youtube: false # Disabled youtube because it's not maintained anymore and got replaced by youtube-plugin
      bandcamp: true
      soundcloud: true
      twitch: true
      vimeo: true
      http: true
      local: true
      nico: true
    filters:
      volume: true
      equalizer: true
      karaoke: true
      timescale: true
      tremolo: true
      vibrato: true
      distortion: true
      rotation: true
      channelMix: true
      lowPass: true
    bufferDurationMs: 400
    frameBufferDurationMs: 5000
    opusEncodingQuality: 10
    resamplingQuality: MEDIUM
    trackStuckThresholdMs: 10000
    useSeekGhosting: true
    youtubePlaylistLoadLimit: 6
    playerUpdateInterval: 5
    youtubeSearchEnabled: true
    soundcloudSearchEnabled: true
    gc-warnings: true

metrics:
  prometheus:
    enabled: false
    endpoint: /metrics

sentry:
  dsn: ""
  environment: "dev"
logging:
  file:
    path: ./logs/

  level:
    root: INFO
    lavalink: INFO
    dev.lavalink.youtube.http.YoutubeOauth2Handler: INFO

  request:
    enabled: true
    includeClientInfo: true
    includeHeaders: false
    includeQueryString: true
    includePayload: true
    maxPayloadLength: 10000


  logback:
    rollingpolicy:
      max-file-size: 1GB
      max-history: 30
```
YAML is whitespace-sensitive. Make sure you are using a text editor which properly handles this.


There are a few values to keep in mind.

The `host` is the IP of the Lavalink host. This will be `0.0.0.0` by default, but it should be changed as it is a security risk. For this guide, set this to `127.0.0.1` as we will be running Lavalink locally.

`port` is the allowed port for the Lavalink connection. `2333` is the default port, and is what will be used for this guide.

The `password` is the password that you will need to specify when connecting. This can be anything as long as it is a valid YAML string. Keep it as `youshallnotpass` for this guide.

When you are finished configuring this, save the file in the same directory as your Lavalink executable.

Keep note of your `port`, `address`, and `password` values, as you will need them later for connecting.

## Starting Lavalink

Open your command prompt or terminal and navigate to the directory containing Lavalink.

Once there, type `java -jar Lavalink.jar`. You should start seeing log output from Lavalink.

If everything is configured properly, you should see this appear somewhere in the log output without any errors:
```yml
INFO 1 --- [Lavalink] [           main] lavalink.server.Launcher                 : Starting Launcher v4.0.8 using Java 18.0.2.1 with PID 1 (/opt/Lavalink/Lavalink.jar started by lavalink in /opt/Lavalink)
INFO 1 --- [Lavalink] [           main] lavalink.server.Launcher                 : No active profile set, falling back to 1 default profile: "default"
INFO 1 --- [Lavalink] [           main] l.server.bootstrap.PluginManager         : Found plugin 'lavasearch-plugin' version 1.0.0
INFO 1 --- [Lavalink] [           main] l.server.bootstrap.PluginManager         : Found plugin 'lavasrc-plugin' version 4.3.0
INFO 1 --- [Lavalink] [           main] l.server.bootstrap.PluginManager         : Found plugin 'sponsorblock-plugin' version 3.0.0
INFO 1 --- [Lavalink] [           main] l.server.bootstrap.PluginManager         : Found plugin 'youtube-plugin' version 1.11.3
INFO 1 --- [Lavalink] [           main] l.server.bootstrap.PluginManager         : Loaded lavasearch-plugin-1.0.0.jar (20 classes)
INFO 1 --- [Lavalink] [           main] l.server.bootstrap.PluginManager         : Loaded lavasrc-plugin-4.3.0.jar (168 classes)
INFO 1 --- [Lavalink] [           main] l.server.bootstrap.PluginManager         : Loaded sponsorblock-plugin-3.0.0.jar (90 classes)
INFO 1 --- [Lavalink] [           main] l.server.bootstrap.PluginManager         : Loaded youtube-plugin-1.11.3.jar (16 classes)
INFO 1 --- [Lavalink] [           main] lavalink.server.Launcher                 : Started Launcher in 2.194 seconds (process running for 2.752)
INFO 1 --- [Lavalink] [           main] lavalink.server.Launcher                 :

       .   _                  _ _       _    __ _ _
      /\\ | | __ ___   ____ _| (_)_ __ | | __\ \ \ \
     ( ( )| |/ _` \ \ / / _` | | | '_ \| |/ / \ \ \ \
      \\/ | | (_| |\ V / (_| | | | | | |   <   ) ) ) )
       '  |_|\__,_| \_/ \__,_|_|_|_| |_|_|\_\ / / / /
    =========================================/_/_/_/

        Version:        4.0.8
        Build time:     20.09.2024 20:20:10 UTC
        Branch          HEAD
        Commit:         2946608
        Commit time:    20.09.2024 20:17:58 UTC
        JVM:            18.0.2.1
        Lavaplayer      2.2.2

INFO 1 --- [Lavalink] [           main] lavalink.server.Launcher                 : No active profile set, falling back to 1 default profile: "default"
WARN 1 --- [Lavalink] [           main] io.undertow.websockets.jsr               : UT026010: Buffer pool was not set on WebSocketDeploymentInfo, the default pool will be used
INFO 1 --- [Lavalink] [           main] io.undertow.servlet                      : Initializing Spring embedded WebApplicationContext
INFO 1 --- [Lavalink] [           main] w.s.c.ServletWebServerApplicationContext : Root WebApplicationContext: initialization completed in 712 ms
INFO 1 --- [Lavalink] [           main] c.g.t.lavasrc.plugin.LavaSrcPlugin       : Loading LavaSrc plugin...
INFO 1 --- [Lavalink] [           main] c.g.t.lavasrc.plugin.LavaSrcPlugin       : Registering Youtube Source audio source manager...
INFO 1 --- [Lavalink] [           main] c.g.t.lavasrc.plugin.LavaSrcPlugin       : Registering Spotify search manager...
INFO 1 --- [Lavalink] [           main] c.g.t.lavasrc.plugin.LavaSrcPlugin       : Registering Youtube search manager...
INFO 1 --- [Lavalink] [           main] c.g.t.lavasrc.plugin.LavaSrcPlugin       : Registering Yandex Music search manager...
INFO 1 --- [Lavalink] [           main] c.s.d.l.tools.GarbageCollectionMonitor   : GC monitoring enabled, reporting results every 2 minutes.
INFO 1 --- [Lavalink] [           main] c.g.t.lavasrc.plugin.LavaSrcPlugin       : Registering Spotify audio source manager...
INFO 1 --- [Lavalink] [           main] c.g.t.lavasrc.plugin.LavaSrcPlugin       : Registering Yandex Music audio source manager...
INFO 1 --- [Lavalink] [           main] c.g.t.lavasrc.plugin.LavaSrcPlugin       : Registering Flowery TTS audio source manager...
2025-01-22T14:52:24.055Z  WARN 1 --- [Lavalink] [           main] d.l.youtube.plugin.ClientProvider        : Failed to resolve M_WEB into a Client
INFO 1 --- [Lavalink] [           main] d.l.youtube.plugin.YoutubePluginLoader   : YouTube source initialised with clients: WEB, WEB_REMIX, WEB_EMBEDDED_PLAYER, ANDROID_VR, TVHTML5, TVHTML5_SIMPLY_EMBEDDED_PLAYER
INFO 1 --- [Lavalink] [           main] d.l.youtube.http.YoutubeOauth2Handler    : ==================================================
INFO 1 --- [Lavalink] [           main] d.l.youtube.http.YoutubeOauth2Handler    : !!! DO NOT AUTHORISE WITH YOUR MAIN ACCOUNT, USE A BURNER !!!
INFO 1 --- [Lavalink] [           main] d.l.youtube.http.YoutubeOauth2Handler    : OAUTH INTEGRATION: To give youtube-source access to your account, go to https://www.google.com/device and enter code <redacted>
INFO 1 --- [Lavalink] [           main] d.l.youtube.http.YoutubeOauth2Handler    : !!! DO NOT AUTHORISE WITH YOUR MAIN ACCOUNT, USE A BURNER !!!
INFO 1 --- [Lavalink] [           main] d.l.youtube.http.YoutubeOauth2Handler    : ==================================================
Picked up _JAVA_OPTIONS: -Xmx6G
INFO 1 --- [Lavalink] [           main] c.g.t.s.plugin.SponsorBlockPlugin        : Loading SponsorBlock Plugin...
INFO 1 --- [Lavalink] [           main] l.server.config.KoeConfiguration         : OS: LINUX, Arch: X86_64
INFO 1 --- [Lavalink] [           main] l.server.config.KoeConfiguration         : Enabling JDA-NAS
INFO 1 --- [Lavalink] [           main] c.s.l.c.natives.NativeLibraryLoader      : Native library udpqueue: loading with filter null
INFO 1 --- [Lavalink] [           main] c.s.l.c.natives.NativeLibraryLoader      : Native library udpqueue: successfully loaded.
2025-01-22T14:52:24.541Z  WARN 1 --- [Lavalink] [           main] l.server.config.SentryConfiguration      : Turning off sentry
INFO 1 --- [Lavalink] [           main] io.undertow                              : starting server: Undertow - 2.3.13.Final
INFO 1 --- [Lavalink] [           main] org.xnio                                 : XNIO version 3.8.8.Final
INFO 1 --- [Lavalink] [           main] org.xnio.nio                             : XNIO NIO Implementation Version 3.8.8.Final
INFO 1 --- [Lavalink] [           main] org.jboss.threads                        : JBoss Threads version 3.5.0.Final
INFO 1 --- [Lavalink] [           main] o.s.b.w.e.undertow.UndertowWebServer     : Undertow started on port 2333 (http) with context path '/'
INFO 1 --- [Lavalink] [           main] lavalink.server.Launcher                 : Started Launcher in 3.43 seconds (process running for 6.19)
INFO 1 --- [Lavalink] [           main] lavalink.server.Launcher                 : Lavalink is ready to accept connections.
```

If it does, congratulations. We are now ready to interact with it using DisCatSharp.

## YouTube OAuth Token

If you configured OAuth correctly, you should see the following in your logs:
```yml
INFO 1 --- [Lavalink] [           main] d.l.youtube.http.YoutubeOauth2Handler    : ==================================================
INFO 1 --- [Lavalink] [           main] d.l.youtube.http.YoutubeOauth2Handler    : !!! DO NOT AUTHORISE WITH YOUR MAIN ACCOUNT, USE A BURNER !!!
INFO 1 --- [Lavalink] [           main] d.l.youtube.http.YoutubeOauth2Handler    : OAUTH INTEGRATION: To give youtube-source access to your account, go to https://www.google.com/device and enter code XXX-XXX-XXX
INFO 1 --- [Lavalink] [           main] d.l.youtube.http.YoutubeOauth2Handler    : !!! DO NOT AUTHORISE WITH YOUR MAIN ACCOUNT, USE A BURNER !!!
INFO 1 --- [Lavalink] [           main] d.l.youtube.http.YoutubeOauth2Handler    : ==================================================
```
