using System;
using System.Collections.Generic;
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
using DisCatSharp.Lavalink.Payloads;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Lavalink;

/// <summary>
/// Represents a guild player based on a <see cref="LavalinkPlayer"/>.
/// </summary>
public sealed class LavalinkGuildPlayer
{
	/// <summary>
	/// Triggers when the voice state gets updated.
	/// </summary>
	internal event AsyncEventHandler<DiscordClient, VoiceStateUpdateEventArgs> VoiceStateUpdated
	{
		add => this._voiceStateUpdated.Register(value);
		remove => this._voiceStateUpdated.Unregister(value);
	}

	private readonly AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs> _voiceStateUpdated;

	/// <summary>
	/// Triggers when a track started.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildPlayer, LavalinkTrackStartedEventArgs> TrackStarted
	{
		add => this.TrackStartedEvent.Register(value);
		remove => this.TrackStartedEvent.Unregister(value);
	}

	internal readonly AsyncEvent<LavalinkGuildPlayer, LavalinkTrackStartedEventArgs> TrackStartedEvent;

	/// <summary>
	/// Triggers when a track ended.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildPlayer, LavalinkTrackEndedEventArgs> TrackEnded
	{
		add => this.TrackEndedEvent.Register(value);
		remove => this.TrackEndedEvent.Unregister(value);
	}

	internal readonly AsyncEvent<LavalinkGuildPlayer, LavalinkTrackEndedEventArgs> TrackEndedEvent;

	/// <summary>
	/// Triggers when a track throws an exception.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildPlayer, LavalinkTrackExceptionEventArgs> TrackException
	{
		add => this.TrackExceptionEvent.Register(value);
		remove => this.TrackExceptionEvent.Unregister(value);
	}

	internal readonly AsyncEvent<LavalinkGuildPlayer, LavalinkTrackExceptionEventArgs> TrackExceptionEvent;

	/// <summary>
	/// Triggers when a track gets stuck.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildPlayer, LavalinkTrackStuckEventArgs> TrackStuck
	{
		add => this.TrackStuckEvent.Register(value);
		remove => this.TrackStuckEvent.Unregister(value);
	}

	internal readonly AsyncEvent<LavalinkGuildPlayer, LavalinkTrackStuckEventArgs> TrackStuckEvent;

	/// <summary>
	/// Triggers when a player state updated.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildPlayer, LavalinkPlayerStateUpdateEventArgs> StateUpdated
	{
		add => this.StateUpdatedEvent.Register(value);
		remove => this.StateUpdatedEvent.Unregister(value);
	}

	internal readonly AsyncEvent<LavalinkGuildPlayer, LavalinkPlayerStateUpdateEventArgs> StateUpdatedEvent;

	/// <summary>
	/// <see cref="TaskCompletionSource"/> for the queue.
	/// </summary>
	private TaskCompletionSource<bool>? _queueTsc = null;

	/// <summary>
	/// Gets whether the built-in queue system is enabled.
	/// </summary>
	private bool QUEUE_SYSTEM_ENABLED
		=> this.Session.Config.EnableBuiltInQueueSystem;

	/// <summary>
	/// Gets the current guild id.
	/// </summary>
	public ulong GuildId { get; }

	/// <summary>
	/// Gets the current channel id.
	/// </summary>
	public ulong ChannelId { get; internal set; }

	/// <summary>
	/// Gets the session this player is attached to.
	/// </summary>
	public LavalinkSession Session { get; }

	/// <summary>
	/// Gets the guild this player is attached to.
	/// </summary>
	public DiscordGuild Guild
		=> this.Discord.Guilds.TryGetValue(this.GuildId, out var guild)
			? guild
			: null!;

	/// <summary>
	/// Gets the discord client.
	/// </summary>
	public DiscordClient Discord
		=> this.Session.Discord;

	/// <summary>
	/// Gets the current channel this player is in.
	/// </summary>
	public DiscordChannel Channel
		=> this.Guild.ChannelsInternal[this.ChannelId];

	/// <summary>
	/// Gets the lavalink player.
	/// </summary>
	public LavalinkPlayer Player { get; internal set; }

	/// <summary>
	/// Gets the current track.
	/// </summary>
	public LavalinkTrack? CurrentTrack
		=> this.Player.Track;

	/// <summary>
	/// Gets the current track position.
	/// </summary>
	public TimeSpan TrackPosition
		=> this.Player.PlayerState.Position;

	/// <summary>
	/// Gets the current ping.
	/// </summary>
	public int Ping
		=> this.Player.PlayerState.Ping;

	/// <summary>
	/// Gets the queue entries.
	/// </summary>
	public IReadOnlyList<IQueueEntry> QueueEntries
		=> this._queueEntriesInternal.Values.ToList();

	/// <summary>
	/// Gets the internal queue entries.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "<Pending>")]
	private SortedList<string, IQueueEntry> _queueEntriesInternal = [];

	/// <summary>
	/// Gets a list of current user in the <see cref="Channel"/>.
	/// </summary>
	public IReadOnlyList<DiscordUser> CurrentUsers
		=> new List<DiscordUser>(this.CurrentUsersInternal.Values);

	/// <summary>
	/// Gets the internal list of users.
	/// </summary>
	internal readonly Dictionary<ulong, DiscordUser> CurrentUsersInternal = [];

	/// <summary>
	/// Gets whether this player is currently connected.
	/// </summary>
	public bool IsConnected => !Volatile.Read(ref this._isDisposed) && this.Channel != null!;

	/// <summary>
	/// Gets whether this player is disposed.
	/// </summary>
	private bool _isDisposed;

	/// <summary>
	/// Constructs a new <see cref="LavalinkGuildPlayer"/>.
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
	}

	/// <summary>
	/// Fired when a track ended.
	/// <para>Used to control the built-in queue.</para>
	/// </summary>
	/// <param name="sender">The guild player.</param>
	/// <param name="args">The event args.</param>
	/// <returns></returns>
	private Task OnTrackEnded(LavalinkGuildPlayer sender, LavalinkTrackEndedEventArgs args)
	{
		if (this.QUEUE_SYSTEM_ENABLED && this._queueTsc != null)
			this._queueTsc.SetResult(true);
		if (this.CurrentTrack?.Info.Identifier == args.Track.Info.Identifier)
			this.Player.Track = null;
		args.Handled = false;
		return Task.CompletedTask;
	}

	/// <summary>
	/// Decodes encoded <see cref="LavalinkTrack"/>s.
	/// <para>Might not work with pre 4.0 tracks.</para>
	/// </summary>
	/// <param name="tracks">The tracks to decode.</param>
	/// <returns>A <see cref="List{T}"/> of decoded <see cref="LavalinkTrack"/>s.</returns>
	public async Task<IReadOnlyList<LavalinkTrack>> DecodeTracksAsync(IEnumerable<string> tracks)
		=> await this.Session.DecodeTracksAsync(tracks).ConfigureAwait(false);

	/// <summary>
	/// Decodes an encoded <see cref="LavalinkTrack"/>.
	/// <para>Might not work with some pre 4.0 tracks.</para>
	/// </summary>
	/// <param name="track">The track to decode.</param>
	/// <returns>The decoded <see cref="LavalinkTrack"/>.</returns>
	public async Task<LavalinkTrack> DecodeTrackAsync(string track)
		=> await this.Session.DecodeTrackAsync(track).ConfigureAwait(false);

	/// <summary>
	/// Loads tracks by <paramref name="identifier"/>.
	/// Returns a dynamic object you have to parse with (Type)Result.
	/// </summary>
	/// <param name="identifier">The identifier to load.</param>
	/// <returns>A track loading result.</returns>
	public async Task<LavalinkTrackLoadingResult> LoadTracksAsync(string identifier)
		=> await this.Session.LoadTracksAsync(identifier).ConfigureAwait(false);

	/// <summary>
	/// Loads tracks by <paramref name="identifier"/>.
	/// Returns a dynamic object you have to parse with (Type)Result.
	/// </summary>
	/// <param name="searchType">The search type to use. Some types need additional setup.</param>
	/// <param name="identifier">The identifier to load.</param>
	/// <returns>A track loading result.</returns>
	public async Task<LavalinkTrackLoadingResult> LoadTracksAsync(LavalinkSearchType searchType, string identifier)
		=> await this.Session.LoadTracksAsync(searchType, identifier).ConfigureAwait(false);

	/// <summary>
	/// Updates the <see cref="LavalinkPlayer"/>.
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
			mdl.Filters
		).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	/// Plays a song partially by its identifier with a start and end-time.
	/// </summary>
	/// <param name="identifier">The identifier to play.</param>
	/// <param name="startTime">The start time.</param>
	/// <param name="endTime">The end time.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PlayPartialAsync(string identifier, TimeSpan startTime, TimeSpan endTime)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, false, identifier: identifier, position: (int)startTime.TotalMilliseconds,
			endTime: (int)endTime.TotalMilliseconds).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	/// Plays a track partially with a start and end-time.
	/// </summary>
	/// <param name="track">The track to play.</param>
	/// <param name="startTime">The start time.</param>
	/// <param name="endTime">The end time.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PlayPartialAsync(LavalinkTrack track, TimeSpan startTime, TimeSpan endTime)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, false, identifier: track.Info.Uri.ToString(),
			position: (int)startTime.TotalMilliseconds, endTime: (int)endTime.TotalMilliseconds).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	/// Plays a song by its encoded track string with a start and end-time.
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
	/// Plays a song by its identifier.
	/// </summary>
	/// <param name="identifier">The identifier to play.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PlayAsync(string identifier)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, false, identifier: identifier).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	/// Plays a track.
	/// </summary>
	/// <param name="track">The track to play.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PlayAsync(LavalinkTrack track)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, false, identifier: track.Info.Uri.ToString()).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	/// Plays a song by its encoded track string.
	/// </summary>
	/// <param name="encodedTrack">The encoded track to play.</param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PlayEncodedAsync(string encodedTrack)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, false, encodedTrack).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="position"></param>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> SeekAsync(TimeSpan position)
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, true, position: (int)position.TotalMilliseconds).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	/// Modifies the volume of the player.
	/// </summary>
	/// <param name="volume">The volume of the player, range <c>0</c>-<c>1000</c>, in percentage.</param>
	/// <returns>The updated guild player.</returns>
	/// <exception cref="ArgumentException">Thrown when the <paramref name="volume"/> is out of range.</exception>
	public async Task<LavalinkGuildPlayer> SetVolumeAsync(int volume)
	{
		if (volume is < 0 or > 1000)
			throw new ArgumentException("Volume can only be between 0 and 1000", nameof(volume));

		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, true, volume: volume).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	/// Pauses the player.
	/// </summary>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> PauseAsync()
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, true, paused: true).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	/// Resumes the player.
	/// </summary>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> ResumeAsync()
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, true, paused: false).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	/// Stops the player.
	/// </summary>
	/// <returns>The updated guild player.</returns>
	public async Task<LavalinkGuildPlayer> StopAsync()
	{
		this.Player = await this.Session.Rest.UpdatePlayerAsync(this.Session.Config.SessionId!, this.GuildId, false, null).ConfigureAwait(false);
		return this;
	}

	/// <summary>
	/// Directly plays a song by url.
	/// </summary>
	/// <param name="url">The url to play.</param>
	/// <returns></returns>
	/// <exception cref="NotSupportedException">Thrown when the <paramref name="url"/> is a pure youtube playlist.</exception>
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

#region Queue Operations

	/// <summary>
	/// Adds a <see cref="LavalinkTrack"/> to the queue.
	/// </summary>
	/// <typeparam name="T">Queue entry object type.</typeparam>
	/// <param name="entry">The entry to add. Please construct a new() entry for every track.</param>
	/// <param name="track">The track to attach.</param>
	public void AddToQueue<T>(T entry, LavalinkTrack track) where T : IQueueEntry
		=> this._queueEntriesInternal.Add(track.Info.Uri.ToString(), entry.AddTrack(track));

	/// <summary>
	/// Removes a queue entry.
	/// </summary>
	/// <param name="entry">The entry to remove.</param>
	/// <returns><see langword="true"/> if the entry was found and removed.</returns>
	public bool RemoveFromQueue(IQueueEntry entry)
		=> this._queueEntriesInternal.Remove(entry.Track.Info.Uri.ToString());

	/// <summary>
	/// Removes a queue entry by the track identifier.
	/// </summary>
	/// <param name="identifier">The identifier to look up.</param>
	/// <returns><see langword="true"/> if the entry was found and removed.</returns>
	public bool RemoveFromQueueByIdentifierAsync(string identifier)
		=> this._queueEntriesInternal.Count != 0 && this._queueEntriesInternal!.TryGetValue(identifier, out var entry) && this.RemoveFromQueue(entry!);

	/// <summary>
	/// Plays the queue while entries are presented.
	/// </summary>
	public async void PlayQueueAsync()
	{
		if (!this.QUEUE_SYSTEM_ENABLED)
			return;

		while (this._queueEntriesInternal.Count > 0)
		{
			this._queueTsc = new();
			var currentQueueEntry = this._queueEntriesInternal.First();

			if (await currentQueueEntry.Value.BeforePlayingAsync(this).ConfigureAwait(false))
			{
				await this.PlayAsync(currentQueueEntry.Value.Track).ConfigureAwait(false);

				await this._queueTsc.Task.ConfigureAwait(false);

				await currentQueueEntry.Value.AfterPlayingAsync(this).ConfigureAwait(false);
			}

			this._queueEntriesInternal.RemoveAt(0);
			this._queueTsc = null!;
		}
	}

#endregion

	/// <summary>
	/// Switches the player to a new channel.
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
			Payload = new VoiceStateUpdatePayload()
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
	/// Updates the player state.
	/// </summary>
	/// <param name="state">The player state to update with.</param>
	internal void UpdatePlayerState(LavalinkPlayerState state)
		=> this.Player.PlayerState = state;

	/// <summary>
	/// Updates the voice state.
	/// </summary>
	/// <param name="voiceState">The voice state to update with.</param>
	internal void UpdateVoiceState(LavalinkVoiceState voiceState)
		=> this.Player.VoiceState = voiceState;

	/// <summary>
	/// Disconnect the guild player from the channel.
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
	/// Informs discord to perform a voice disconnect.
	/// </summary>
	/// <returns></returns>
	internal async Task DisconnectVoiceAsync()
	{
		var vsd = new DiscordDispatchPayload
		{
			OpCode = 4,
			Payload = new VoiceStateUpdatePayload()
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
	/// Handles the <see cref="DiscordClient.VoiceStateUpdated"/> event.
	/// </summary>
	/// <param name="sender">The discord client.</param>
	/// <param name="args">The event args.</param>
	private Task OnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs args)
	{
		if (args.User.Id == this.Discord.CurrentUser.Id)
			return Task.CompletedTask;

		if (args.Channel?.Id != this.ChannelId)
			return Task.CompletedTask;

		if (args.After.ChannelId != null && args.After.ChannelId == this.ChannelId)
		{
			this.CurrentUsersInternal.TryAdd(args.User.Id, args.User);
			return Task.CompletedTask;
		}

		if (args.Before.ChannelId != null && args.Before.ChannelId!.Value == this.ChannelId && (args.After.ChannelId != null || args.After.ChannelId!.Value != this.ChannelId))
		{
			this.CurrentUsersInternal.Remove(args.User.Id);
			return Task.CompletedTask;
		}

		return Task.CompletedTask;
	}
}
