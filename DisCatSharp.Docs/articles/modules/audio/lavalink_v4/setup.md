---
uid: modules_audio_lavalink_v4_setup
title: Lavalink V4 Setup
author: DisCatSharp Team
---

# Lavalink Setup

## Configuring Java

In order to run Lavalink, you must have Java 17 or greater installed. For more details check the [requirements](https://github.com/lavalink-devs/Lavalink/tree/dev#requirements) before downloading.
The latest v17 releases can be found [here](https://www.oracle.com/java/technologies/downloads/#java17).

Make sure the location of the newest JRE's bin folder is added to your system variable's path. This will make the `java` command run from the latest runtime. You can verify that you have the right version by entering `java -version` in your command prompt or terminal.

## Downloading Lavalink

Download the Lavalink V4 server from the [GitHub](https://github.com/lavalink-devs/Lavalink/releases/tag/4.0.0).

To use the Lavalink server, you need to configure it first.

Create a new YAML file called `application.yml`, and use the [example file](https://github.com/lavalink-devs/Lavalink/blob/dev/LavalinkServer/application.yml.example), or copy this snippet:

>[!NOTE]
> For YouTube Support, see the sections `plugins -> youtube` and `lavalink -> plugins` in the config.

```yaml
server: # REST and WS server
  port: 2333
  address: 0.0.0.0
plugins: # Uncomment the youtube config if you enabled the youtube plugin down below.
#  youtube:
#    enabled: true
#    clients: ["MUSIC", "ANDROID", "WEB"] # Log in credentials are no longer support for bypassing age-gated videos. Instead, add 'TVHTML5EMBEDDED' to the clients. Do keep in mind that, even with this client enabled, age-restricted tracks are not guaranteed to play.
lavalink:
  plugins: # Uncomment this plugin for youtube support. Replace VERSION with the latest version from here: https://github.com/lavalink-devs/youtube-source/releases
#    - dependency: "com.github.lavalink-devs.lavaplayer-youtube-source:plugin:VERSION"
#      repository: "https://jitpack.io"
  server:
    password: "youshallnotpass"
    sources: 
      youtube: false # Keep this at false, for youtube support see the plugins section above.
      bandcamp: true
      soundcloud: true
      twitch: true
      vimeo: true
      http: true
      local: false
    filters: # All filters are enabled by default
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
    bufferDurationMs: 400 # The duration of the NAS buffer. Higher values fare better against longer GC pauses. Duration <= 0 to disable JDA-NAS. Minimum of 40ms, lower values may introduce pauses.
    frameBufferDurationMs: 5000 # How many milliseconds of audio to keep buffered
    opusEncodingQuality: 10 # Opus encoder quality. Valid values range from 0 to 10, where 10 is best quality but is the most expensive on the CPU.
    resamplingQuality: LOW # Quality of resampling operations. Valid values are LOW, MEDIUM and HIGH, where HIGH uses the most CPU.
    trackStuckThresholdMs: 10000 # The threshold for how long a track can be stuck. A track is stuck if does not return any audio data.
    useSeekGhosting: true # Seek ghosting is the effect where whilst a seek is in progress, the audio buffer is read from until empty, or until seek is ready.
    youtubePlaylistLoadLimit: 6 # Number of pages at 100 each
    playerUpdateInterval: 5 # How frequently to send player updates to clients, in seconds
    youtubeSearchEnabled: true
    soundcloudSearchEnabled: true
    gc-warnings: true
    #ratelimit:
      #ipBlocks: ["1.0.0.0/8", "..."] # list of ip blocks
      #excludedIps: ["...", "..."] # ips which should be explicit excluded from usage by lavalink
      #strategy: "RotateOnBan" # RotateOnBan | LoadBalance | NanoSwitch | RotatingNanoSwitch
      #searchTriggersFail: true # Whether a search 429 should trigger marking the ip as failing
      #retryLimit: -1 # -1 = use default lavaplayer value | 0 = infinity | >0 = retry will happen this numbers times
    #httpConfig: # Useful for blocking bad-actors from ip-grabbing your music node and attacking it, this way only the http proxy will be attacked
      #proxyHost: "localhost" # Hostname of the proxy, (ip or domain)
      #proxyPort: 3128 # Proxy port, 3128 is the default for squidProxy
      #proxyUser: "" # Optional user for basic authentication fields, leave blank if you don't use basic auth
      #proxyPassword: "" # Password for basic authentication

metrics:
  prometheus:
    enabled: false
    endpoint: /metrics

sentry:
  dsn: ""
  environment: ""
#  tags:
#    some_key: some_value
#    another_key: another_value

logging:
  file:
    path: ./logs/

  level:
    root: INFO
    lavalink: INFO

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
2023-06-25 06:56:56.474  INFO 35056 --- [           main] lavalink.server.Launcher                 : Starting Launcher using Java 11.0.16.1 on AITSYS with PID 35056 (H:\Lavalink.jar started by Lulalaby in H:\)
2023-06-25 06:56:56.514  INFO 35056 --- [           main] lavalink.server.Launcher                 : No active profile set, falling back to 1 default profile: "default"
2023-06-25 06:56:58.946  INFO 35056 --- [           main] lavalink.server.bootstrap.PluginManager  : Found plugin 'lavasrc' version c9aac26
2023-06-25 06:56:59.025  INFO 35056 --- [           main] lavalink.server.bootstrap.PluginManager  : Loaded lavasrc-plugin-c9aac26.jar (29 classes)
2023-06-25 06:56:59.426  INFO 35056 --- [           main] lavalink.server.Launcher                 : Started Launcher in 5.012 seconds (JVM running for 6.469)
2023-06-25 06:56:59.431  INFO 35056 --- [           main] lavalink.server.Launcher                 : You can safely ignore the big red warning about illegal reflection. See https://github.com/lavalink-devs/Lavalink/issues/295
2023-06-25 06:56:59.568  INFO 35056 --- [           main] lavalink.server.Launcher                 :

       .   _                  _ _       _    __ _ _
      /\\ | | __ ___   ____ _| (_)_ __ | | __\ \ \ \
     ( ( )| |/ _` \ \ / / _` | | | '_ \| |/ / \ \ \ \
      \\/ | | (_| |\ V / (_| | | | | | |   <   ) ) ) )
       '  |_|\__,_| \_/ \__,_|_|_|_| |_|_|\_\ / / / /
    =========================================/_/_/_/

        Version:        6f62e585df6f177eb8bf8dfe965ddae4838a1e7b-SNAPSHOT
        Build time:     26.05.2023 12:21:23 UTC
        Branch          v4
        Commit:         6f62e58
        Commit time:    26.05.2023 12:20:13 UTC
        JVM:            11.0.16.1
        Lavaplayer      1.4.1

```

If it does, congratulations. We are now ready to interact with it using DisCatSharp.
