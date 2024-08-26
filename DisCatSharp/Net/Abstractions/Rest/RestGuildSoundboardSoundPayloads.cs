using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents a payload for creating a soundboard sound.
/// </summary>
internal sealed class RestSoundboardSoundCreatePayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the name of the soundboard sound.
	/// </summary>
	[JsonProperty("name")]
	public required string Name { get; set; }

	/// <summary>
	///     Gets or sets the sound data.
	/// </summary>
	[JsonProperty("sound")]
	public required string Sound { get; set; }

	/// <summary>
	///     Gets or sets the volume of the soundboard sound (0 to 1).
	/// </summary>
	[JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
	public double? Volume { get; set; }

	/// <summary>
	///     Gets or sets the emoji id for the soundboard sound's custom emoji.
	/// </summary>
	[JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? EmojiId { get; set; }

	/// <summary>
	///     Gets or sets the emoji name for the soundboard sound's standard emoji.
	/// </summary>
	[JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Ignore)]
	public string? EmojiName { get; set; }
}

/// <summary>
///     Represents a payload for modifying a soundboard sound.
/// </summary>
internal sealed class RestSoundboardSoundModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the name of the soundboard sound.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Name { get; set; }

	/// <summary>
	///     Gets or sets the volume of the soundboard sound (0 to 1).
	/// </summary>
	[JsonProperty("volume", NullValueHandling = NullValueHandling.Include)]
	public Optional<double?> Volume { get; set; }

	/// <summary>
	///     Gets or sets the emoji id for the soundboard sound's custom emoji.
	/// </summary>
	[JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Include)]
	public Optional<ulong?> EmojiId { get; set; }

	/// <summary>
	///     Gets or sets the emoji name for the soundboard sound's standard emoji.
	/// </summary>
	[JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> EmojiName { get; set; }
}

/// <summary>
///     Represents a payload for sending a soundboard sound to a voice channel.
/// </summary>
internal sealed class RestSendSoundboardSoundPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the ID of the soundboard sound to play.
	/// </summary>
	[JsonProperty("sound_id")]
	public ulong SoundId { get; set; }

	/// <summary>
	///     Gets or sets the ID of the guild the soundboard sound is from.
	///     This field is required when the sound is from a different server.
	/// </summary>
	[JsonProperty("source_guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ulong?> SourceGuildId { get; set; }
}
