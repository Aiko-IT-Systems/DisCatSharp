---
uid: modules_audio_lavalink_v4_queue_system
title: Lavalink V4 Queue System
author: DisCatSharp Team
---

# Lavalink Guild Player Queue System

The Lavalink module provides a built-in queue system for managing audio tracks. The queue system allows you to add, remove, and manipulate tracks in a guild-specific queue.

## Enabling the Queue System
The queue system is disabled by default, To use the queue system, you must enable it in your [`LavalinkConfiguration`](xref:DisCatSharp.Lavalink.LavalinkConfiguration):

```cs
var config = new LavalinkConfiguration
{
    EnableBuiltInQueueSystem = true
};
```
## Basic Queue Operations
### Adding Tracks
You can add individual tracks or entire playlists to the queue:

```cs
// Add a single track
guildPlayer.AddToQueue(track);

// Add all tracks from a playlist
    var loadResult = await guildPlayer.LoadTracksAsync(LavalinkSearchType.Youtube, query);
var playlist = loadResult.GetResultAs<LavalinkPlaylist>()
guildPlayer.AddToQueue(playlist);
```

### Playing the Queue
To start playing tracks from the queue:
```cs
guildPlayer.PlayQueue();
```

The finished commands should look like so:
```cs
[SlashCommand("play", "Play a track")]
public async Task PlayAsync(InteractionContext ctx, [Option("query", "The query to search for")] string query)
{
    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
    if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
    {
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
        return;
    }

    var lavalink = ctx.Client.GetLavalink();
    var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);

    if (guildPlayer == null)
    {
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Lavalink is not connected."));
        return;
    }

    var loadResult = await guildPlayer.LoadTracksAsync(LavalinkSearchType.Youtube, query);

    if (loadResult.LoadType == LavalinkLoadResultType.Empty || loadResult.LoadType == LavalinkLoadResultType.Error)
    {
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Track search failed for {query}."));
        return;
    }

    LavalinkTrack track = loadResult.LoadType switch
    {
        LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
        LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
        LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
        _ => throw new InvalidOperationException("Unexpected load result type.")
    };

    if (guildPlayer.CurrentTrack == null)
    {
        guildPlayer.AddToQueue(track);
        guildPlayer.PlayQueue();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Now playing {query}!"));
    }
    else
    {
        guildPlayer.AddToQueue(track);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Track added to queue {track.Info.Title}!"));
    }
}
```
### Skipping the Current Track
To skip the currently playing track, you can use the `SkipAsync` method of the `LavalinkGuildPlayer` class. This method will skip the current track. If the queue system is enabled and there are more tracks in the queue, the next track will automatically start playing.
```cs
await guildPlayer.SkipAsync();
```

### Managing the Queue

The queue system provides several other methods for managing tracks:
```cs
// See list queue
guildPlayer.Queue;

// Remove a specific track
guildPlayer.RemoveQueue(track);

// Remove a track by its title/identifier
guildPlayer.RemoveQueue("song title");

// Clear all tracks from the queue
guildPlayer.ClearQueue();

// Shuffle the queue
guildPlayer.ShuffleQueue();

// Reverse the queue order
guildPlayer.ReverseQueue();

...
```
## Queue Entry Pipeline
The Lavalink queue system includes a powerful pipeline system that allows you to execute custom actions before and after track playback. This is implemented through the `IQueueEntry` interface.
### Queue Entry Lifecycle
Each track in the queue goes through the following pipeline stages:
1. **Before Playing**: Called right before a track starts playing
2. **Track Playback**: The actual track playback
3. **After Playing**: Called after track playback completes

### Creating Custom Queue Entries
To create a custom queue entry, implement the `IQueueEntry` interface:

```cs
public class CustomQueueEntry : IQueueEntry
{
    public LavalinkTrack Track { get; set; }

    public async Task<bool> BeforePlayingAsync(LavalinkGuildPlayer player)
    {
        // Execute code before the track plays
        // Return false to skip this track
        return true;
    }

    public async Task AfterPlayingAsync(LavalinkGuildPlayer player)
    {
        // Execute code after the track finishes playing
    }
}
```

### Configuring Custom Queue Entries
To use custom queue entries, configure them in your `LavalinkConfiguration`:
```cs
var config = new LavalinkConfiguration
{
    EnableBuiltInQueueSystem = true,
    QueueEntryFactory = () => new CustomQueueEntry()
};
```
or
```cs
 services.AddTransient<IQueueEntry, CustomQueueEntry>();

var config = new LavalinkConfiguration
{
    EnableBuiltInQueueSystem = true,
    QueueEntryFactory = () => scope.ServiceProvider.GetRequiredService<IQueueEntry>
};
```
In case you use dependency injection in `CustomQueueEntry`

### Pipeline Flow Control
- The `BeforePlayingAsync` method can control whether a track should be played by returning `true` or `false`
- The `AfterPlayingAsync` method is called after the track finishes, allowing for cleanup or next-track preparation
- Both methods have access to the `LavalinkGuildPlayer` instance for advanced control

Example usage of flow control:
```cs
public async Task<bool> BeforePlayingAsync(LavalinkGuildPlayer player)
{
    if (/* some condition */)
    {
        // Skip this track
        return false;
    }

    // Play this track
    await player.Channel.SendMessageAsync($"Track Started: {Track.Info.Title}");
    return true;
}
```
