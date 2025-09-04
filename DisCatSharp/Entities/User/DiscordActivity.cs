using System;
using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Net.Abstractions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents user status.
/// </summary>
[JsonConverter(typeof(UserStatusConverter))]
public enum UserStatus
{
	/// <summary>
	///     User is offline.
	/// </summary>
	Offline = 0,

	/// <summary>
	///     User is online.
	/// </summary>
	Online = 1,

	/// <summary>
	///     User is idle.
	/// </summary>
	Idle = 2,

	/// <summary>
	///     User asked not to be disturbed.
	/// </summary>
	DoNotDisturb = 4,

	/// <summary>
	///     User is invisible. They will appear as Offline to anyone but themselves.
	/// </summary>
	Invisible = 5,

	/// <summary>
	///     User is streaming.
	/// </summary>
	Streaming = 6
}

/// <summary>
///     Represents a user status converter.
/// </summary>
internal sealed class UserStatusConverter : JsonConverter
{
	/// <summary>
	///     Writes the json.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The serializer.</param>
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value is UserStatus status)
			switch (status) // reader.Value can be a string, DateTime or DateTimeOffset (yes, it's weird)
			{
				case UserStatus.Online:
					writer.WriteValue("online");
					return;

				case UserStatus.Idle:
					writer.WriteValue("idle");
					return;

				case UserStatus.DoNotDisturb:
					writer.WriteValue("dnd");
					return;

				case UserStatus.Invisible:
					writer.WriteValue("invisible");
					return;

				case UserStatus.Streaming:
					writer.WriteValue("streaming");
					return;

				case UserStatus.Offline:
				default:
					writer.WriteValue("offline");
					return;
			}
	}

	/// <summary>
	///     Reads the json.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="objectType">The object type.</param>
	/// <param name="existingValue">The existing value.</param>
	/// <param name="serializer">The serializer.</param>
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
		// Active sessions are indicated with an "online", "idle", or "dnd" string per platform. If a user is
		// offline or invisible, the corresponding field is not present.
		reader.Value?.ToString().ToLowerInvariant() switch // reader.Value can be a string, DateTime or DateTimeOffset (yes, it's weird)
		{
			"online" => UserStatus.Online,
			"idle" => UserStatus.Idle,
			"dnd" => UserStatus.DoNotDisturb,
			"invisible" => UserStatus.Invisible,
			"streaming" => UserStatus.Streaming,
			_ => UserStatus.Offline
		};

	/// <summary>
	///     Whether this user5 status can be converted.
	/// </summary>
	/// <param name="objectType">The object type.</param>
	/// <returns>A bool.</returns>
	public override bool CanConvert(Type objectType) => objectType == typeof(UserStatus);
}

/// <summary>
///     Represents a game that a user is playing.
/// </summary>
public sealed class DiscordActivity
{
	/// <summary>
	///     Creates a new, empty instance of a <see cref="DiscordActivity" />.
	/// </summary>
	public DiscordActivity()
	{
		this.ActivityType = ActivityType.Playing;
	}

	/// <summary>
	///     Creates a new instance of a <see cref="DiscordActivity" /> with specified name.
	/// </summary>
	/// <param name="name">Name of the activity.</param>
	public DiscordActivity(string name)
	{
		this.Name = name;
		this.ActivityType = ActivityType.Playing;
	}

	/// <summary>
	///     Creates a new instance of a <see cref="DiscordActivity" /> with specified name.
	/// </summary>
	/// <param name="name">Name of the activity.</param>
	/// <param name="type">Type of the activity.</param>
	public DiscordActivity(string name, ActivityType type)
	{
		if (type == ActivityType.Custom)
		{
			this.Id = "custom";
			this.Name = "Custom Status";
			this.State = name;
		}
		else
			this.Name = name;

		this.ActivityType = type;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordActivity" /> class.
	/// </summary>
	/// <param name="rawActivity">The raw activity.</param>
	internal DiscordActivity(TransportActivity rawActivity)
	{
		this.UpdateWith(rawActivity);
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordActivity" /> class.
	/// </summary>
	/// <param name="other">The other.</param>
	internal DiscordActivity(DiscordActivity other)
	{
		this.Id = other.Id;
		this.Name = other.Name;
		this.State = other.State;
		this.ActivityType = other.ActivityType;
		this.StreamUrl = other.StreamUrl;
		this.SessionId = other.SessionId;
		this.SyncId = other.SyncId;
		this.Platform = other.Platform;
		this.RichPresence = new(other.RichPresence);
		this.CustomStatus = new(other.CustomStatus);
	}

	/// <summary>
	///     Gets or sets the partial emoji.
	/// </summary>
	public PartialEmoji? Emoji { get; set; }

	/// <summary>
	///     Gets or sets the id of user's activity.
	/// </summary>
	public string? Id { get; set; }

	/// <summary>
	///     Gets or sets the name of user's activity.
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	///     Gets or sets the state of user's activity.
	/// </summary>
	public string? State { get; set; }

	/// <summary>
	///     Gets or sets the stream URL, if applicable.
	/// </summary>
	public string? StreamUrl { get; set; }

	/// <summary>
	///     Gets or sets platform in this rich presence.
	/// </summary>
	public string? Platform { get; set; }

	/// <summary>
	///     Gets or sets sync id in this rich presence.
	/// </summary>
	public string? SyncId { get; set; }

	/// <summary>
	///     Gets or sets session_id in this rich presence.
	/// </summary>
	public string? SessionId { get; set; }

	/// <summary>
	///     Gets or sets the activity type.
	/// </summary>
	public ActivityType ActivityType { get; set; }

	/// <summary>
	///     Gets the rich presence details, if present.
	/// </summary>
	public DiscordRichPresence? RichPresence { get; internal set; }

	/// <summary>
	///     Gets the custom status of this activity, if present.
	/// </summary>
	public DiscordCustomStatus? CustomStatus { get; internal set; }

	/// <summary>
	///     Updates a activity with an transport activity.
	/// </summary>
	/// <param name="rawActivity">The raw activity.</param>
	internal void UpdateWith(TransportActivity rawActivity)
	{
		this.Id = rawActivity?.Id;
		this.Name = rawActivity?.Name;
		this.ActivityType = rawActivity?.ActivityType ?? ActivityType.Playing;
		this.StreamUrl = rawActivity?.StreamUrl;
		this.SessionId = rawActivity?.SessionId;
		this.SyncId = rawActivity?.SyncId;
		this.Platform = rawActivity?.Platform;
		this.State = rawActivity?.State;

		if (rawActivity?.IsRichPresence() == true && this.RichPresence != null)
			this.RichPresence.UpdateWith(rawActivity);
		else this.RichPresence = rawActivity?.IsRichPresence() == true ? new DiscordRichPresence(rawActivity) : null;

		if (rawActivity?.IsCustomStatus() == true && this.CustomStatus != null)
			this.CustomStatus.UpdateWith(rawActivity.State, rawActivity.Emoji);
		else
			this.CustomStatus = rawActivity?.IsCustomStatus() == true
				? new DiscordCustomStatus
				{
					Name = rawActivity.Name!,
					State = rawActivity.State,
					Emoji = rawActivity.Emoji
				}
				: null;
	}
}

/// <summary>
///     Represents details for a custom status activity, attached to a <see cref="DiscordActivity" />.
/// </summary>
public sealed class DiscordCustomStatus
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordCustomStatus" /> class.
	/// </summary>
	internal DiscordCustomStatus()
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordCustomStatus" /> class.
	/// </summary>
	/// <param name="other">The other.</param>
	internal DiscordCustomStatus(DiscordCustomStatus other)
	{
		this.Name = other.Name;
		this.State = other.State;
		this.Emoji = other.Emoji;
	}

	/// <summary>
	///     Gets the name of this custom status.
	/// </summary>
	public string Name { get; internal set; }

	/// <summary>
	///     Gets the state of this custom status.
	/// </summary>
	public string? State { get; internal set; }

	/// <summary>
	///     Gets the emoji of this custom status, if any.
	/// </summary>
	public PartialEmoji? Emoji { get; internal set; }

	/// <summary>
	///     Updates a discord status.
	/// </summary>
	/// <param name="state">The state.</param>
	/// <param name="emoji">The emoji.</param>
	internal void UpdateWith(string? state, PartialEmoji? emoji)
	{
		this.Name = "Custom Status";
		this.State = state;
		this.Emoji = emoji;
	}
}

/// <summary>
///     Represents details for Discord rich presence, attached to a <see cref="DiscordActivity" />.
/// </summary>
public sealed class DiscordRichPresence
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordRichPresence" /> class.
	/// </summary>
	internal DiscordRichPresence()
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordRichPresence" /> class.
	/// </summary>
	/// <param name="rawGame">The raw game.</param>
	internal DiscordRichPresence(TransportActivity rawGame)
	{
		this.UpdateWith(rawGame);
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordRichPresence" /> class.
	/// </summary>
	/// <param name="other">The other.</param>
	internal DiscordRichPresence(DiscordRichPresence other)
	{
		this.Details = other.Details;
		this.State = other.State;
		this.Application = other.Application;
		this.Instance = other.Instance;
		this.LargeImageText = other.LargeImageText;
		this.SmallImageText = other.SmallImageText;
		this.LargeImage = other.LargeImage;
		this.SmallImage = other.SmallImage;
		this.CurrentPartySize = other.CurrentPartySize;
		this.MaximumPartySize = other.MaximumPartySize;
		this.PartyId = other.PartyId;
		this.Buttons = other.Buttons;
		this.StartTimestamp = other.StartTimestamp;
		this.EndTimestamp = other.EndTimestamp;
		this.JoinSecret = other.JoinSecret;
		this.MatchSecret = other.MatchSecret;
		this.SpectateSecret = other.SpectateSecret;
	}

	/// <summary>
	///     Gets the details of this presence.
	/// </summary>
	public string Details { get; internal set; }

	/// <summary>
	///     Gets the game state.
	/// </summary>
	public string State { get; internal set; }

	/// <summary>
	///     Gets the application for which the rich presence is for.
	/// </summary>
	public DiscordApplication Application { get; internal set; }

	/// <summary>
	///     Gets the instance status.
	/// </summary>
	public bool? Instance { get; internal set; }

	/// <summary>
	///     Gets the large image for the rich presence.
	/// </summary>
	public DiscordAsset LargeImage { get; internal set; }

	/// <summary>
	///     Gets the hover text for large image.
	/// </summary>
	public string LargeImageText { get; internal set; }

	/// <summary>
	///     Gets the small image for the rich presence.
	/// </summary>
	public DiscordAsset SmallImage { get; internal set; }

	/// <summary>
	///     Gets the hover text for small image.
	/// </summary>
	public string SmallImageText { get; internal set; }

	/// <summary>
	///     Gets the current party size.
	/// </summary>
	public long? CurrentPartySize { get; internal set; }

	/// <summary>
	///     Gets the maximum party size.
	/// </summary>
	public long? MaximumPartySize { get; internal set; }

	/// <summary>
	///     Gets the party ID.
	/// </summary>
	public ulong? PartyId { get; internal set; }

	/// <summary>
	///     Gets the buttons.
	/// </summary>
	public IReadOnlyList<string> Buttons { get; internal set; }

	/// <summary>
	///     Gets the game start timestamp.
	/// </summary>
	public DateTimeOffset? StartTimestamp { get; internal set; }

	/// <summary>
	///     Gets the game end timestamp.
	/// </summary>
	public DateTimeOffset? EndTimestamp { get; internal set; }

	/// <summary>
	///     Gets the secret value enabling users to join your game.
	/// </summary>
	public string JoinSecret { get; internal set; }

	/// <summary>
	///     Gets the secret value enabling users to receive notifications whenever your game state changes.
	/// </summary>
	public string MatchSecret { get; internal set; }

	/// <summary>
	///     Gets the secret value enabling users to spectate your game.
	/// </summary>
	public string SpectateSecret { get; internal set; }

	/// <summary>
	///     Updates a game activity with an transport activity.
	/// </summary>
	/// <param name="rawGame">The raw game.</param>
	internal void UpdateWith(TransportActivity rawGame)
	{
		this.Details = rawGame?.Details;
		this.State = rawGame?.State;
		this.Application = rawGame?.ApplicationId != null
			? new DiscordApplication
			{
				Id = rawGame.ApplicationId.Value
			}
			: null;
		this.Instance = rawGame?.Instance;
		this.LargeImageText = rawGame?.Assets?.LargeImageText;
		this.SmallImageText = rawGame?.Assets?.SmallImageText;
		this.CurrentPartySize = rawGame?.Party?.Size?.Current;
		this.MaximumPartySize = rawGame?.Party?.Size?.Maximum;
		if (rawGame?.Party != null && ulong.TryParse(rawGame.Party.Id, NumberStyles.Number, CultureInfo.InvariantCulture, out var partyId))
			this.PartyId = partyId;
		this.Buttons = rawGame?.Buttons;
		this.StartTimestamp = rawGame?.Timestamps?.Start;
		this.EndTimestamp = rawGame?.Timestamps?.End;
		this.JoinSecret = rawGame?.Secrets?.Join;
		this.MatchSecret = rawGame?.Secrets?.Match;
		this.SpectateSecret = rawGame?.Secrets?.Spectate;

		var lid = rawGame?.Assets?.LargeImage;
		if (lid != null)
		{
			if (lid.StartsWith("spotify:", StringComparison.Ordinal))
				this.LargeImage = new DiscordSpotifyAsset
				{
					Id = lid
				};
			else if (ulong.TryParse(lid, NumberStyles.Number, CultureInfo.InvariantCulture, out var ulid))
				this.LargeImage = new DiscordApplicationAsset
				{
					Id = lid,
					Application = this.Application,
					Type = ApplicationAssetType.LargeImage
				};
		}

		var sid = rawGame?.Assets?.SmallImage;
		if (sid != null)
		{
			if (sid.StartsWith("spotify:", StringComparison.Ordinal))
				this.SmallImage = new DiscordSpotifyAsset
				{
					Id = sid
				};
			else if (ulong.TryParse(sid, NumberStyles.Number, CultureInfo.InvariantCulture, out var usid))
				this.SmallImage = new DiscordApplicationAsset
				{
					Id = sid,
					Application = this.Application,
					Type = ApplicationAssetType.LargeImage
				};
		}
	}
}

/// <summary>
///     Determines the type of a user activity.
/// </summary>
public enum ActivityType
{
	/// <summary>
	///     Indicates the user is playing a game.
	/// </summary>
	Playing = 0,

	/// <summary>
	///     Indicates the user is streaming a game.
	/// </summary>
	Streaming = 1,

	/// <summary>
	///     Indicates the user is listening to something.
	/// </summary>
	ListeningTo = 2,

	/// <summary>
	///     Indicates the user is watching something.
	/// </summary>
	Watching = 3,

	/// <summary>
	///     Indicates the current activity is a custom status.
	/// </summary>
	Custom = 4,

	/// <summary>
	///     Indicates the user is competing in something.
	/// </summary>
	Competing = 5
}
