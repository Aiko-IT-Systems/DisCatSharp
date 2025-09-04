using System;
using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Entities;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents a game a user is playing.
/// </summary>
internal sealed class TransportActivity : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="TransportActivity" /> class.
	/// </summary>
	internal TransportActivity()
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="TransportActivity" /> class.
	/// </summary>
	/// <param name="game">The game.</param>
	internal TransportActivity(DiscordActivity game)
	{
		if (game == null)
			return;

		if (game.ActivityType == ActivityType.Custom)
		{
			this.Id = "custom";
			this.Name = "Custom Status";
			this.State = game.Name;
			this.Emoji = game.Emoji;
			this.ActivityType = game.ActivityType;
		}
		else
		{
			this.Name = game.Name;
			this.State = game.State;
			this.ActivityType = game.ActivityType;
			this.StreamUrl = game.StreamUrl;
		}
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
	public PartialEmoji? Emoji { get; internal set; }

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
	public GameParty? Party { get; internal set; }

	/// <summary>
	///     Gets or sets information about assets related to this rich presence.
	///     This is a component of the rich presence, and, as such, can only be used by regular users.
	/// </summary>
	[JsonProperty("assets", NullValueHandling = NullValueHandling.Ignore)]
	public PresenceAssets? Assets { get; internal set; }

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
	public GameTimestamps? Timestamps { get; internal set; }

	/// <summary>
	///     Gets or sets information about current game's secret values.
	///     This is a component of the rich presence, and, as such, can only be used by regular users.
	/// </summary>
	[JsonProperty("secrets", NullValueHandling = NullValueHandling.Ignore)]
	public GameSecrets? Secrets { get; internal set; }

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

	/// <summary>
	///     Represents information about assets attached to a rich presence.
	/// </summary>
	public class PresenceAssets
	{
		/// <summary>
		///     Gets the large image asset ID.
		/// </summary>
		[JsonProperty("large_image")]
		public string LargeImage { get; set; }

		/// <summary>
		///     Gets the large image text.
		/// </summary>
		[JsonProperty("large_text", NullValueHandling = NullValueHandling.Ignore)]
		public string? LargeImageText { get; internal set; }

		/// <summary>
		///     Gets the small image asset ID.
		/// </summary>
		[JsonProperty("small_image")]
		internal string SmallImage { get; set; }

		/// <summary>
		///     Gets the small image text.
		/// </summary>
		[JsonProperty("small_text", NullValueHandling = NullValueHandling.Ignore)]
		public string? SmallImageText { get; internal set; }
	}

	/// <summary>
	///     Represents information about rich presence game party.
	/// </summary>
	public class GameParty
	{
		/// <summary>
		///     Gets the game party ID.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public string? Id { get; internal set; }

		/// <summary>
		///     Gets the size of the party.
		/// </summary>
		[JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
		public GamePartySize? Size { get; internal set; }

		/// <summary>
		///     Represents information about party size.
		/// </summary>
		[JsonConverter(typeof(GamePartySizeConverter))]
		public class GamePartySize
		{
			/// <summary>
			///     Gets the current number of players in the party.
			/// </summary>
			public long Current { get; internal set; }

			/// <summary>
			///     Gets the maximum party size.
			/// </summary>
			public long Maximum { get; internal set; }
		}
	}

	/// <summary>
	///     Represents information about the game state's timestamps.
	/// </summary>
	public class GameTimestamps
	{
		[JsonProperty("end", NullValueHandling = NullValueHandling.Ignore)]
		internal long? EndInternal;

		[JsonProperty("start", NullValueHandling = NullValueHandling.Ignore)]
		internal long? StartInternal;

		/// <summary>
		///     Gets the time the game has started.
		/// </summary>
		[JsonIgnore]
		public DateTimeOffset? Start
			=> this.StartInternal != null ? Utilities.GetDateTimeOffsetFromMilliseconds(this.StartInternal.Value, false) : null;

		/// <summary>
		///     Gets the time the game is going to end.
		/// </summary>
		[JsonIgnore]
		public DateTimeOffset? End
			=> this.EndInternal != null ? Utilities.GetDateTimeOffsetFromMilliseconds(this.EndInternal.Value, false) : null;
	}

	/// <summary>
	///     Represents information about secret values for the Join, Spectate, and Match actions.
	/// </summary>
	public class GameSecrets
	{
		/// <summary>
		///     Gets the secret value for join action.
		/// </summary>
		[JsonProperty("join", NullValueHandling = NullValueHandling.Ignore)]
		public string? Join { get; internal set; }

		/// <summary>
		///     Gets the secret value for match action.
		/// </summary>
		[JsonProperty("match", NullValueHandling = NullValueHandling.Ignore)]
		public string? Match { get; internal set; }

		/// <summary>
		///     Gets the secret value for spectate action.
		/// </summary>
		[JsonProperty("spectate", NullValueHandling = NullValueHandling.Ignore)]
		public string? Spectate { get; internal set; }
	}
}

/// <summary>
///     Represents a game party size converter.
/// </summary>
internal sealed class GamePartySizeConverter : JsonConverter
{
	/// <summary>
	///     Writes the json.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The serializer.</param>
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		var obj = value is TransportActivity.GameParty.GamePartySize sinfo
			? new object[] { sinfo.Current, sinfo.Maximum }
			: null;
		serializer.Serialize(writer, obj);
	}

	/// <summary>
	///     Reads the json.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="objectType">The object type.</param>
	/// <param name="existingValue">The existing value.</param>
	/// <param name="serializer">The serializer.</param>
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		var arr = this.ReadArrayObject(reader, serializer);
		return new TransportActivity.GameParty.GamePartySize
		{
			Current = (long)arr[0],
			Maximum = (long)arr[1]
		};
	}

	/// <summary>
	///     Reads the array object.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="serializer">The serializer.</param>
	private JArray ReadArrayObject(JsonReader reader, JsonSerializer serializer) =>
		serializer.Deserialize<JToken>(reader) is not JArray arr || arr.Count != 2
			? throw new JsonSerializationException("Expected array of length 2")
			: arr;

	/// <summary>
	///     Whether it can convert.
	/// </summary>
	/// <param name="objectType">The object type.</param>
	public override bool CanConvert(Type objectType) => objectType == typeof(TransportActivity.GameParty.GamePartySize);
}
