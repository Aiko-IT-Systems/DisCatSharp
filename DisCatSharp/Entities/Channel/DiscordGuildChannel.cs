using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a base class for all guild channels.
/// </summary>
public abstract class DiscordGuildChannel : BaseDiscordChannel
{
	/// <summary>
	/// Gets the guild id this channel belongs to.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	/// <summary>
	/// Gets the channel name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the channel position.
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int? Position { get; internal set; }

	/// <summary>
	/// Gets the permission overwrites for this channel.
	/// </summary>
	[JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
	public object[] PermissionOverwrites { get; internal set; } // Replace object with actual type

	/// <summary>
	/// Gets the parent category id.
	/// </summary>
	[JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ParentId { get; internal set; }

	/// <summary>
	/// Gets the channel flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public int? Flags { get; internal set; }
}
