using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord clan.
/// </summary>
public sealed class DiscordClan
{
	/// <summary>
	/// Gets the identity guild id.
	/// </summary>
	[JsonProperty("identity_guild_id")]
	public ulong IdentityGuildId { get; internal set; }

	/// <summary>
	/// Gets whether the identity is enabled and shown to everyone.
	/// </summary>
	[JsonProperty("identity_enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool IdentityEnabled { get; internal set; }

	/// <summary>
	/// Gets the clan tag.
	/// </summary>
	[JsonProperty("tag")]
	public string Tag { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordClan"/> class.
	/// </summary>
	internal DiscordClan()
	{ }
}
