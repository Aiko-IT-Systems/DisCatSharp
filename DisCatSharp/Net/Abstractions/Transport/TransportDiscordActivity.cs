using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents a game a user is playing.
/// </summary>
public sealed class TransportDiscordActivity : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="TransportDiscordActivity" /> class.
	/// </summary>
	internal TransportDiscordActivity()
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="TransportDiscordActivity" /> class.
	/// </summary>
	/// <param name="game">The game.</param>
	internal TransportDiscordActivity(DiscordActivity game)
	{
		if (game == null)
			return;

		if (game.ActivityType == ActivityType.Custom)
			this.Id = "custom";
		this.Name = game.Name;
		this.State = game.State;
		this.ActivityType = game.ActivityType;
		this.StreamUrl = game.StreamUrl;
	}

	/// <summary>
	///     Gets or sets the id of user's activity.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public string? Id { get; internal set; }

	/// <summary>
	///     Gets or sets the name of the game the user is playing.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
	public string? Name { get; internal set; }

	/// <summary>
	///     Gets or sets the stream URI, if applicable.
	/// </summary>
	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public string? StreamUrl { get; internal set; }

	/// <summary>
	///     Gets or sets the livestream type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ActivityType ActivityType { get; internal set; }

	/// <summary>
	///     Gets or sets the details.
	///     <para>This is a component of the rich presence, and, as such, can only be used by regular users.</para>
	/// </summary>
	[JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
	public string? Details { get; internal set; }

	/// <summary>
	///     Gets or sets game state.
	///     <para>This is a component of the rich presence, and, as such, can only be used by regular users.</para>
	/// </summary>
	[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
	public string? State { get; internal set; }

	/// <summary>
	///     Gets the emoji details for a custom status, if any.
	/// </summary>
	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmoji? Emoji { get; internal set; }

	/// <summary>
	///     Gets ID of the application for which this rich presence is for.
	///     This is a component of the rich presence, and, as such, can only be used by regular users.
	/// </summary>
	[JsonIgnore]
	public ulong? ApplicationId
	{
		get => this.ApplicationIdStr != null ? ulong.Parse(this.ApplicationIdStr, CultureInfo.InvariantCulture) : null;
		internal set => this.ApplicationIdStr = value?.ToString(CultureInfo.InvariantCulture);
	}

	/// <summary>
	///     Gets or sets the application id string.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	internal string? ApplicationIdStr { get; set; }

	/// <summary>
	///     Gets or sets instance status.
	///     This is a component of the rich presence, and, as such, can only be used by regular users.
	/// </summary>
	[JsonProperty("instance", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Instance { get; internal set; }

	/// <summary>
	///     Gets or sets information about the current game's party.
	///     This is a component of the rich presence, and, as such, can only be used by regular users.
	/// </summary>
	[JsonProperty("party", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordGameParty? Party { get; internal set; }

	/// <summary>
	///     Gets or sets information about assets related to this rich presence.
	///     This is a component of the rich presence, and, as such, can only be used by regular users.
	/// </summary>
	[JsonProperty("assets", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordPresenceAssets? Assets { get; internal set; }

	/// <summary>
	///     Gets or sets information about buttons in this rich presence.
	///     This is a component of the rich presence, and, as such, can only be used by regular users.
	/// </summary>
	[JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<string>? Buttons { get; internal set; }

	/// <summary>
	///     Gets or sets platform in this rich presence.
	///     This is a component of the rich presence, and, as such, can only be used by regular users.
	/// </summary>
	[JsonProperty("platform", NullValueHandling = NullValueHandling.Ignore)]
	public string? Platform { get; internal set; }

	/// <summary>
	///     Gets or sets sync_id in this rich presence.
	///     This is a component of the rich presence, and, as such, can only be used by regular users.
	/// </summary>
	[JsonProperty("sync_id", NullValueHandling = NullValueHandling.Ignore)]
	public string? SyncId { get; internal set; }

	/// <summary>
	///     Gets or sets session_id in this rich presence.
	///     This is a component of the rich presence, and, as such, can only be used by regular users.
	/// </summary>
	[JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
	public string? SessionId { get; internal set; }

	/// <summary>
	///     Gets or sets information about current game's timestamps.
	///     This is a component of the rich presence, and, as such, can only be used by regular users.
	/// </summary>
	[JsonProperty("timestamps", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordGameTimestamps? Timestamps { get; internal set; }

	/// <summary>
	///     Gets or sets information about current game's secret values.
	///     This is a component of the rich presence, and, as such, can only be used by regular users.
	/// </summary>
	[JsonProperty("secrets", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordGameSecrets? Secrets { get; internal set; }

	/// <summary>
	///     Whether this activity is a rich presence.
	/// </summary>
	public bool IsRichPresence()
		=> this.Details != null || this.State != null || this.ApplicationId != null || this.Instance != null || this.Party != null || this.Assets != null || this.Secrets != null || this.Timestamps != null || this.Buttons != null;

	/// <summary>
	///     Whether this activity is a custom status.
	/// </summary>
	public bool IsCustomStatus()
		=> this.Name == "Custom Status";
}
