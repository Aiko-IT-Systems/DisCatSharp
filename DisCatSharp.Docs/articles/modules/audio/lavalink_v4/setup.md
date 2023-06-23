---
uid: modules_audio_lavalink_v4_setup
title: Lavalink Setup
---

# Lavalink Setup

## Configuring Java
In order to run Lavalink, you must have Java 17 or greater installed. Certain Java versions may not be functional with Lavalink, so it is best to check the [requirements](https://github.com/lavalink-devs/Lavalink/tree/v4#requirements) before downloading.
The latest releases can be found [here](https://www.oracle.com/technetwork/java/javase/downloads/index.html).

Make sure the location of the newest JRE's bin folder is added to your system variable's path. This will make the `java` command run from the latest runtime. You can verify that you have the right version by entering `java -version` in your command prompt or terminal.

## Downloading Lavalink
<!--Next, head over to the [releases](https://github.com/freyacodes/Lavalink/releases) tab on the Lavalink GitHub page and download the Jar file from the latest version. Alternatively, stable builds with the latest changes can be found on their [CI Server](https://ci.fredboat.com/viewLog.html?buildId=lastSuccessful&buildTypeId=Lavalink_Build&tab=artifacts&guest=1).-->

>[!NOTE]
> Lavalink V4 is required but currently not in stable release.
>
> We are providing a direct download from the pre-release ci.

Download the Lavalink V4 server from [here](/natives/Lavalink.jar).

The program will not be ready to run yet, as you will need to create a configuration file first. To do so, create a new YAML file called `application.yml`, and use the [example file](https://github.com/lavalink-devs/Lavalink/blob/v4/LavalinkServer/application.yml.example), or copy this text:

```yaml
server: # REST and WS server
  port: 2333
  address: 0.0.0.0
plugins:
#  name: # Name of the plugin
#    some_key: some_value # Some key-value pair for the plugin
#    another_key: another_value
lavalink:
  plugins:
#    - dependency: "group:artifact:version"
#      repository: "repository"
  server:
    password: "youshallnotpass"
    sources:
      youtube: true
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
    #youtubeConfig: # Required for avoiding all age restrictions by YouTube, some restricted videos still can be played without.
      #email: "" # Email of Google account
      #password: "" # Password of Google account
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

`host` is the IP of the Lavalink host. This will be `0.0.0.0` by default, but it should be changed as it is a security risk. For this guide, set this to `127.0.0.1` as we will be running Lavalink locally.

`port` is the allowed port for the Lavalink connection. `2333` is the default port, and is what will be used for this guide.

`password` is the password that you will need to specify when connecting. This can be anything as long as it is a valid YAML string. Keep it as `youshallnotpass` for this guide.

When you are finished configuring this, save the file in the same directory as your Lavalink executable.

Keep note of your `port`, `address`, and `password` values, as you will need them later for connecting.

## Starting Lavalink

Open your command prompt or terminal and navigate to the directory containing Lavalink. Once there, type `java -jar Lavalink.jar`. You should start seeing log output from Lavalink.

If everything is configured properly, you should see this appear somewhere in the log output without any errors:
```
[           main] lavalink.server.Launcher                 : Started Launcher in 5.769 seconds (JVM running for 6.758)
```

If it does, congratulations. We are now ready to interact with it using DisCatSharp.
