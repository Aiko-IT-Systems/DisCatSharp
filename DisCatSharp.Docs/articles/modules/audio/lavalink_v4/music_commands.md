---
uid: modules_audio_lavalink_v4_music_commands
title: Lavalink V4 Music Commands
author: DisCatSharp Team
hasDiscordComponents: true
---

# Adding Music Commands

This article assumes that you know how to use CommandsNext. If you do not, you should learn [here](xref:modules_commandsnext_intro) before continuing with this guide.

## Prerequisites

Before we start we will need to make sure CommandsNext is configured. For this we can make a simple configuration and command class:

```cs
using DisCatSharp.CommandsNext;

namespace FirstLavalinkBot;

public class MyFirstLavalinkCommands : BaseCommandModule
{ }
```

And be sure to register it in your program file:

```cs
CommandsNext = Discord.UseCommandsNext(new CommandsNextConfiguration
{
    StringPrefixes = new string[] { "!" }
});

CommandsNext.RegisterCommands<MyLavalinkCommands>();
```

## Adding join and leave commands

Your bot, and Lavalink, will need to connect to a voice channel to play music. Let's create the base for these commands:

```cs
[Command("join", "Join a voice channel")]
public async Task JoinAsync(CommandContext ctx, DiscordChannel channel)
{ }

[Command("leave", "Leave the voice channel")]
public async Task LeaveAsync(CommandContext ctx, DiscordChannel channel)
{ }
```

In order to connect to a voice channel, we'll need to do a few things.

1. Get our node connection. You can either use linq or `GetIdealNodeConnection()`
2. Check if the channel is a voice channel, and tell the user if not.
3. Connect the node to the channel.


And for the leave command:

1. Get the node connection, using the same process.
2. Check if the channel is a voice channel, and tell the user if not.
3. Get our existing connection.
4. Check if the connection exists, and tell the user if not.
5. Disconnect from the channel.

`GetIdealNodeConnection()` will return the least affected node through load balancing, which is useful for larger bots. It can also filter nodes based on an optional voice region to use the closest nodes available. Since we only have one connection we can use linq's `.First()` method on the extensions connected nodes to get what we need.

So far, your command class should look something like this:

```cs
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;

namespace FirstLavalinkBot;

public class MyFirstLavalinkCommands : BaseCommandModule
{
	[Command("join", "Join a voice channel")]
	public async Task JoinAsync(CommandContext ctx, DiscordChannel channel)
	{
		var lava = ctx.Client.GetLavalink();
		if (!lava.ConnectedNodes.Any())
		{
			await ctx.RespondAsync("The Lavalink connection is not established");
			return;
		}

		var node = lava.ConnectedNodes.Values.First();

		if (channel.Type != ChannelType.Voice)
		{
			await ctx.RespondAsync("Not a valid voice channel.");
			return;
		}

		await node.ConnectAsync(channel);
		await ctx.RespondAsync($"Joined {channel.Mention}!");
	}

	[Command("leave", "Leave the voice channel")]
	public async Task LeaveAsync(CommandContext ctx, DiscordChannel channel)
	{
		var lava = ctx.Client.GetLavalink();
		if (!lava.ConnectedNodes.Any())
		{
			await ctx.RespondAsync("The Lavalink connection is not established");
			return;
		}

		var node = lava.ConnectedNodes.Values.First();

		if (channel.Type != ChannelType.Voice)
		{
			await ctx.RespondAsync("Not a valid voice channel.");
			return;
		}

		var conn = node.GetGuildConnection(channel.Guild);

		if (conn == null)
		{
			await ctx.RespondAsync("Lavalink is not connected.");
			return;
		}

		await conn.DisconnectAsync();
		await ctx.RespondAsync($"Left {channel.Mention}!");
	}
}
```

## Adding player commands

Now that we can join a voice channel, we can make our bot play music! Let's now create the base for a play command:

```cs
[Command("play", "Play a track")]
public async Task PlayAsync(CommandContext ctx, [RemainingText] string search)
{ }
```
One of Lavalink's best features is its ability to search for tracks from a variety of media sources, such as YouTube, SoundCloud, Twitch, and more. This is what makes bots like Rythm, Fredboat, and Groovy popular. The search is used in a REST request to get the track data, which is then sent through the WebSocket connection to play the track in the voice channel. That is what we will be doing in this command.

Lavalink can also play tracks directly from a media url, in which case the play command can look like this:

```cs
[Command("play", "Play a track")]
public async Task PlayAsync(CommandContext ctx, Uri url)
{ }
```

Like before, we will need to get our node and guild connection and have the appropriate checks. Since it wouldn't make sense to have the channel as a parameter, we will instead get it from the member's voice state:

```cs
//Important to check the voice state itself first,
//as it may throw a NullReferenceException if they don't have a voice state.
if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
{
    await ctx.RespondAsync("You are not in a voice channel.");
    return;
}

var lava = ctx.Client.GetLavalink();
var node = lava.ConnectedNodes.Values.First();
var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

if (conn == null)
{
    await ctx.RespondAsync("Lavalink is not connected.");
    return;
}
```

Next, we will get the track details by calling `node.Rest.GetTracksAsync()`. There are a variety of overloads for this:

1. `GetTracksAsync(LavalinkSearchType.Youtube, search)` will search YouTube for your search string.
2. `GetTracksAsync(LavalinkSearchType.SoundCloud, search)` will search SoundCloud for your search string.
3. `GetTracksAsync(Uri)` will use the direct url to obtain the track. This is mainly used for the other media sources.

For this guide we will be searching YouTube. Let's pass in our search string and store the result in a variable:

```cs
//We don't need to specify the search type here
//since it is YouTube by default.
var loadResult = await node.Rest.GetTracksAsync(search);
```

The load result will contain an enum called `LoadResultType`, which will inform us if Lavalink was able to retrieve the track data. We can use this as a check:

```cs
//If something went wrong on Lavalink's end or it just couldn't find anything.
if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
	|| loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
{
    await ctx.RespondAsync($"Track search failed for {search}.");
    return;
}
```

Lavalink will return the track data from your search in a collection called `loadResult.Tracks`, similar to using the search bar in YouTube or SoundCloud directly. The first track is typically the most accurate one, so that is what we will use:

```cs
var track = loadResult.Tracks.First();
```

And finally, we can play the track:

```cs
await conn.PlayAsync(track);

await ctx.RespondAsync($"Now playing {track.Title}!");
```

Your play command should look like this:
```cs
[Command("play", "Play a track")]
public async Task PlayAsync(CommandContext ctx, [RemainingText] string search)
{
    if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
    {
        await ctx.RespondAsync("You are not in a voice channel.");
        return;
    }

    var lava = ctx.Client.GetLavalink();
    var node = lava.ConnectedNodes.Values.First();
    var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

    if (conn == null)
    {
        await ctx.RespondAsync("Lavalink is not connected.");
        return;
    }

    var loadResult = await node.Rest.GetTracksAsync(search);

    if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
        || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
    {
        await ctx.RespondAsync($"Track search failed for {search}.");
        return;
    }

    var track = loadResult.Tracks.First();

    await conn.PlayAsync(track);

    await ctx.RespondAsync($"Now playing {track.Title}!");
}
```

Being able to pause the player is also useful. For this we can use most of the base from the play command:

```cs
[Command("pause", "Pause a track")]
public async Task PauseAsync(CommandContext ctx)
{
    if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
    {
        await ctx.RespondAsync("You are not in a voice channel.");
        return;
    }

    var lava = ctx.Client.GetLavalink();
    var node = lava.ConnectedNodes.Values.First();
    var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

    if (conn == null)
    {
        await ctx.RespondAsync("Lavalink is not connected.");
        return;
    }
}
```

For this command we will also want to check the player state to determine if we should send a pause command. We can do so by checking `conn.CurrentState.CurrentTrack`:

```cs
if (conn.CurrentState.CurrentTrack == null)
{
    await ctx.RespondAsync("There are no tracks loaded.");
    return;
}
```

And finally, we can call pause:

```cs
await conn.PauseAsync();
```

The finished command should look like so:
```cs
[Command("pause", "Pause a track")]
public async Task PauseAsync(CommandContext ctx)
{
    if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
    {
        await ctx.RespondAsync("You are not in a voice channel.");
        return;
    }

    var lava = ctx.Client.GetLavalink();
    var node = lava.ConnectedNodes.Values.First();
    var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

    if (conn == null)
    {
        await ctx.RespondAsync("Lavalink is not connected.");
        return;
    }

    if (conn.CurrentState.CurrentTrack == null)
    {
         await ctx.RespondAsync("There are no tracks loaded.");
         return;
    }

    await conn.PauseAsync();
    await ctx.RespondAsync("Paused the playback!");
}
```

Now we can use these commands to listen to music!

## Visual Example

<discord-messages>
    <discord-message profile="user">!join <discord-mention type="voice">Music</discord-mention></discord-message>
    <discord-message profile="dcs" highlight>
        <discord-reply slot="reply" profile="user" mentions>!join <discord-mention type="voice">Music</discord-mention></discord-reply>
        Joined <discord-mention type="voice">Music</discord-mention>!
    </discord-message>
    <discord-message profile="user">!play <a target="_blank" class="discord-link external" href="https://youtu.be/38-cJT320aw">https://youtu.be/38-cJT320aw</a>
<discord-embed
	slot="embeds"
	provider="YouTube"
	author-name="Raon"
	author-url="https://www.youtube.com/channel/UCQn1FqrR2OCjSe6Nl4GlVHw"
	color="#FF0000"
	embed-title="Raon 라온 | ‘クネクネ (Wiggle Wiggle)’ M/V"
	video="https://cdn.aitsys.dev/file/data/q2tzqoz7ua7sfyeulapq/PHID-FILE-3det2pn34p5chh4enez4/38-cJT320aw.mp4"
    url="https://www.youtube.com/watch?v=38-cJT320aw"
    image="https://cdn.aitsys.dev/file/data/kcrwt6baxsmr32rjnrdg/PHID-FILE-2w72lbyg6lrbstqo3geh/38-cJT320aw.jpg"
></discord-embed>
    </discord-message>
    <discord-message profile="dcs" highlight>
        <discord-reply slot="reply" profile="user" mentions>!play <a target="_blank" class="discord-link external" href="https://youtu.be/38-cJT320aw">https://youtu.be/38-cJT320aw</a></discord-reply>
        Now playing Raon 라온 | ‘クネクネ (Wiggle Wiggle)’ M/V!
    </discord-message>
    <discord-message profile="user">!pause</discord-message>
    <discord-message profile="dcs" highlight>
        <discord-reply slot="reply" profile="user" mentions>!pause</discord-reply>
        Paused the playback!
    </discord-message>
    <discord-message profile="user">!leave</discord-message>
    <discord-message profile="dcs" highlight>
        <discord-reply slot="reply" profile="user" mentions>!leave</discord-reply>
        Left <discord-mention type="voice">Music</discord-mention>!
    </discord-message>
</discord-messages>

Of course, there are other commands Lavalink has to offer. Check out [the docs](https://docs.dcs.aitsys.dev/api/DisCatSharp.Lavalink.LavalinkGuildConnection.html#methods) to view the commands you can use while playing tracks.

