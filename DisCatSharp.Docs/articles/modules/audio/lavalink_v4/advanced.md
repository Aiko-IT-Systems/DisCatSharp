---
uid: modules_audio_lavalink_v4_advanced
title: Lavalink V4 Advanced
author: DisCatSharp Team
---

# Advanced Lavalink Usage

Given the new possibilities to use Lavalink with Spotify, Apple Music, Deezer and Yandex Music, we go a bit deeper into this topic here.

## Preamble

For this to work, you need the following plugin installed in your Lavalink server: https://github.com/topi314/LavaSrc

## Setup

General config:

```yml
lavalink:
  plugins:
    - dependency: "com.github.topi314.lavasrc:lavasrc-plugin:4.0.1"
      repository: "https://maven.topi.wtf/releases"
[..]
plugins:
  lavasrc:
    providers: # Custom providers for track loading. This is the default
      - "ytsearch:\"%ISRC%\"" # Will be ignored if track does not have an ISRC. See https://esearch.wikipedia.org/wiki/International_Standard_Recording_Code
      - "ytsearch:%QUERY%" # Will be used if track has no ISRC or no track could be found for the ISRC
      - "scsearch:%QUERY%" # Soundcloud search provider
      - "spsearch:%QUERY%" # Spotify search provider
      - "sprec:%QUERY%" # Spotify recommendation search provider
      - "ymsearch:%QUERY%" # YouTube Music search provider
      - "amsearch:%QUERY%" # Apple Music search provider
      - "dzisrc:%ISRC%" # Deezer ISRC provider
      - "dzsearch:%QUERY%" # Deezer search provider
      - "ymsearch:%QUERY%" # Yandex Music search provider

    sources:
      spotify: true # Enable Spotify source
      applemusic: true # Enable Apple Music source
      deezer: true # Enable Deezer source
      yandexmusic: true # Enable Yandex Music source
```

### Spotify Setup

```yml
plugins:
  lavasrc:
[..]
    spotify:
        clientId: "your client id"
        clientSecret: "your client secret"
        countryCode: "US" # the country code you want to use for filtering the artists top tracks. See https://esearch.wikipedia.org/wiki/ISO_3166-1_alpha-2
        playlistLoadLimit: 6 # The number of pages at 100 tracks each
        albumLoadLimit: 6 # The number of pages at 50 tracks each
```

### Apple Music Setup

```yml
plugins:
  lavasrc:
[..]
    applemusic:
        countryCode: "US" # the country code you want to use for filtering the artists top tracks and language. See https://esearch.wikipedia.org/wiki/ISO_3166-1_alpha-2
        playlistLoadLimit: 6 # The number of pages at 100 tracks each
        albumLoadLimit: 6 # The number of pages at 50 tracks each
```

### Deezer Setup

```yml
plugins:
  lavasrc:
[..]
    deezer:
      masterDecryptionKey: "your master decryption key" # the master key used for decrypting the deezer tracks. (yes this is not here you need to get it from somewhere else)
```

### Yandex Music Setup

```yml
plugins:
  lavasrc:
[..]
    yandexmusic:
      accessToken: "your access token" # the token used for accessing the yandex music api. See https://github.com/topi314/LavaSrc#yandex-music
```

## Usage

### Client Configuration

You don't have to configure anything directly in DisCatSharp.Lavalink.

### Code Snippets

```cs
// We got the search as string from xy already
string search = "spsearch:Shooting Stars";
// Node connection is already defined.
LavalinkNodeConnection nodeConnection;

// We set YouTube as default search type because it's the most common one.
var type = LavalinkSearchType.Youtube;
if (search.StartsWith("ytsearch:"))
{
	search = search.Replace("ytsearch:", "");
	type = LavalinkSearchType.Youtube;
}
else if (search.StartsWith("scsearch:"))
{
	search = search.Replace("ytsearch:", "");
	type = LavalinkSearchType.SoundCloud;
}
else if (search.StartsWith("spsearch:"))
{
	search = search.Replace("spsearch:", "");
	type = LavalinkSearchType.Spotify;
}
else if (search.StartsWith("amsearch:"))
{
	search = search.Replace("amsearch:", "");
	type = LavalinkSearchType.AppleMusic;
}

// This function gives us a result from Lavalink.
LavalinkLoadResult result = await guildPlayer.LoadTracksAsync(type, query);

// Internally Lavalink searches the Spotify api, and then searches YouTube for the result.
// We can now use the result and play the song.
// Same for Apple Music.
```

## Conclusion

This is the most basic example of how to use the new Lavalink features. You can find more information in the Lavalink plugin docs: https://github.com/topi314/LavaSrc#usage
