using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Common.RegularExpressions;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using DisCatSharp.Lavalink.EventArgs;
using DisCatSharp.Lavalink.Models;
using DisCatSharp.Net.Abstractions;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Lavalink;

/// <summary>
///     Represents a guild player based on a <see cref="LavalinkPlayer" />.
/// </summary>
public sealed class LavalinkGuildPlayer
{
	/// <summary>
	///     Gets the list of tracks that have been played.
	/// </summary>
	private readonly List<LavalinkTrack> _playedTracks = [];

	/// <summary>
	///     Triggers when the voice state gets updated.
	/// </summary>
	private readonly AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs> _voiceStateUpdated;

	/// <summary>
	///     Gets the internal list of users.
	/// </summary>
	internal readonly Dictionary<ulong, DiscordUser> CurrentUsersInternal = [];

	/// <summary>
	///     Triggers when the player state is updated.
	/// </summary>
	internal readonly AsyncEvent<LavalinkGuildPlayer, LavalinkPlayerStateUpdateEventArgs> StateUpdatedEvent;

	/// <summary>
	///     Triggers when a track ends.
	/// </summary>
	internal readonly AsyncEvent<LavalinkGuildPlayer, LavalinkTrackEndedEventArgs> TrackEndedEvent;

	/// <summary>
	///     Triggers when a track throws an exception.
	/// </summary>
	internal readonly AsyncEvent<LavalinkGuildPlayer, LavalinkTrackExceptionEventArgs> TrackExceptionEvent;

	/// <summary>
	///     Triggers when a track starts.
	/// </summary>
	internal readonly AsyncEvent<LavalinkGuildPlayer, LavalinkTrackStartedEventArgs> TrackStartedEvent;

	/// <summary>
	///     Triggers when a track gets stuck.
	/// </summary>
	internal readonly AsyncEvent<LavalinkGuildPlayer, LavalinkTrackStuckEventArgs> TrackStuckEvent;

	/// <summary>
	///     Gets or sets the current track being played.
	/// </summary>
	private LavalinkTrack? _currentTrack;

	/// <summary>
	///     Gets whether this player is disposed.
	/// </summary>
	private bool _isDisposed;

	/// <summary>
	///     <see cref="TaskCompletionSource" /> for the queue.
	/// </summary>
	private TaskCompletionSource<bool>? _queueTsc = null;

	/// <summary>
	///     Gets or sets the repeat mode for the queue.
	/// </summary>
	private RepeatMode _repeatMode = RepeatMode.None;

	/// <summary>
	///     Constructs a new <see cref="LavalinkGuildPlayer" />.
	/// </summary>
	/// <param name="session">The session this guild player is registered on.</param>
	/// <param name="guildId">The guild id the player is registered for.</param>
	/// <param name="player">The registered lavalink player.</param>
	internal LavalinkGuildPlayer(LavalinkSession session, ulong guildId, LavalinkPlayer player)
	{
		this.Session = session;
		this.GuildId = guildId;
		this.Player = player;
		this._voiceStateUpdated = new("LAVALINK_PLAYER_VOICE_STATE_UPDATE", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this.TrackStartedEvent = new("LAVALINK_TRACK_STARTED", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this.TrackEndedEvent = new("LAVALINK_TRACK_ENDED", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this.TrackExceptionEvent = new("LAVALINK_TRACK_EXCEPTION", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this.TrackStuckEvent = new("LAVALINK_TRACK_STUCK", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this.StateUpdatedEvent = new("LAVALINK_PLAYER_STATE_UPDATED", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this.Discord.VoiceStateUpdated += async (sender, args) => await this._voiceStateUpdated.InvokeAsync(sender, args).ConfigureAwait(false);
		this.VoiceStateUpdated += this.OnVoiceStateUpdated;
		this.CurrentUsersInternal.Add(this.Discord.CurrentUser.Id, this.Discord.CurrentUser);
		this.TrackEnded += this.OnTrackEnded;
		if (this.QUEUE_SYSTEM_ENABLED)
			QueueInternal.TryAdd(guildId, new());
	}

	/// <summary>
	///     Gets the internal queue of tracks for each guild.
	/// </summary>
	internal static ConcurrentDictionary<ulong, LavalinkQueue<LavalinkTrack>> QueueInternal { get; } = new();

	/// <summary>
	///     Gets the queue.
	/// </summary>
	public IReadOnlyList<LavalinkTrack> Queue => [.. QueueInternal.GetOrAdd(this.GuildId, new LavalinkQueue<LavalinkTrack>())];

	/// <summary>
	///     Gets whether the built-in queue system is enabled.
	/// </summary>
	private bool QUEUE_SYSTEM_ENABLED
		=> this.Session.Config.EnableBuiltInQueueSystem;

	/// <summary>
	///     Gets the current guild id.
	/// </summary>
	public ulong GuildId { get; }

	/// <summary>
	///     Gets the current channel id.
	/// </summary>
	public ulong ChannelId { get; internal set; }

	/// <summary>
	///     Gets the session this player is attached to.
	/// </summary>
	public LavalinkSession Session { get; }

	/// <summary>
	///     Gets the guild this player is attached to.
	/// </summary>
	public DiscordGuild Guild
		=> this.Discord.Guilds.TryGetValue(this.GuildId, out var guild)
			? guild
			: null!;

	/// <summary>
	///     Gets the discord client.
	/// </summary>
	public DiscordClient Discord
		=> this.Session.Discord;

	/// <summary>
	///     Gets the current channel this player is in.
	/// </summary>
	public DiscordChannel Channel
		=> this.Guild.ChannelsInternal[this.ChannelId];

	/// <summary>
	///     Gets the lavalink player.
	/// </summary>
	public LavalinkPlayer Player { get; internal set; }

	/// <summary>
	///     Gets the current track.
	/// </summary>
	public LavalinkTrack? CurrentTrack
		=> this.Player.Track;

	/// <summary>
	///     Gets the current track position.
	/// </summary>
	public TimeSpan TrackPosition
		=> this.Player.PlayerState.Position;

	/// <summary>
	///     Gets the current ping.
	/// </summary>
	public int Ping
		=> this.Player.PlayerState.Ping;

	/// <summary>
	///     Gets a list of current user in the <see cref="Channel" />.
	/// </summary>
	public IReadOnlyList<DiscordUser> CurrentUsers
		=> [.. this.CurrentUsersInternal.Values];

	/// <summary>
	///     Gets whether this player is currently connected.
	/// </summary>
	public bool IsConnected => !Volatile.Read(ref this._isDisposed) && this.Channel != null!;

	/// <summary>
	///     Triggers when the voice state gets updated.
	/// </summary>
	internal event AsyncEventHandler<DiscordClient, VoiceStateUpdateEventArgs> VoiceStateUpdated
	{
		add => this._voiceStateUpdated.Register(value);
		remove => this._voiceStateUpdated.Unregister(value);
	}

	/// <summary>
	///     Triggers when a track started.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildPlayer, LavalinkTrackStartedEventArgs> TrackStarted
	{
		add => this.TrackStartedEvent.Register(value);
		remove => this.TrackStartedEvent.Unregister(value);
	}

	/// <summary>
	///     Triggers when a track ended.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildPlayer, LavalinkTrackEndedEventArgs> TrackEnded
	{
		add => this.TrackEndedEvent.Register(value);
		remove => this.TrackEndedEvent.Unregister(value);
	}

	/// <summary>
	///     Triggers when a track throws an exception.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildPlayer, LavalinkTrackExceptionEventArgs> TrackException
	{
		add => this.TrackExceptionEvent.Register(value);
		remove => this.TrackExceptionEvent.Unregister(value);
	}

	/// <summary>
	///     Triggers when a track gets stuck.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildPlayer, LavalinkTrackStuckEventArgs> TrackStuck
	{
		add => this.TrackStuckEvent.Register(value);
		remove => this.TrackStuckEvent.Unregister(value);
	}

	/// <summary>
	///     Triggers when a player state updated.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildPlayer, LavalinkPlayerStateUpdateEventArgs> StateUpdated
	{
		add => this.StateUpdatedEvent.Register(value);
		remove => this.StateUpdatedEvent.Unregister(value);
	}

	/// <summary>
	///     Fired when a track ended.
	///     <para>Used to control the built-in queue.</para>
	/// </summary>
	/// <param name="sender">The guild player.</param>
	/// <param name="args">The event args.</param>
	/// <returns></returns>
	private Task OnTrackEnded(LavalinkGuildPlayer sender, LavalinkTrackEndedEventArgs args)
	{
		if (this.QUEUE_SYSTEM_ENABLED)
		{
			this._queueTsc?.SetResult(true);
			this.PlayNextInQueue();
		}

		if (this.CurrentTrack?.Info.Identifier == args.Track.Info.Identifier)
			this.Player.Track = null;
		args.Handled = false;
		return Task.CompletedTask;
	}

	/// <summary>
	///     Decodes encoded <see cref="LavalinkTrack" />s.
	///     <para>Might not work with pre 4.0 tracks.</para>
	/// </summary>
	/// <param name="tracks">The tracks to decode.</param>
	/// <returns>A <see cref="List{T}" /> of decoded <see cref="LavalinkTrack" />s.</returns>
	public async Task<IReadOnlyList<LavalinkTrack>> DecodeTracksAsync(IEnumerable<string> tracks)
		=> await this.Session.DecodeTracksAsync(tracks).ConfigureAwait(false);

	/// <summary>
	///     Decodes an encoded <see cref="LavalinkTrack" />.
	///     <para>Might not work with some pre 4.0 tracks.</para>
	/// </summary>
	/// <param name="track">The track to decode.</param>
	/// <returns>The decoded <see cref="LavalinkTrack" />.</returns>
	public async Task<LavalinkTrack> DecodeTrackAsync(string track)
		=> await this.Session.DecodeTrackAsync(track).ConfigureAwait(false);

	/// <summary>
	///     Loads tracks by <paramref name="identifier" />.
	///     Returns a dynamic object you have to parse with (Type)Result.
	/// </summary>
	/// <param name="identifier">The identifier to load.</param>
	/// <returns>A track loading result.</returns>
	public async Task<LavalinkTrackLoadingResult> LoadTracksAsync(string identifier)
		=> await this.Session.LoadTracksAsync(identifier).ConfigureAwait(false);

	/// <summary>
	///     Loads tracks by <paramref name="identifier" />.
	///     Returns a dynamic object you have to parse with (Type)Result.
	/// </summary>
	/// <param name="searchType">The search type to use. Some types need additional setup.</param>
	/// <param name="identifier">The identifier to load.</param>
	/// <returns>A track loading result.</returns>
	public async Task<LavalinkTrackLoadingResult> LoadTracksAsync(LavalinkSearchType searchType, string identifier)
		=> await this.Session.LoadTracksAsync(searchType, identifier).ConfigureAwait(false);

	/// <summary>
	///     Gets the lyrics for the currently playing track.
	/// </summary>
	/// <param name="skipTrackSource">Whether to skip the current track source and fetch from highest priority source.</param>
	/// <returns>The <see cref="LavalinkLyricsResult" /> or <see langword="null" />.</returns>
	public async Task<LavalinkLyricsResult?> GetLyricsAsync(bool skipTrackSource = false)
		=> await this.Session.Rest.GetLyricsForCurrentTrackAsync(this.Session.Config.SessionId!, this.GuildId, skipTrackSource).ConfigureAwait(false);

	/// <summary>
	///     Updates the <see cref="LavalinkPlayer" />.
	/// </summary>
	/// <param name="action">The action to perform on the player.</param>
	/// <returns>The updated guild player.</returns>
	/// <exception cref="InvalidOperationException">Thrown when both <c>EncodedTrack</c> and <c>Identifier</c> are set.</exception>
	public async Task<LavalinkGuildPlayer> UpdateAsync(Action<LavalinkPlayerUpdateModel> action)
	{
		var mdl = new LavalinkPlayerUpdateModel();
		action(mdl);

		if (mdl.EncodedTrack.HasValue && !string.IsNullOrEmpty(mdl.EncodedTrack.Value) && mdl.Identifier.HasValue)
			throw new InvalidOperationException($"Cannot set both {nameof(mdl.EncodedTrack)} & {mdl.Identifier}. Only one at a time.");

		this.Player = await this.Session.Rest.UpdatePlayerAsync(
			this.Session.Config.SessionId!, this.GuildId, !mdl.Replace,
			mdl.EncodedTrack, mdl.Identifier,
			mdl.Position, mdl.EndTime,
			mdl.Volume, mdl.Paused,
			mdl.Filters,
			mdl.UserData
		).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Plays a song partially by its identifier with a start and end-time.
	/// </summary>
	/// <param name="identifier">The identifier to play.</param>
	/// <param name="startTime">The start time.</param>
	/// <param name="endTime">The end time.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PlayPartialAsync(string identifier, TimeSpan startTime, TimeSpan endTime)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, identifier: identifier, position: (int)startTime.TotalMilliseconds,
			endTime: (int)endTime.TotalMilliseconds).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Plays a track partially with a start and end-time.
	/// </summary>
	/// <param name="track">The track to play.</param>
	/// <param name="startTime">The start time.</param>
	/// <param name="endTime">The end time.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PlayPartialAsync(LavalinkTrack track, TimeSpan startTime, TimeSpan endTime)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, identifier: track.Info.Uri.ToString(),
			position: (int)startTime.TotalMilliseconds, endTime: (int)endTime.TotalMilliseconds).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Plays a song by its encoded track string with a start and end-time.
	/// </summary>
	/// <param name="encodedTrack">The encoded track to play.</param>
	/// <param name="startTime">The start time.</param>
	/// <param name="endTime">The end time.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PlayPartialEncodedAsync(string encodedTrack, TimeSpan startTime, TimeSpan endTime)
	{
		this.Player = await this.Session.Rest
			.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, false, encodedTrack, position: (int)startTime.TotalMilliseconds, endTime: (int)endTime.TotalMilliseconds)
			.ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Plays a song by its identifier.
	/// </summary>
	/// <param name="identifier">The identifier to play.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PlayAsync(string identifier)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, identifier: identifier).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Plays a track.
	/// </summary>
	/// <param name="track">The track to play.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PlayAsync(LavalinkTrack track)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, identifier: track.Info.Uri.ToString()).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Plays a song by its encoded track string.
	/// </summary>
	/// <param name="encodedTrack">The encoded track to play.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PlayEncodedAsync(string encodedTrack)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, false, encodedTrack).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Seeks the player to the position in <paramref name="seconds" />.
	/// </summary>
	/// <param name="seconds">The seconds to seek to.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> SeekAsync(int seconds)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, true, position: seconds * 1000).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Seeks the player to the position.
	/// </summary>
	/// <param name="position">The position to seek to.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> SeekAsync(TimeSpan position)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, true, position: (int)position.TotalMilliseconds).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Modifies the volume of the player.
	/// </summary>
	/// <param name="volume">The volume of the player, range <c>0</c>-<c>1000</c>, in percentage.</param>
	/// <returns>The updated guild player.</returns>
	/// <exception cref="ArgumentException">Thrown when the <paramref name="volume" /> is out of range.</exception>
	public async Task<LavalinkGuildPlayer> SetVolumeAsync(int volume)
	{
		if (volume is < 0 or > 1000)
			throw new ArgumentException("Volume can only be between 0 and 1000", nameof(volume));

		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, true, volume: volume).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Pauses the player.
	/// </summary>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PauseAsync()
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, true, paused: true).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Resumes the player.
	/// </summary>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> ResumeAsync()
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, true, paused: false).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Stops the player.
	/// </summary>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> StopAsync()
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, false, null).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Skips the current track.
	/// </summary>
	/// <returns>The updated guild player.</returns>
	public Task<LavalinkGuildPlayer> SkipAsync()
		=> this.StopAsync();

	/// <summary>
	///     Directly plays a song by url.
	/// </summary>
	/// <param name="url">The url to play.</param>
	/// <returns></returns>
	/// <exception cref="NotSupportedException">Thrown when the <paramref name="url" /> is a pure youtube playlist.</exception>
	public async Task<LavalinkGuildPlayer> PlayDirectUrlAsync(string url)
	{
		if (CommonRegEx.AdvancedYoutubeRegex().IsMatch(url))
		{
			if (url.Contains("playlist"))
				throw new NotSupportedException("Lavalink is unable to play a playlist directly.");

			var match = CommonRegEx.AdvancedYoutubeRegex().Match(url);
			if (match.Groups["list"] != null! && !string.IsNullOrEmpty(match.Groups["list"].Value))
			{
				this.Session.Discord.Logger.LogTrace(LavalinkEvents.Misc, null, "Removed list from playlist url. Not supported");
				url = url.Replace($"list={match.Groups["list"].Value}", null);
			}

			if (match.Groups["index"] != null! && !string.IsNullOrEmpty(match.Groups["index"].Value))
			{
				this.Session.Discord.Logger.LogTrace(LavalinkEvents.Misc, null, "Removed index from playlist url. Not supported");
				url = url.Replace($"index={match.Groups["index"].Value}", null);
			}
		}

		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, identifier: url).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///     Switches the player to a new channel.
	/// </summary>
	/// <param name="channel">The new channel to switch to.</param>
	/// <param name="deafened">Whether to join deafened.</param>
	/// <exception cref="ArgumentException">Thrown when the target channel is not of the base type voice.</exception>
	public async Task SwitchChannelAsync(DiscordChannel channel, bool deafened = true)
	{
		if (channel.Type != ChannelType.Stage && channel.Type != ChannelType.Voice)
			throw new ArgumentException("Cannot switch to a non-voice channel", nameof(channel));

		this.ChannelId = channel.Id;
		var vsd = new DiscordDispatchPayload
		{
			OpCode = 4,
			Payload = new VoiceStateUpdatePayload
			{
				GuildId = this.GuildId,
				ChannelId = this.ChannelId,
				Deafened = deafened,
				Muted = false
			}
		};
		await this.Session.Discord.WsSendAsync(LavalinkJson.SerializeObject(vsd)).ConfigureAwait(false);
	}

	/// <summary>
	///     Updates the player state.
	/// </summary>
	/// <param name="state">The player state to update with.</param>
	internal void UpdatePlayerState(LavalinkPlayerState state)
		=> this.Player.PlayerState = state;

	/// <summary>
	///     Updates the voice state.
	/// </summary>
	/// <param name="voiceState">The voice state to update with.</param>
	internal void UpdateVoiceState(LavalinkVoiceState voiceState)
		=> this.Player.VoiceState = voiceState;

	/// <summary>
	///     Disconnect the guild player from the channel.
	/// </summary>
	public async Task DisconnectAsync()
	{
		await this.Session.Rest.DestroyPlayerAsync(this.Session.Config.SessionId!, this.GuildId).ConfigureAwait(false);
		await this.DisconnectVoiceAsync().ConfigureAwait(false);
		this.CurrentUsersInternal.Clear();
		Volatile.Write(ref this._isDisposed, true);
		await this.Session.GuildPlayerDestroyedEvent.InvokeAsync(this.Session, new(this)).ConfigureAwait(false);
	}

	/// <summary>
	///     Informs discord to perform a voice disconnect.
	/// </summary>
	/// <returns></returns>
	internal async Task DisconnectVoiceAsync()
	{
		var vsd = new DiscordDispatchPayload
		{
			OpCode = 4,
			Payload = new VoiceStateUpdatePayload
			{
				GuildId = this.GuildId,
				ChannelId = null,
				Deafened = false,
				Muted = false
			}
		};
		await this.Session.Discord.WsSendAsync(LavalinkJson.SerializeObject(vsd)).ConfigureAwait(false);
	}

	/// <summary>
	///     Handles the <see cref="DiscordClient.VoiceStateUpdated" /> event.
	/// </summary>
	/// <param name="sender">The discord client.</param>
	/// <param name="args">The event args.</param>
	private Task OnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs args)
	{
		if (args.User.Id == this.Discord.CurrentUser.Id)
			return Task.CompletedTask;

		if (args.Channel?.Id != this.ChannelId)
			return Task.CompletedTask;

		if (args.After.ChannelId is not null && args.After.ChannelId == this.ChannelId)
		{
			this.CurrentUsersInternal.TryAdd(args.User.Id, args.User);
			return Task.CompletedTask;
		}

		if (args.Before.ChannelId is null || args.Before.ChannelId!.Value != this.ChannelId || (args.After.ChannelId is null && args.After.ChannelId!.Value == this.ChannelId))
			return Task.CompletedTask;

		this.CurrentUsersInternal.Remove(args.User.Id);
		return Task.CompletedTask;
	}

#region Queue Operations

	/// <summary>
	///     Adds a Lavalink track to the queue.
	/// </summary>
	/// <param name="track">The track to add.</param>
	public void AddToQueue(LavalinkTrack track)
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		QueueInternal.GetOrAdd(this.GuildId, new LavalinkQueue<LavalinkTrack>()).Enqueue(track);
	}

	/// <summary>
	///     Adds all tracks from a Lavalink playlist to the queue.
	/// </summary>
	/// <param name="playlist">The <see cref="LavalinkPlaylist" /> containing the tracks to add.</param>
	public void AddToQueue(LavalinkPlaylist playlist)
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		foreach (var track in playlist.Tracks)
			this.AddToQueue(track);
	}

	/// <summary>
	///     Inserts a track at the specified index in the queue.
	/// </summary>
	/// <param name="index">The zero-based index at which the track should be inserted.</param>
	/// <param name="track">The track to insert.</param>
	public void AddToQueueAt(int index, LavalinkTrack track)
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		if (QueueInternal.TryGetValue(this.GuildId, out var queue))
			queue.InsertAt(index, track);
	}

	/// <summary>
	///     Inserts a range of tracks at the specified index in the queue.
	/// </summary>
	/// <param name="index">The zero-based index at which the tracks should be inserted.</param>
	/// <param name="tracks">The tracks to insert.</param>
	public void AddToQueueAt(int index, IEnumerable<LavalinkTrack> tracks)
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		if (QueueInternal.TryGetValue(this.GuildId, out var queue))
			queue.InsertRange(index, tracks);
	}

	/// <summary>
	///     Attempts to retrieve the next track in the queue without removing it.
	/// </summary>
	/// <param name="track">The next track in the queue, or null if the queue is empty.</param>
	/// <returns>True if a track was found, false otherwise.</returns>
	public bool TryPeekQueue([NotNullWhen(true)] out LavalinkTrack? track)
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
		{
			track = null;
			return false;
		}

		if (QueueInternal.TryGetValue(this.GuildId, out var queue) && queue.TryPeek(out var result))
		{
			track = result;
			return true;
		}

		track = null;
		return false;
	}

	/// <summary>
	///     Shuffles the tracks in the queue.
	/// </summary>
	public void ShuffleQueue()
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		if (QueueInternal.TryGetValue(this.GuildId, out var queue))
			queue.Shuffle();
	}

	/// <summary>
	///     Reverses the order of the tracks in the queue.
	/// </summary>
	public void ReverseQueue()
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		if (QueueInternal.TryGetValue(this.GuildId, out var queue))
			queue.Reverse();
	}

	/// <summary>
	///     Removes the specified track from the queue.
	/// </summary>
	/// <param name="track">The track to remove.</param>
	public void RemoveFromQueue(LavalinkTrack track)
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		if (QueueInternal.TryGetValue(this.GuildId, out var queue))
			queue.Remove(track);
	}

	/// <summary>
	///     Removes the track with the specified title (identifier) from the queue.
	/// </summary>
	/// <param name="identifier">The title (identifier) of the track to remove.</param>
	public void RemoveFromQueue(string identifier)
	{
		if (!this.QUEUE_SYSTEM_ENABLED || !QueueInternal.TryGetValue(this.GuildId, out var queue))
			return;

		var trackToRemove = queue.FirstOrDefault(x => x.Info.Title == identifier);
		if (trackToRemove != null)
			queue.Remove(trackToRemove);
	}

	/// <summary>
	///     Removes the track at the specified index from the queue.
	/// </summary>
	/// <param name="index">The zero-based index of the track to remove.</param>
	public void RemoveFromQueueAt(int index)
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		if (QueueInternal.TryGetValue(this.GuildId, out var queue))
			queue.RemoveAt(index);
	}

	/// <summary>
	///     Removes a range of tracks from the queue.
	/// </summary>
	/// <param name="index">The zero-based index of the first track to remove.</param>
	/// <param name="count">The number of tracks to remove.</param>
	public void RemoveFromQueueAtRange(int index, int count)
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		if (QueueInternal.TryGetValue(this.GuildId, out var queue))
			queue.RemoveRange(index, count);
	}

	/// <summary>
	///     Clears all tracks from the queue.
	/// </summary>
	public void ClearQueue()
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		if (QueueInternal.TryGetValue(this.GuildId, out var queue))
			queue.Clear();
	}

	/// <summary>
	///     Sets the repeat mode for the queue.
	/// </summary>
	/// <param name="mode">The repeat mode to set.</param>
	public void SetRepeatMode(RepeatMode mode)
		=> this._repeatMode = mode;

	/// <summary>
	///     Starts playing the next track in the queue.
	/// </summary>
	public void PlayQueue()
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		this.PlayNextInQueue();
	}

	/// <summary>
	///     Plays the next track in the queue.
	/// </summary>
	private async void PlayNextInQueue()
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		while (this._queueTsc is not null)
			await Task.Delay(TimeSpan.FromSeconds(1));

		var queue = QueueInternal.GetOrAdd(this.GuildId, new LavalinkQueue<LavalinkTrack>());
		LavalinkTrack? nextTrack;

		if (this._repeatMode == RepeatMode.Current && this._currentTrack is not null)
			nextTrack = this._currentTrack;
		else if (queue.TryDequeue(out nextTrack))
			this._currentTrack = nextTrack;
		else if (this._repeatMode == RepeatMode.All)
		{
			foreach (var track in this._playedTracks)
				queue.Enqueue(track);

			this._playedTracks.Clear();

			if (queue.TryDequeue(out nextTrack))
				this._currentTrack = nextTrack;
		}

		if (nextTrack is not null)
		{
			this._queueTsc = new();

			var queueEntry = this.Session.Config.QueueEntry.AddTrack(nextTrack);

			if (await queueEntry.BeforePlayingAsync(this).ConfigureAwait(false))
			{
				await this.PlayAsync(queueEntry.Track).ConfigureAwait(false);

				await this._queueTsc.Task.ConfigureAwait(false);
				this._queueTsc = null!;

				await queueEntry.AfterPlayingAsync(this).ConfigureAwait(false);
			}

			if (this._repeatMode == RepeatMode.All)
				this._playedTracks.Add(nextTrack);
		}
	}

#endregion
}
