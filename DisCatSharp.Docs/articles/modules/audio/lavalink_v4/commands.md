---
uid: modules_audio_lavalink_v4_commands
title: Lavalink V4 Commands
author: DisCatSharp Team
hasDiscordComponents: true
---

# Adding Music Commands

This article assumes that you know how to use ApplicationCommands. If you do not, you should learn [here](xref:modules_application_commands_intro) before continuing with this guide.

## Prerequisites

Before we start we will need to make sure ApplicationCommands is configured. For this we can make a simple configuration and command class:

```cs
using DisCatSharp.ApplicationCommands;
namespace FirstLavalinkBot;

public class MyFirstLavalinkCommands : ApplicationCommandsModule
{ }
```

Next up you gotta register the ApplicationCommands module:

```cs
ApplicationCommands = Discord.UseApplicationCommands();
// Either as a global command
ApplicationCommands.RegisterGlobalCommands<MyFirstLavalinkCommands>();
// Or as a guild command
ulong myGuildId = 858089281214087179;
ApplicationCommands.RegisterGuildCommands<MyFirstLavalinkCommands>(myGuildId);
```

## Adding base commands

Your bot, and Lavalink, will need to connect to a voice channel to play music.

Let's create the base for these commands:

```cs
[SlashCommand("join", "Join a voice channel")]
public async Task JoinAsync(InteractionContext ctx, [Option("channel", "Channel to join")] DiscordChannel channel)
{
	await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
}

[SlashCommand("leave", "Leave the voice channel")]
public async Task LeaveAsync(InteractionContext ctx)
{
	await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
}
```

To get the Lavalink node we will need to use the `GetLavalink()` extension method on the DiscordClient. This will return a [LavalinkExtension](xref:DisCatSharp.Lavalink.LavalinkExtension) object, which contains a collection of connected sessions. Since we only have one node, we can use linq's `.First()` method on the extensions connected nodes to get what we need.

If we have the session, we can use the `ConnectAsync()` method to connect to the voice channel. This method will return a [LavalinkGuildPlayer](xref:DisCatSharp.Lavalink.LavalinkGuildPlayer) object, which we can use to play music. We will also need to check if the channel is a voice channel. If the guild player is already connected, the function will return the existing player.

So far, your command class should look something like this:

```cs
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
namespace FirstLavalinkBot;

public class MyFirstLavalinkCommands : BaseCommandModule
{
	[SlashCommand("join", "Join a voice channel")]
	public async Task JoinAsync(InteractionContext ctx, [Option("channel", "Channel to join")] DiscordChannel channel)
	{
		await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
		var lavalink = ctx.Client.GetLavalink();
		if (!lavalink.ConnectedSessions.Any())
		{
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("The Lavalink connection is not established"));
			return;
		}

		var session = lavalink.ConnectedSessions.Values.First();

		if (channel.Type != ChannelType.Voice || channel.Type != ChannelType.Stage)
		{
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Not a valid voice channel."));
			return;
		}

		await session.ConnectAsync(channel);
		await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Joined {channel.Mention}!"));
	}

	[SlashCommand("leave", "Leave the voice channel")]
	public async Task LeaveAsync(InteractionContext ctx)
	{
		await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
		var lavalink = ctx.Client.GetLavalink();
		var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);
		if (guildPlayer == null)
		{
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Lavalink not connected."));
			return;
		}

		await guildPlayer.DisconnectAsync();
		await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Left {guildPlayer.channel.Mention}!"));
	}
}
```

## Adding playback commands

Now that we have the base commands, we can add the playback commands. For this we will need to get the Lavalink session, the guild connection, and the track details. We will also need to check if Lavalink is connected.

```cs
[SlashCommand("play", "Play a track")]
public async Task PlayAsync(InteractionContext ctx, [Option("query", "The query to search for")] string query)
{
	await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
}
```
One of Lavalink's best features is its ability to search for tracks from a variety of media sources, such as YouTube, SoundCloud, Twitch, and more. This is what makes bots like Rythm, Fredboat, and Groovy popular

Lavalink can also play tracks directly from a media url. This is useful for playing tracks from other sources, such as Bandcamp, Vimeo, and more.

Like before, we will need to get our node and guild connection and have the appropriate checks. Since it wouldn't make sense to have the channel as a parameter, we will instead get it from the member's voice state:

```cs
// Important to check the voice state itself first, as it may throw a NullReferenceException if they don't have a voice state.
if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
{
	await ctx.RespondAsync("You are not in a voice channel.");
	return;
}

var lavalink = ctx.Client.GetLavalink();
var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);

if (guildPlayer == null)
{
	await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Lavalink is not connected."));
	return;
}
```

```cs
var loadResult = await guildPlayer.LoadTracksAsync(LavalinkSearchType.Youtube, query);
```

The load result will contain an enum called `LoadResultType`, which will inform us if Lavalink was able to retrieve the track data. We can use this as a check:

```cs
// If something went wrong on Lavalink's end or it just couldn't find anything.
if (loadResult.LoadType == LavalinkLoadResultType.Empty || loadResult.LoadType == LavalinkLoadResultType.Error)
{
	await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Track search failed for {query}."));
	return;
}
```

Lavalink will return a dynamic result object. This object will contain the track data, as well as the type of result.

We can use the `LoadResultType` to determine what type of result we got. If we now the result type we need to specify the objects type.

A shorthand method exists as `GetResultAs<T>`:

```cs
LavalinkTrack track = loadResult.LoadType switch {
    LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
    LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
    LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().Tracks.First(),
    _ => throw new InvalidOperationException("Unexpected load result type.")
};
```

And finally, we can play the track:

```cs
await guildPlayer.PlayAsync(track);

await ctx.RespondAsync($"Now playing {track.Title}!");
```

Your play command should look like this:
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

	LavalinkTrack track = loadResult.LoadType switch {
        LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
        LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
        LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().Tracks.First(),
        _ => throw new InvalidOperationException("Unexpected load result type.")
    };

	await guildPlayer.PlayAsync(track);

	await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Now playing {query}!"));
}
```

Being able to pause and resume the player is also useful. For this we can use most of the base from the play command:

```cs
[SlashCommand("pause", "Pause a track")]
public async Task PauseAsync(InteractionContext ctx)
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
}
```

For this command we will also want to check the player state to determine if we should send a pause command. We can do so by checking `guildPlayer.CurrentTrack`:

```cs
if (guildPlayer.CurrentTrack == null)
{
	await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no tracks loaded."));
	return;
}
```

And finally, we can call pause:

```cs
await guildPlayer.PauseAsync();
```

The equivalent for resume:

```cs
await guildPlayer.ResumeAsync();
```

The finished commands should look like so:
```cs
[SlashCommand("pause", "Pause a track")]
public async Task PauseAsync(InteractionContext ctx)
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

	if (guildPlayer.CurrentTrack == null)
	{
		await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no tracks loaded."));
		return;
	}

	await guildPlayer.PauseAsync();
	await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Playback paused!"));
}

[SlashCommand("resume", "Resume a track")]
public async Task ResumeAsync(InteractionContext ctx)
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

	if (guildPlayer.CurrentTrack == null)
	{
		await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no tracks loaded."));
		return;
	}

	await guildPlayer.ResumeAsync();
	await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Playback resumed!"));
}
```

Now we can use these commands to listen to music!

## Visual Example

<discord-messages>
	<discord-message profile="dcs">
		<discord-command slot="reply" profile="user" command="/join"></discord-command>
		Joined <discord-mention type="voice">Music</discord-mention>!
	</discord-message>
	<discord-message profile="dcs">
		<discord-command slot="reply" profile="user" command="/play"></discord-command>
		Now playing <a target="_blank" class="discord-link external" href="https://youtu.be/38-cJT320aw">https://youtu.be/38-cJT320aw</a>!
		<discord-embed
			slot="embeds"
			provider="YouTube"
            provider-url="https://www.youtube.com"
			author-name="Raon"
			author-url="https://www.youtube.com/channel/UCQn1FqrR2OCjSe6Nl4GlVHw"
			color="#FF0000"
			embed-title="Raon 라온 | ‘クネクネ (Wiggle Wiggle)’ M/V"
			video="38-cJT320aw"
			url="https://www.youtube.com/watch?v=38-cJT320aw"
			image="https://cdn.aitsys.dev/file/data/kcrwt6baxsmr32rjnrdg/PHID-FILE-2w72lbyg6lrbstqo3geh/38-cJT320aw.jpg"
		></discord-embed>
	</discord-message>
	<discord-message profile="dcs">
		<discord-command slot="reply" profile="user" command="/pause"></discord-command>
		Playback paused!
	</discord-message>
	<discord-message profile="dcs">
		<discord-command slot="reply" profile="user" command="/resume"></discord-command>
		Playback resumed!
	</discord-message>
	<discord-message profile="dcs">
		<discord-command slot="reply" profile="user" command="/leave"></discord-command>
		Left <discord-mention type="voice">Music</discord-mention>!
	</discord-message>
</discord-messages>
