---
uid: modules_audio_lavalink_advanced
title: Lavalink Advanced
---

# Advanced Lavalink Usage

Given the new possibilities to use Lavalink with Spotify and Apple Music, we go a bit deeper into this topic here.

## Preamble
For this to work, you need the following plugin installed in your Lavalink server: https://github.com/Topis-Lavalink-Plugins/Topis-Source-Managers-Plugin


## Setup

General config:
```yml
lavalink:
  plugins:
    - dependency: "com.github.Topis-Lavalink-Plugins:Topis-Source-Managers-Plugin:vx.x.x" # replace vx.x.x with the latest release tag!
      repository: "https://jitpack.io"
[..]
plugins:
  topissourcemanagers:
    providers: # Custom providers for track loading. This is the default
      - "ytsearch:\"%ISRC%\"" # Will be ignored if track does not have an ISRC. See https://esearch.wikipedia.org/wiki/International_Standard_Recording_Code
      - "ytsearch:%QUERY%" # Will be used if track has no ISRC or no track could be found for the ISRC
    # - "scsearch:%QUERY%" you can add multiple other fallback sources here
    sources:
      spotify: true # Enable Spotify source
      applemusic: true # Enable Apple Music source
```

### Spotify Setup
```yml
plugins:
  topissourcemanagers:
[..]
    spotify:
        clientId: "your client id"
        clientSecret: "your client secret"
        countryCode: "US" # the country code you want to use for filtering the artists top tracks. See https://esearch.wikipedia.org/wiki/ISO_3166-1_alpha-2
```

### Apple Music Setup
```yml
plugins:
  topissourcemanagers:
[..]
    applemusic:
        countryCode: "US" # the country code you want to use for filtering the artists top tracks and language. See https://esearch.wikipedia.org/wiki/ISO_3166-1_alpha-2
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
LavalinkLoadResult result = await nodeConnection.Rest.GetTracksAsync(search, type);

// Internally Lavalink searches the Spotify api, and then searches YouTube for the result.
// We can now use the result and play the song.
// Same for Apple Music.
```
## Conclusion

This is the most basic example of how to use the new Lavalink features. You can find more information in the Lavalink plugin docs: https://github.com/Topis-Lavalink-Plugins/Topis-Source-Managers-Plugin#usage
